using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using CsvHelper;
using CsvHelper.Configuration;
using Logic.Common;
using Logic.Dal;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Facades
{
    public class EmulatorFacade
    {
        readonly DateTime minDate = new DateTime(2014, 11, 17, 0, 27, 0);

        private readonly Lazy<IDataManagerFactrory> dataFactory;
        private readonly Lazy<IEmulatorConfig> readerConfig;
        private readonly Lazy<ILog> logger;
 
        private EmulatorState state;
        private readonly Object critical = new Object();

        public EmulatorFacade(Func<IDataManagerFactrory> dataFactory, 
            Func<IEmulatorConfig> readerConfig,
            Func<ILoggerFactoryAdapter> loggerAdapter)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
            this.readerConfig = new Lazy<IEmulatorConfig>(readerConfig);
            logger = new Lazy<ILog>(() => loggerAdapter().GetLogger(GetType()));
        }

        public void Clear()
        {
            dataFactory.Value.WithDataManager(dm =>
            {
                dm.WithRepository<ISettingsRepository>(repo => repo.SetReadTime(null));
                dm.WithRepository<IDataRepository>(repo => repo.Clear());
            }, true);
        }

        public async Task StartRead()
        {
            if (state != null)
            {
                logger.Value.Info("Read is already run");
                return;
            }

            logger.Value.Info("Read is started");
            try
            {
                state = await Task.Run(() => InitState());

                state.ReadTask = Task.WhenAll(
                    Task.Run(() => DataRead(state.TokenSource.Token, state.Session), state.TokenSource.Token),
                    Task.Run(() => DataChunk(state.TokenSource.Token, state.Session), state.TokenSource.Token),
                    Task.Run(() => DataSave(state.TokenSource.Token, state.Session), state.TokenSource.Token),
                    Task.Run(() => UpdateSession(state.TokenSource.Token, state.Session), state.TokenSource.Token));

                await state.ReadTask;

                logger.Value.Info("Read is completed");
            }
            catch (OperationCanceledException)
            {
                logger.Value.Info("Read is cancelled");
            }
            catch(Exception ex)
            {
                BreakCurrentState().Wait();
                logger.Value.Error(ex.Message, ex);
                throw;
            }
        }
        public async Task StopRead()
        {
            await BreakCurrentState();
            logger.Value.Info("Read has been stopped");
        }

        public TimeSpan? GetReadTime()
        {
            return state != null
                ? state.Session.ReadTime
                : dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(repo => repo.GetReadTime());
        }

        public Double GetVelocity()
        {
            return state != null
                ? state.Session.Velocity
                : dataFactory.Value.WithRepository<Double, ISettingsRepository>(repo => repo.GetReadVelocity() ?? 1.0);
        }
        public void SetVelocity(Double velocity)
        {
            if (state != null)
                state.Session.Velocity = velocity;

            dataFactory.Value.WithRepository<ISettingsRepository>(repo => repo.SetReadVelocity(velocity), true);
            logger.Value.TraceFormat("ReadVelocity has been changed to {0}", velocity);
        }

        public TimeSpan? GetAllTime()
        {
            return state != null
                ? state.Session.AllTime
                : dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(repo => repo.GetAllTime());
        }
        public void SetAllTime(TimeSpan time)
        {
            if (state != null)
                state.Session.AllTime = time;

            dataFactory.Value.WithRepository<ISettingsRepository>(repo => repo.SetAllTime(time), true);
            logger.Value.TraceFormat("AllTime has been changed to {0}", time);
        }

        private EmulatorState InitState()
        {
            lock (critical)
            {
                var newState = new EmulatorState();                

                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    newState.Session.ReadTime = repo.GetReadTime() ?? -TimeSpan.FromSeconds(1);
                    newState.Session.UpdateStopWatch(newState.Session.ReadTime);

                    newState.Session.AllTime = repo.GetAllTime();
                    newState.Session.Velocity = repo.GetReadVelocity() ?? 1.0;
                });

                logger.Value.InfoFormat("Init read session: AllTime: {0}\tReadTime: {1}\tReadVelocity:{2}",
                    newState.Session.AllTime, newState.Session.ReadTime, newState.Session.Velocity);

                return newState;
            }
        }

        private void DataRead(CancellationToken token, ReadSession session)
        {
            var sw = new Stopwatch();

            using (var stream = File.OpenRead(readerConfig.Value.FileName))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CsvConfiguration {Delimiter = " "}))
            {
                sw.Start();
                logger.Value.Trace("DataRead started");

                while (csv.Read() && !session.IsComplete)
                {
                    token.ThrowIfCancellationRequested();

                    var rawData = GetRawData(csv);
                    if (session.Finish(rawData.Timestamp))
                        break;

                    if (session.InPast(rawData.Timestamp))
                        continue;

                    while (session.InFuture(rawData.Timestamp))
                    {
                        if (!session.ChunkDataCollection.Any() && !session.RawDataCollection.Any())                        
                            SetReadTime(session, session.StopWatch());

                        if (token.WaitHandle.WaitOne(ReadSession.Min(TimeSpan.FromSeconds(1), session.FutureRealTime(rawData.Timestamp))))
                            break;
                    }

                    session.RawDataCollection.Add(rawData, token);
                }
            }

            token.ThrowIfCancellationRequested();
            session.RawDataCollection.CompleteAdding();

            sw.Stop();
            logger.Value.TraceFormat("DataRead completed. Elapsed {0}", sw.Elapsed);
        }
        private void DataChunk(CancellationToken token, ReadSession session)
        {
            var sw = new Stopwatch();
            logger.Value.Trace("DataChunk started");

            var chunk = new List<Data>();
            var readTime = session.StopWatch();
            var realTime = DateTime.Now.TimeOfDay;

            foreach (var item in session.RawDataCollection.GetConsumingEnumerable(token))
            {
                sw.Start();

                if (item.Timestamp != readTime)
                {
                    if (!chunk.Any())
                        realTime = DateTime.Now.TimeOfDay;

                    if (DateTime.Now.TimeOfDay - realTime > TimeSpan.FromSeconds(1))
                    {
                        realTime = DateTime.Now.TimeOfDay;
                        session.ChunkDataCollection.Add(chunk.ToList(), token);
                        chunk.Clear();
                    }

                    readTime = item.Timestamp;
                }

                var data = GetData(item);
                chunk.Add(data);
            }

            token.ThrowIfCancellationRequested();

            if (chunk.Any())
                session.ChunkDataCollection.Add(chunk, token);

            session.ChunkDataCollection.CompleteAdding();

            sw.Stop();
            logger.Value.TraceFormat("DataChunk completed. Elapsed {0}", sw.Elapsed);
        }
        private void DataSave(CancellationToken token, ReadSession session)
        {
            var sw = new Stopwatch();
            logger.Value.Trace("DataSave started");

            foreach (var data in session.ChunkDataCollection.GetConsumingEnumerable(token))
            {
                sw.Start();

                dataFactory.Value.WithRepository<IDataRepository>(repo => repo.Save(data));
                logger.Value.TraceFormat("DataSave: {0} items saved", data.Count);

                SetReadTime(session, data.Last().Timestamp);
            }

            token.ThrowIfCancellationRequested();

            session.IsComplete = true;
            if (session.AllTime.HasValue && session.ReadTime != session.AllTime)
                SetReadTime(session, session.AllTime.Value);

            sw.Stop();
            logger.Value.TraceFormat("DataSave completed. Elapsed {0}", sw.Elapsed);
        }
        private void UpdateSession(CancellationToken token, ReadSession session)
        {
            logger.Value.Trace("UpdateSession started");

            while (!session.IsComplete)
            {
                token.ThrowIfCancellationRequested();

                if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30)))
                    break;

                logger.Value.Trace("Update session");

                var prev = new {session.AllTime, session.Velocity};
                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    session.AllTime = repo.GetAllTime();
                    session.Velocity = repo.GetReadVelocity() ?? 1.0;
                });

                if (prev.AllTime != session.AllTime || Math.Abs(prev.Velocity - session.Velocity) > 1e-10)
                    logger.Value.InfoFormat("Read settings has been changed: AllTime={0}\tVelocity={1}", session.AllTime,
                        session.Velocity);
            }

            token.ThrowIfCancellationRequested();
            logger.Value.Trace("UpdateSession completed");
        }
        private async Task BreakCurrentState()
        {
            Task task;
            lock (critical)
            {
                if (state == null)
                    return;

                state.Session.IsComplete = true;
                state.TokenSource.Cancel();

                task = state.ReadTask;
                state = null;
            }

            try
            {
                await task;
            }
            catch (OperationCanceledException) { }
        }

        private void SetReadTime(ReadSession session, TimeSpan readTime)
        {
            dataFactory.Value.WithRepository<ISettingsRepository>(repo => repo.SetReadTime(readTime), true);
            session.ReadTime = readTime;
            logger.Value.TraceFormat("ReadTime: moved to {0}\tstopWatch: {1}", readTime, session.StopWatch());
        }
        private RawData GetRawData(ICsvReaderRow row)
        {
            var rawData = new RawData
            {
                Mac = row[6],
                Date = row[22],
                Time = row[23],

                MessageType = row[1],
                StreamType = row[2],
            };

            rawData.Timestamp = DateTime.Parse(rawData.Date) + TimeSpan.Parse(rawData.Time) - minDate;
            return rawData;
        }
        private Data GetData(RawData rawData)
        {
            var data = new Data
            {
                Timestamp = rawData.Timestamp,
                Mac = rawData.Mac,
                MessageType = rawData.MessageType.GetNullableElementByCode<MessageType>(),
                ContentType = rawData.StreamType.GetNullableElementByCode<ContentType>(),
            };

            if (data.MessageType == null && !String.IsNullOrEmpty(rawData.MessageType))
                logger.Value.WarnFormat("Message type {0} is unknown", rawData.MessageType);

            if (data.ContentType == null && !String.IsNullOrEmpty(rawData.StreamType))
                logger.Value.WarnFormat("Content type {0} is unknown", rawData.StreamType);

            return data;
        }

        private class EmulatorState
        {
            public ReadSession Session { get; private set; }
            public CancellationTokenSource TokenSource { get; private set; }
            public Task ReadTask { get; set; }

            public EmulatorState()
            {
                Session = new ReadSession();
                TokenSource = new CancellationTokenSource();
            }
        }
        private class ReadSession
        {
            public BlockingCollection<RawData> RawDataCollection { get; private set; }
            public BlockingCollection<IList<Data>> ChunkDataCollection { get; private set; }

            public TimeSpan ReadTime { get; set; }
            public TimeSpan? AllTime { get; set; }

            private TimeSpan elapsed;
            private DateTime start;

            private Double velocity;
            private readonly Object stopWatchCritical = new Object();

            public Double Velocity
            {
                get { return velocity; }
                set
                {
                    if (Math.Abs(velocity - value) < 1e-3)
                        return;

                    lock (stopWatchCritical)
                    {
                        elapsed = StopWatch();
                        start = DateTime.Now;
                        velocity = value;
                    }
                }
            }

            public Boolean IsComplete { get; set; }

            public ReadSession()
            {
                IsComplete = false;
                RawDataCollection = new BlockingCollection<RawData>();
                ChunkDataCollection = new BlockingCollection<IList<Data>>();
                start = DateTime.Now;
                elapsed = TimeSpan.Zero;
            }

            public TimeSpan StopWatch()
            {
                lock (stopWatchCritical)
                {
                    return elapsed + TimeSpan.FromSeconds((DateTime.Now - start).TotalSeconds*Velocity);
                }
            }
            public void UpdateStopWatch(TimeSpan elapsedTime)
            {
                lock (stopWatchCritical)
                {
                    elapsed = elapsedTime;
                    start = DateTime.Now;
                }
            }

            public TimeSpan FutureRealTime(TimeSpan dateTime)
            {
                // увеличиваем время с запасом, чтобы накопились сообщения для обработки
                var delay = TimeSpan.FromSeconds(5*Velocity);
                var sessionTime = dateTime - StopWatch();

                //масштабирование к реальному времени
                return TimeSpan.FromSeconds(sessionTime.TotalSeconds/Velocity) + delay;
            }

            public Boolean InPast(TimeSpan dateTime)
            {
                return dateTime <= ReadTime;
            }
            public Boolean InFuture(TimeSpan dateTime)
            {
                // Сокращаем накопление в delay сек
                var delay = TimeSpan.FromSeconds(5*Velocity);
                return FutureRealTime(dateTime) > TimeSpan.Zero + delay;
            }
            public Boolean Finish(TimeSpan dateTime)
            {
                if (!AllTime.HasValue)
                    return false;

                return dateTime > AllTime.Value || ReadTime == AllTime;
            }

            public static TimeSpan Min(TimeSpan t1, TimeSpan t2)
            {
                return t1 <= t2 ? t1 : t2;
            }
        }
        private class RawData
        {
            public TimeSpan Timestamp { get; set; }

            public String Mac { get; set; }
            public String Date { get; set; }
            public String Time { get; set; }

            public String MessageType { get; set; }
            public String StreamType { get; set; }
        }
    }
}