using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Logic.Facades;

namespace WebClient.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StreamService : IStreamService, IDisposable
    {
        private readonly Lazy<EmulatorFacade> algorithmFacade;
        private CancellationTokenSource readCancel;
        
        public StreamService(Func<EmulatorFacade> algorithmFacade)
        {
            this.algorithmFacade = new Lazy<EmulatorFacade>(algorithmFacade);
        }

        public void StartRead()
        {
            lock (algorithmFacade)
            {
                if (readCancel != null)
                    readCancel.Cancel();

                readCancel = new CancellationTokenSource();
                Task.Run(() => algorithmFacade.Value.StartRead(readCancel.Token), readCancel.Token);
            }
        }

        public void StopRead()
        {
            lock (algorithmFacade)
            {
                readCancel.Cancel();
                readCancel = null;
            }
        }

        public void Dispose()
        {
            StopRead();
        }
    }
}