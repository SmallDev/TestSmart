using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Logic.Dal;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Facades
{
    public class EmulatorFacade
    {
        private readonly Lazy<IDataManagerFactrory> dataFactory;
        private readonly Lazy<IEmulatorConfig> readerConfig;

        private EmulatorState state;
        private readonly Object critical = new Object();

        public EmulatorFacade(Func<IDataManagerFactrory> dataFactory, Func<IEmulatorConfig> readerConfig)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
            this.readerConfig = new Lazy<IEmulatorConfig>(readerConfig);
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
            try
            {
                await Task.Run(() => InitState());

                await Task.WhenAll(
                    Task.Run(() => DataRead(state.TokenSource.Token, state.Session), state.TokenSource.Token),
                    Task.Run(() => DataChunk(state.TokenSource.Token, state.Session), state.TokenSource.Token),
                    Task.Run(() => DataSave(state.TokenSource.Token, state.Session), state.TokenSource.Token),
                    Task.Run(() => UpdateSession(state.TokenSource.Token, state.Session), state.TokenSource.Token));
            }
            catch
            {
                state.TokenSource.Cancel();
                throw;
            }
            finally
            {
                state.Session.IsComplete = true;                
            }
        }
        public async Task StopRead()
        {
            await Task.Run(() => BreakCurrentState());
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
        }

        private void InitState()
        {
            lock (critical)
            {
                BreakCurrentState();

                var newState = new EmulatorState();                

                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    newState.Session.ReadTime = repo.GetReadTime() ?? -TimeSpan.FromSeconds(1);
                    newState.Session.AllTime = repo.GetAllTime();
                    newState.Session.Velocity = repo.GetReadVelocity() ?? 1.0;
                });

                state = newState;
            }
        }

        private void DataRead(CancellationToken token, ReadSession session)
        {
            using (var stream = File.OpenRead(readerConfig.Value.FileName))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CsvConfiguration {Delimiter = " "}))
            {
                while (csv.Read() && !session.IsComplete)
                {
                    token.ThrowIfCancellationRequested();

                    var rawData = GetRawData(csv);
                    if (session.Finish(rawData.Timestamp))
                        break;
                    if (session.InPast(rawData.Timestamp))
                        continue;
                    if (session.InFuture(rawData.Timestamp))
                        Thread.Sleep(session.FutureRealTime(rawData.Timestamp));

                    session.RawData.Add(rawData, token);
                }
            }

            session.RawData.CompleteAdding();
        }
        private void DataChunk(CancellationToken token, ReadSession session)
        {
            var chunk = new List<Data>();
            var readTime = session.StopWatch();
            var realTime = DateTime.Now.TimeOfDay;

            foreach (var item in session.RawData.GetConsumingEnumerable(token))
            {
                token.ThrowIfCancellationRequested();

                if (session.TimeShift(item.Timestamp) != readTime)
                {
                    if (!chunk.Any())
                        realTime = DateTime.Now.TimeOfDay;

                    if (DateTime.Now.TimeOfDay - realTime > TimeSpan.FromSeconds(1))
                    {
                        realTime = DateTime.Now.TimeOfDay;
                        session.ChunkData.Add(chunk.ToList(), token);
                        chunk.Clear();
                    }

                    readTime = session.TimeShift(item.Timestamp);
                }

                var data = GetData(item);
                chunk.Add(data);
            }

            if (chunk.Any())
                session.ChunkData.Add(chunk, token);

            session.ChunkData.CompleteAdding();
        }
        private void DataSave(CancellationToken token, ReadSession session)
        {
            foreach (var data in session.ChunkData.GetConsumingEnumerable(token))
            {
                var readTime = session.TimeShift(data.Last().Timestamp);
                dataFactory.Value.WithDataManager(dm =>
                {
                    dm.WithRepository<IDataRepository>(repo => repo.Save(data));
                    dm.WithRepository<ISettingsRepository>(repo => repo.SetReadTime(readTime));
                }, true);
            }
        }
        private void UpdateSession(CancellationToken token, ReadSession session)
        {
            while (!session.IsComplete && !token.IsCancellationRequested)
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));
                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    session.AllTime = repo.GetAllTime();
                    session.Velocity = repo.GetReadVelocity() ?? 1.0;
                });
            }
        }
        private void BreakCurrentState()
        {            
            lock (critical)
            {
                if (state == null)
                    return;

                state.Session.IsComplete = true;
                state.TokenSource.Cancel();
                state = null;
            }            
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

            rawData.Timestamp = DateTime.Parse(rawData.Date) + TimeSpan.Parse(rawData.Time);
            return rawData;
        }
        private Data GetData(RawData rawData)
        {
            var data = new Data
            {
                Timestamp = rawData.Timestamp,
                Mac = rawData.Mac,
                MessageType = MessageType.S, //rawData.MessageType,
                ContentType = ContentType.C, //rawData.ContentType
            };

            return data;
        }

        private class EmulatorState
        {
            public ReadSession Session { get; private set; }
            public CancellationTokenSource TokenSource { get; private set; }

            public EmulatorState()
            {
                Session = new ReadSession();
                TokenSource = new CancellationTokenSource();
            }
        }
        private class ReadSession
        {
            private readonly DateTime minDate = new DateTime(2014, 11, 17, 0, 27, 0);

            public BlockingCollection<RawData> RawData { get; private set; }
            public BlockingCollection<IList<Data>> ChunkData { get; private set; }

            public TimeSpan ReadTime { get; set; }
            public TimeSpan? AllTime { get; set; }

            private TimeSpan elapsed;
            private DateTime start;

            private Double velocity;
            private readonly Object velocityCritical = new Object();

            public Double Velocity
            {
                get { return velocity; }
                set
                {
                    if (Math.Abs(velocity - value) < 1e-3)
                        return;

                    lock (velocityCritical)
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
                RawData = new BlockingCollection<RawData>();
                ChunkData = new BlockingCollection<IList<Data>>();
                start = DateTime.Now;
                elapsed = TimeSpan.Zero;
            }

            public TimeSpan StopWatch()
            {
                lock (velocityCritical)
                {
                    return elapsed + TimeSpan.FromSeconds((DateTime.Now - start).TotalSeconds*Velocity);
                }
            }

            public TimeSpan TimeShift(DateTime dateTime)
            {
                return TimeSpan.FromSeconds((dateTime - minDate).TotalSeconds);
            }

            public TimeSpan FutureRealTime(DateTime dateTime)
            {
                // ����������� ����� � �������, ����� ���������� ��������� ��� ���������
                var delay = TimeSpan.FromSeconds(5*Velocity);
                var sessionTime = TimeShift(dateTime) - StopWatch();
                return TimeSpan.FromSeconds(sessionTime.TotalSeconds/Velocity) + delay;
                    //��������������� � ��������� �������
            }

            public Boolean InPast(DateTime dateTime)
            {
                return TimeShift(dateTime) <= ReadTime;
            }

            public Boolean InFuture(DateTime dateTime)
            {
                // ��������� ���������� � delay ���
                var delay = TimeSpan.FromSeconds(5*Velocity);
                return FutureRealTime(dateTime) > TimeSpan.Zero + delay;
            }

            public Boolean Finish(DateTime dateTime)
            {
                if (!AllTime.HasValue)
                    return false;

                return TimeShift(dateTime) > AllTime.Value;
            }
        }
        private class RawData
        {
            public DateTime Timestamp { get; set; }

            public String Mac { get; set; }
            public String Date { get; set; }
            public String Time { get; set; }

            public String MessageType { get; set; }
            public String StreamType { get; set; }
        }
    }
}