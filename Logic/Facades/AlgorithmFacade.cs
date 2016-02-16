using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Logic.Dal;
using Logic.Dal.Repositories;

namespace Logic.Facades
{
    public class AlgorithmFacade
    {
        private readonly Lazy<IDataManagerFactrory> dataFactory;
        private readonly Lazy<IReaderConfig> readerConfig;

        public AlgorithmFacade(Func<IDataManagerFactrory> dataFactory, Func<IReaderConfig> readerConfig)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
            this.readerConfig = new Lazy<IReaderConfig>(readerConfig);
        }

        public async void StartRead(CancellationToken token, Boolean fromBegin)
        {
            if (fromBegin)
                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                    repo.SetReadTimestamp(null), true);

            var timestamp = dataFactory.Value.WithRepository<DateTime?, ISettingsRepository>(repo =>
                repo.GetReadTimestamp());

            var data = new BlockingCollection<String>();
            await Task.WhenAll(
                Task.Factory.StartNew(() => DataProducer(token, timestamp, data), token),
                Task.Factory.StartNew(() => DataConsumer(token, data), token));
        }

        private void DataProducer(CancellationToken token, DateTime? timeStamp, BlockingCollection<String> data)
        {
            var count = 1;
            using(var stream = File.OpenRead(readerConfig.Value.FileName))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    reader.ReadLine();
                    if (++count == 100)
                    {
                        data.CompleteAdding();
                        break;
                    }                    
                }
            }
        }

        private void DataConsumer(CancellationToken token, BlockingCollection<String> data)
        {
            foreach (var item in data.GetConsumingEnumerable(token))
            {
                token.ThrowIfCancellationRequested();
            }
        }
    }
}