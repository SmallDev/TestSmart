using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web.Mvc;

namespace WebClient.Services
{
    public class DependencyHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            var instance = DependencyResolver.Current.GetService(serviceType);
            return new ServiceHost(instance, baseAddresses);
        }
    }
}