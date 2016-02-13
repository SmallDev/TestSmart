using System;
using Autofac;
using Logic.Dal.Repositories;

namespace Logic.Dal.Wcf
{
    public class WcfDataManager : IDataManader
    {
        private readonly Lazy<IContainer> container;
        public WcfDataManager()
        {
            container = new Lazy<IContainer>(InitContainer);    
        }

        private IContainer InitContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<UserWcfRepository>().As<IUserRepository>().SingleInstance();
            builder.RegisterType<ClusterWcfRepository>().As<IClusterRepository>().SingleInstance();

            return builder.Build();
        }

        public T GetRepository<T>() where T : IRepository
        {
            return container.Value.Resolve<T>();
        }
    }
}
