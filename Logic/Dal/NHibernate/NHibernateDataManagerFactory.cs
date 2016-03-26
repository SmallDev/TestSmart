using System;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using Autofac;
using Common.Logging;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using Logic.Dal.NHibernate.Repositories;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;
using NHibernate.Util;
using ClusterRepository = Logic.Dal.Hive.ClusterRepository;

namespace Logic.Dal.NHibernate
{
    public class NHibernateDataManagerFactory : IDataManagerFactrory
    {
        private readonly Lazy<IContainer> baseContainer;
        private readonly Lazy<ISessionFactory> sessionFactory;
        private readonly Lazy<IDbConfig> config;
        public NHibernateDataManagerFactory(Func<IDbConfig> config, Func<ILoggerFactoryAdapter> logger)
        {
            this.config = new Lazy<IDbConfig>(config);
            baseContainer = new Lazy<IContainer>(() =>  InitContainer(logger));
            sessionFactory = new Lazy<ISessionFactory>(InitSessionFactory);
        }

        private ISessionFactory InitSessionFactory()
        {
            var configuration = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(config.Value.ConnectionString))
                .Mappings(m => m.FluentMappings
                    .AddFromAssemblyOf<IEntity>()
                    .Conventions.Add(ForeignKey.EndsWith("Id")))
                .BuildConfiguration();

            return configuration.BuildSessionFactory();
        }
        private IContainer InitContainer(Func<ILoggerFactoryAdapter> logger)
        {
            var builder = new ContainerBuilder();

            builder.Register(context => logger()).As<ILoggerFactoryAdapter>();
            builder.Register(context => sessionFactory.Value.OpenSession()).InstancePerLifetimeScope();
            builder.Register(context => new SqlConnection(config.Value.ConnectionString));
            builder.Register(context => new OdbcConnection(config.Value.HiveConnectionString));

            GetType().Assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type
                .IsAssignableTo<IRepository>()).ForEach(type => type.GetInterfaces()
                    .Where(i => i.IsAssignableTo<IRepository>())
                    .Aggregate(builder.RegisterType(type), (current, i) => current.As(i)));

            if (String.IsNullOrEmpty(config.Value.HiveConnectionString))
                builder.RegisterType<FakeHiveRepository>().As<IHiveClusterRepository>();
            else
                builder.RegisterType<ClusterRepository>().As<IHiveClusterRepository>();

            return builder.Build();
        }

        public IDataManager GetDataManager()
        {
            return new NHibernateDataManager(baseContainer.Value);
        }
    }
}