using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper.Configuration;
using Logic.Dal;
using Logic.Dal.Repositories;

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
            dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
            {
                repo.SetReadTime(TimeSpan.Zero);
                repo.SetCalcTime(TimeSpan.Zero);
            }, true);
        }
        public void StartRead(CancellationToken token)
        {
            var session = new ReadSession();
            dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
            {
                session.ReadTime = (repo.GetReadTime() ?? TimeSpan.Zero) - TimeSpan.FromSeconds(1);
                session.Velocity = repo.GetReadVelocity();
            });
        
            Task.WaitAll(
                Task.Factory.StartNew(() => DataProducer(token, session), token),
                Task.Factory.StartNew(() => DataConsumer(token, session), token));
        }

        private void DataProducer(CancellationToken token, ReadSession session)
        {
            var count = 1;

            using (var stream = File.OpenRead(readerConfig.Value.FileName))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvHelper.CsvReader(reader, new CsvConfiguration {Delimiter = " "}))
            {
                var tempCollection = new Collection<RowData>();
                var tempTime = session.ReadTime;

                while (csv.Read())
                {
                    token.ThrowIfCancellationRequested();

                    var rowData = new RowData
                    {
                        Mac = csv[6],
                        Date = csv[3],
                        Time = csv[4]
                    };

                    var rowTime = (DateTime.Parse(rowData.Date) + TimeSpan.Parse(rowData.Time)) - session.MinDate;
                    if (rowTime <= session.ReadTime)
                        continue;

                    //if (tempTime != session.ReadTime && rowTime != tempTime)
                    //{
                    //    session.ReadTime = tempTime;
                    //    dataFactory.Value.WithRepository<ISettingsRepository>(repo => repo.SetReadTime(session.ReadTime), true);
                        
                    //    foreach (var data in tempCollection)
                    //        session.Data.Add(data, token);
                    //    tempCollection.Clear();
                    //}

                    //tempTime = rowTime;
                    //tempCollection.Add(rowData);
                    session.Data.Add(rowData, token);

                    if (++count == 100)
                    {
                        session.Data.CompleteAdding();
                        break;
                    }
                }
            }
        }     
        private void DataConsumer(CancellationToken token, ReadSession session)
        {
            TimeSpan time = TimeSpan.Zero;
            foreach (var item in session.Data.GetConsumingEnumerable(token))
            {
                token.ThrowIfCancellationRequested();

                var itemTime = TimeSpan.Parse(item.Time);
                if (itemTime < time)
                {
                    var d = "";
                }

                time = itemTime;
            }
        }
    }

    public class RowData
    {
        public String Mac { get; set; }
        public String Date { get; set; }
        public String Time { get; set; }
    }

    public class ReadSession
    {
        public DateTime MinDate = new DateTime(2014, 7, 1);

        public BlockingCollection<RowData> Data { get; private set; }
        public DateTime Start { get; private set; }
        public TimeSpan ReadTime { get; set; }
        public Double Velocity { get; set; }

        public ReadSession()
        {
            Data = new BlockingCollection<RowData>();
            Start = DateTime.Now;
        }
    }
}