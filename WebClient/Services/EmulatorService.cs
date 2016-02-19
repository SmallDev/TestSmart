using System;
using System.ServiceModel;
using Logic.Facades;
using Common.Logging;

namespace WebClient.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class EmulatorService : IEmulatorService, IDisposable
    {
        private readonly Lazy<EmulatorFacade> emulator;
        private readonly Lazy<ILog> logger;

        public EmulatorService(Func<EmulatorFacade> emulatorFacade, Func<ILoggerFactoryAdapter> loggerFactory)
        {
            emulator = new Lazy<EmulatorFacade>(emulatorFacade);
            logger = new Lazy<ILog>(() => loggerFactory().GetLogger(GetType()));
        }

        public async void StartRead()
        {
            logger.Value.Info("Begin StartRead");
            try
            {
                await emulator.Value.StartRead();
                logger.Value.Info("End StartRead");
            }
            catch (Exception ex)
            {
                logger.Value.Error(ex);
            }
        }
        public async void StopRead()
        {  
            logger.Value.Info("Start StopRead");
            try
            {
                await emulator.Value.StopRead();
                logger.Value.Info("End StopRead");
            }
            catch (Exception ex)
            {
                logger.Value.Error(ex);
            }
        }

        public Double GetVelocity()
        {
            try
            {
                return emulator.Value.GetVelocity();
            }
            catch (Exception ex)
            {
                logger.Value.Error(ex);
                throw;
            }
        }
        public void SetVelocity(Double velocity)
        {
            try
            {
                emulator.Value.SetVelocity(velocity);
            }
            catch (Exception ex)
            {
                logger.Value.Error(ex);
                throw;
            }
        }

        public String GetAllTime()
        {
            try
            {
                var time = emulator.Value.GetAllTime();
                return time.HasValue ? time.Value.ToString("hh\\:mm\\:ss") : null;
            }
            catch (Exception ex)
            {
                logger.Value.Error(ex);
                throw;
            }
        }
        public void SetAllTime(String time)
        {
            try
            {
                emulator.Value.SetAllTime(TimeSpan.Parse(time));
            }
            catch (Exception ex)
            {
                logger.Value.Error(ex);
                throw;
            }
        }

        public void Dispose()
        {
            StopRead();
        }
    }
}