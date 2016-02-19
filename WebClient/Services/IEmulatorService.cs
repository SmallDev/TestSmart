using System;
using System.ServiceModel;

namespace WebClient.Services
{
    [ServiceContract]
    public interface IEmulatorService
    {
        [OperationContract]
        void StartRead();
        [OperationContract]
        void StopRead();

        [OperationContract]
        Double GetVelocity();
        [OperationContract]
        void SetVelocity(Double velocity);

        [OperationContract]
        String GetAllTime();
        [OperationContract]
        void SetAllTime(String time);
    }
}