using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
            dataFactory.Value.WithDataManager(dm => dm.WithRepository<IDataRepository>(repo => repo.Clear()));
        }

        public Boolean IsStarted()
        {
            return state != null;
        }
        public async Task StartRead()
        {
            if (IsStarted())
            {
                logger.Value.Info("Read is already run");
                return;
            }

            logger.Value.Info("Read has started");
            try
            {
                state = await Task.Run(() => InitState()).ConfigureAwait(false);

                state.ReadTask = Task.WhenAll(
                    DataReadTask(), DataChunkTask(),
                    DataSaveTask(), UpdateSessionTask());

                await state.ReadTask;
                logger.Value.Info("Read has completed");
            }
            catch (OperationCanceledException)
            {
                logger.Value.Info("Read has been cancelled");
            }
            catch(Exception ex)
            {
                logger.Value.Info("Read has failed");
                logger.Value.Error(ex.Message, ex);

                BreakCurrentState().Wait();
            }
        }
        public async Task StopRead()
        {
            try
            {
                await BreakCurrentState();
                logger.Value.Info("Read has been stopped");
            }
            catch (Exception ex)
            {
                logger.Value.Info("Read stop has failed");
                logger.Value.Error(ex.Message, ex);
            }
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

        private async Task DataReadTask()
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                logger.Value.Trace("DataRead has started");

                await Task.Run(() => DataRead(state.TokenSource.Token, state.Session), state.TokenSource.Token);

                sw.Stop();
                logger.Value.TraceFormat("DataRead has sccessfully completed, elapsed {0}", sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                logger.Value.TraceFormat("DataRead has been cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Value.TraceFormat("DataRead has failed");
                logger.Value.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                lock (critical)
                {
                    if (state != null)
                        state.Session.RawDataCollection.CompleteAdding();
                }
            }
        }
        private async Task DataChunkTask()
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                logger.Value.Trace("DataChunk has started");

                await Task.Run(() => DataChunk(state.TokenSource.Token, state.Session), state.TokenSource.Token);

                sw.Stop();
                logger.Value.TraceFormat("DataChunk has sccessfully completed, elapsed {0}", sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                logger.Value.TraceFormat("DataChunk has been cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Value.TraceFormat("DataChunk has failed");
                logger.Value.Error(ex.Message, ex);
                state.Session.RawDataCollection.CompleteAdding();
                throw;
            }
            finally
            {
                lock (critical)
                {
                    if (state != null)
                        state.Session.ChunkDataCollection.CompleteAdding();
                }
            }
        }
        private async Task DataSaveTask()
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                logger.Value.Trace("DataSave has started");

                await Task.Run(() => DataSave(state.TokenSource.Token, state.Session), state.TokenSource.Token);

                sw.Stop();
                logger.Value.TraceFormat("DataSave has sccessfully completed, elapsed {0}", sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                logger.Value.TraceFormat("DataSave has been cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Value.TraceFormat("DataSave has failed");
                logger.Value.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                lock (critical)
                {
                    if (state != null)
                        state.Session.ChunkDataCollection.CompleteAdding();
                }
            }
        }
        private async Task UpdateSessionTask()
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                logger.Value.Trace("Update read session has started");

                await Task.Run(() => UpdateSession(state.TokenSource.Token, state.Session), state.TokenSource.Token);

                sw.Stop();
                logger.Value.TraceFormat("Update read session has sccessfully completed, elapsed {0}", sw.Elapsed);
            }
            catch (OperationCanceledException)
            {
                logger.Value.TraceFormat("Update read session has been cancelled");
                throw;
            }
            catch (Exception ex)
            {
                logger.Value.TraceFormat("Update read session has failed");
                logger.Value.Error(ex.Message, ex);
                throw;
            }
        }

        private async Task BreakCurrentState()
        {
            Task task;
            lock (critical)
            {
                if (state == null)
                    return;

                state.Session.Completed = true;
                state.TokenSource.Cancel();

                task = state.ReadTask;
                state = null;
            }

            try
            {
                await task;
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Value.Info("Break read is failed");
                logger.Value.Error(ex.Message, ex);
            }
        }

        private void DataRead(CancellationToken token, ReadSession session)
        {
            using (var stream = File.OpenRead(readerConfig.Value.FileName))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CsvConfiguration {Delimiter = " "}))
            {
                while (csv.Read() && !session.Completed)
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

                        if (token.WaitHandle.WaitOne(MathExtension.Min(TimeSpan.FromSeconds(1),
                                session.FutureRealTime(rawData.Timestamp))))
                            break;
                    }

                    session.RawDataCollection.Add(rawData, token);
                }
            }
        }
        private void DataChunk(CancellationToken token, ReadSession session)
        {
            var chunk = new List<Data>();
            var readTime = session.StopWatch();
            var realTime = DateTime.Now.TimeOfDay;

            foreach (var item in session.RawDataCollection.GetConsumingEnumerable(token))
            {
                if (item.Timestamp != readTime)
                {
                    if (!chunk.Any())
                        realTime = DateTime.Now.TimeOfDay;

                    if (DateTime.Now.TimeOfDay - realTime > TimeSpan.FromSeconds(1) && (
                        session.ChunkDataCollection.Count < 10 || chunk.Count > 20000))
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

            if (chunk.Any())
                session.ChunkDataCollection.Add(chunk, token); 
        }
        private void DataSave(CancellationToken token, ReadSession session)
        {
            foreach (var data in session.ChunkDataCollection.GetConsumingEnumerable(token))
            {               
                var sw = new Stopwatch();
                sw.Start();

                var chunk = data;
                dataFactory.Value.WithRepository<IDataRepository>(repo => repo.Save(chunk));
                
                sw.Stop();
                logger.Value.TraceFormat("DataSave: {0} items saved, elapsed {1}", chunk.Count, sw.Elapsed);

                SetReadTime(session, chunk.Last().Timestamp);
            }

            session.Completed = true;
            if (session.AllTime.HasValue && session.ReadTime != session.AllTime)
                SetReadTime(session, session.AllTime.Value);
        }
        private void UpdateSession(CancellationToken token, ReadSession session)
        {
            while (!session.Completed)
            {
                token.ThrowIfCancellationRequested();

                if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30)))
                    break;

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
                Interval = row[5],
                Received = row[8],
                LinkFaults = row[9],
                Lost = row[12],
                Restored = row[13],
                Overflow = row[14],
                Underflow = row[15],
                DelayFactor = row[16],
                MediaLossRate = row[17]
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
                StreamType = rawData.StreamType.GetNullableElementByCode<StreamType>(),                
            };

            if (data.MessageType == null && !String.IsNullOrEmpty(rawData.MessageType))
                logger.Value.WarnFormat("Message type {0} is unknown", rawData.MessageType);

            if (data.StreamType == null && !String.IsNullOrEmpty(rawData.StreamType))
                logger.Value.WarnFormat("Stream type {0} is unknown", rawData.StreamType);
            
            const String intervalPattern = @"(?<mm>\d+):(?<ss>\d+).(?<fff>\d+)";
            var match = Regex.Match(rawData.Interval, intervalPattern);
            Int32 minutes;
            Int32 seconds;
            Int32 milliseconds;
            if (!match.Success ||
                !Int32.TryParse(match.Groups["mm"].Value, out minutes) ||
                !Int32.TryParse(match.Groups["ss"].Value, out seconds) ||
                !Int32.TryParse(match.Groups["fff"].Value, out milliseconds))
                logger.Value.WarnFormat("Interval '{0}' parsing fail", rawData.Interval);
            
            else
            {
                var interval = new TimeSpan(minutes/(60*24), minutes/60, minutes%60, seconds, milliseconds);

                Int32 received;
                if (!Int32.TryParse(rawData.Received, out received))
                    logger.Value.WarnFormat("Received '{0}' parsing fail", rawData.Received);
                else if (received > 0 && interval.TotalSeconds > 0)
                    data.ReceivedRate = received / interval.TotalSeconds;

                Int32 faults;
                if (!Int32.TryParse(rawData.LinkFaults, out faults))
                    logger.Value.WarnFormat("LinkFaults '{0}' parsing fail", rawData.LinkFaults);
                else if (faults >= 0 && interval.TotalSeconds > 0)
                    data.LinkFaultsRate = faults / interval.TotalSeconds;

                Int32 lost;
                if (!Int32.TryParse(rawData.Lost, out lost))
                    logger.Value.WarnFormat("Lost '{0}' parsing fail", rawData.Lost);
                else if (lost >= 0 && interval.TotalSeconds > 0)
                    data.LostRate = lost / interval.TotalSeconds;

                Int32 restored;
                if (!Int32.TryParse(rawData.Restored, out restored))
                    logger.Value.WarnFormat("Restored '{0}' parsing fail", rawData.Restored);
                else if (received >= 0 && interval.TotalSeconds > 0)
                    data.RestoredRate = restored / interval.TotalSeconds;

                Int32 overflow;
                if (!Int32.TryParse(rawData.Overflow, out overflow))
                    logger.Value.WarnFormat("Overflow '{0}' parsing fail", rawData.Overflow);
                else if (overflow >= 0 && interval.TotalSeconds > 0)
                    data.OverflowRate = overflow / interval.TotalSeconds;

                Int32 underflow;
                if (!Int32.TryParse(rawData.Underflow, out underflow))
                    logger.Value.WarnFormat("Underflow '{0}' parsing fail", rawData.Underflow);
                else if (underflow >= 0 && interval.TotalSeconds > 0)
                    data.UnderflowRate = underflow / interval.TotalSeconds;

                Int32 delayFactor;
                if (!Int32.TryParse(rawData.DelayFactor, out delayFactor))
                    logger.Value.WarnFormat("DelayFactor '{0}' parsing fail", rawData.DelayFactor);
                else if (delayFactor >= 0)
                    data.DelayFactor = delayFactor;

                Int32 lossRate;
                if (!Int32.TryParse(rawData.MediaLossRate, out lossRate))
                    logger.Value.WarnFormat("MediaLossRate '{0}' parsing fail", rawData.MediaLossRate);
                else if (lossRate >= 0)
                    data.MediaLossRate = lossRate;
            }

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

            public Boolean Completed { get; set; }

            public ReadSession()
            {
                Completed = false;
                RawDataCollection = new BlockingCollection<RawData>(500000);
                ChunkDataCollection = new BlockingCollection<IList<Data>>(50);
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
        }
        private class RawData
        {
            public TimeSpan Timestamp { get; set; }

            public String Mac { get; set; }
            public String Date { get; set; }
            public String Time { get; set; }

            public String MessageType { get; set; }
            public String StreamType { get; set; }
            public String Interval { get; set; }
            public String Received { get; set; }
            public String LinkFaults { get; set; }
            public String Lost { get; set; }
            public String Restored { get; set; }
            public String Overflow { get; set; }
            public String Underflow { get; set; }
            public String DelayFactor { get; set; }
            public String MediaLossRate { get; set; }
        }
    }
}