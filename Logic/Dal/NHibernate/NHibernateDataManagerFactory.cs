using System;
using System.Linq;
using Autofac;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;
using NHibernate.Util;

namespace Logic.Dal.NHibernate
{
    public class NHibernateDataManagerFactory : IDataManagerFactrory
    {
        private readonly Lazy<IContainer> baseContainer;
        private readonly Lazy<ISessionFactory> sessionFactory;

        public NHibernateDataManagerFactory(Func<IDbConfig> config)
        {
            baseContainer = new Lazy<IContainer>(InitContainer);
            sessionFactory = new Lazy<ISessionFactory>(() => InitSessionFactory(config));
        }

        private ISessionFactory InitSessionFactory(Func<IDbConfig> config)
        {
            var configuration = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2008.ConnectionString(config().ConnectionString))
                .Mappings(m => m.FluentMappings
                    .AddFromAssemblyOf<IEntity>()
                    .Conventions.Add(ForeignKey.EndsWith("Id")))
                .BuildConfiguration();

            return configuration.BuildSessionFactory();
        }
        private IContainer InitContainer()
        {
            var builder = new ContainerBuilder();

            builder.Register(context => sessionFactory.Value.OpenSession()).InstancePerLifetimeScope();
            GetType().Assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && type
                .IsAssignableTo<IRepository>()).ForEach(type => type.GetInterfaces()
                    .Where(i => i.IsAssignableTo<IRepository>())
                    .Aggregate(builder.RegisterType(type), (current, i) => current.As(i)));

            return builder.Build();
        }

        public IDataManager GetDataManager()
        {
            return new NHibernateDataManager(baseContainer.Value);
        }
    }
}