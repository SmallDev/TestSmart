using System;
using System.Data.SqlClient;
using Autofac;
using Logic.Dal.Repositories;

namespace Logic.Dal.Sql
{
    public class SqlDataManagerFactory : IDataManagerFactrory
    {
        private readonly Lazy<IContainer> baseContainer;

        public SqlDataManagerFactory(Func<IDbConfig> config)
        {
            baseContainer = new Lazy<IContainer>(() => InitContainer(config));
        }

        private IContainer InitContainer(Func<IDbConfig> config)
        {
            var connectionString = config().ConnectionString;
            var builder = new ContainerBuilder();
            
            builder.Register(context => new SqlConnection(connectionString)).InstancePerLifetimeScope();
            builder.RegisterType<SqlUserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            builder.RegisterType<SqlClusterRepository>().As<IClusterRepository>().InstancePerLifetimeScope();

            return builder.Build();
        }

        public IDataManager GetDataManager()
        {
            return new SqlDataManager(baseContainer.Value);
        }
    }
}