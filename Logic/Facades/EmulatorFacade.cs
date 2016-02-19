using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly Lazy<IReaderConfig> readerConfig;
        
        public EmulatorFacade(Func<IDataManagerFactrory> dataFactory, Func<IReaderConfig> readerConfig)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
            this.readerConfig = new Lazy<IReaderConfig>(readerConfig);
        }

        public void Clear()
        {
            dataFactory.Value.WithDataManager(dm =>
            {
                dm.WithRepository<ISettingsRepository>(repo => repo.SetReadTime(null));
                dm.WithRepository<IDataRepository>(repo => repo.Clear());
            }, true);
        }
        public void StartRead(CancellationTokenSource tokenSource)
        {
            var session = new ReadSession();
            dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
            {
                session.ReadTime = repo.GetReadTime() ?? -TimeSpan.FromSeconds(1);
                session.AllTime = repo.GetAllTime();
                session.Velocity = repo.GetReadVelocity() ?? 1.0;
            });

            try
            {
                Task.WaitAll(
                    Task.Run(() => DataRead(tokenSource.Token, session), tokenSource.Token),
                    Task.Run(() => DataChunk(tokenSource.Token, session), tokenSource.Token),
                    Task.Run(() => DataSave(tokenSource.Token, session), tokenSource.Token),
                    Task.Run(() => UpdateSession(session)));
            }
            catch
            {
                tokenSource.Cancel();
                throw;
            }
            finally
            {
                session.IsComplete = true;
            }
        }

        private void DataRead(CancellationToken token, ReadSession session)
        {
            using (var stream = File.OpenRead(readerConfig.Value.FileName))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, new CsvConfiguration {Delimiter = " "}))
            {
                while (csv.Read())
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
        private void UpdateSession(ReadSession session)
        {
            while (!session.IsComplete)
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));
                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    session.AllTime = repo.GetAllTime();
                    session.Velocity = repo.GetReadVelocity() ?? 1.0;
                });
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
    }
}