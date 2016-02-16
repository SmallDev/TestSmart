using System;
using System.ServiceModel;

namespace WebClient.Services
{
    [ServiceContract]
    public interface IStreamService
    {
        [OperationContract]
        void Start(Boolean fromBegin = false);

        [OperationContract]
        void Stop();        
    }
}