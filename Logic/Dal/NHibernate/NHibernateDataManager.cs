using System;
using Autofac;
using Logic.Dal.Repositories;
using NHibernate;

namespace Logic.Dal.NHibernate
{
    class NHibernateDataManager : IDataManager
    {
        private readonly Lazy<ILifetimeScope> scope;
        private ISession session;
        private ITransaction transaction;

        public NHibernateDataManager(ILifetimeScope baseScope)
        {
            scope = new Lazy<ILifetimeScope>(() =>  InitScope(baseScope));
        }

        private ILifetimeScope InitScope(ILifetimeScope baseScope)
        {
            var newScope = baseScope.BeginLifetimeScope();
            session = newScope.Resolve<ISession>();
            transaction = session.BeginTransaction();

            return newScope;
        }

        public T GetRepository<T>() where T : IRepository
        {
            return scope.Value.Resolve<T>();
        }

        public void Commit()
        {
            if (transaction != null)
                transaction.Commit();
        }

        public void Rollback()
        {
            if (transaction != null)
                transaction.Rollback();
        }

        public void Dispose()
        {
            if (transaction != null)
                transaction.Dispose();

            if (session != null)
                session.Dispose();

            if (scope.IsValueCreated)
                scope.Value.Dispose();
        }
    }
}
