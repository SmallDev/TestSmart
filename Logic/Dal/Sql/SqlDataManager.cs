using System;
using System.Data.SqlClient;
using Autofac;

namespace Logic.Dal.Sql
{
    class SqlDataManager : IDataManager
    {
        private readonly Lazy<ILifetimeScope> scope;
        private SqlConnection connection;
        private SqlTransaction transaction;

        public SqlDataManager(ILifetimeScope baseScope)
        {
            scope = new Lazy<ILifetimeScope>(() =>  InitScope(baseScope));
        }

        private ILifetimeScope InitScope(ILifetimeScope baseScope)
        {
            var newScope = baseScope.BeginLifetimeScope();
            connection = newScope.Resolve<SqlConnection>();
            connection.Open();
            transaction = connection.BeginTransaction();

            return newScope;
        }

        public T GetRepository<T>() where T : IRepository
        {
            return scope.Value.Resolve<T>();
        }

        public void Commit()
        {
            if (connection != null && transaction != null)
                transaction.Commit();
        }

        public void Rollback()
        {
            if (connection != null && transaction != null)
                transaction.Rollback();
        }

        public void Dispose()
        {
            if (transaction != null)
                transaction.Dispose();

            if (connection != null)
                connection.Dispose();

            if (scope.IsValueCreated)
                scope.Value.Dispose();
        }
    }
}
