using System.ServiceModel;

namespace WebClient.Services
{
    [ServiceContract]
    public interface IStreamService
    {
        [OperationContract]
        void StartRead();

        [OperationContract]
        void StopRead();        
    }
}