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
        private readonly Lazy<AlgorithmFacade> algorithmFacade;
        private CancellationTokenSource cancelSource;
        
        public StreamService(Func<AlgorithmFacade> algorithmFacade)
        {
            this.algorithmFacade = new Lazy<AlgorithmFacade>(algorithmFacade);
        }

        public void Start(Boolean fromBegin = false)
        {
            lock (algorithmFacade)
            {
                if (cancelSource != null)
                    cancelSource.Cancel();

                cancelSource = new CancellationTokenSource();
                algorithmFacade.Value.StartRead(cancelSource.Token, fromBegin);
            }
        }

        public void Stop()
        {
            lock (algorithmFacade)
            {
                cancelSource.Cancel();
                cancelSource = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}