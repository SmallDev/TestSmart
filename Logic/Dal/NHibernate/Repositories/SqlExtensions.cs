using System;
using System.Data.SqlClient;

namespace Logic.Dal.NHibernate.Repositories
{
    public static class SqlExtensions
    {
        public static T WithConnection<T>(this SqlConnection sqlConnection, Func<SqlConnection, T> action, Boolean useTransaction = false)
        {
            using (sqlConnection)
            {
                sqlConnection.Open();
                T result;

                if (useTransaction)
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        result = action(sqlConnection);
                        transaction.Commit();
                    }
                
                else
                    result = action(sqlConnection);

                return result;
            }
        }
        public static void WithConnection(this SqlConnection sqlConnection, Action<SqlConnection> action, Boolean useTransaction = false)
        {
            sqlConnection.WithConnection(connection =>
            {
                action(connection);
                return true;
            }, useTransaction);
        }

        public static T WithCommand<T>(this SqlConnection sqlConnection, Func<SqlCommand, T> action, Boolean useTransaction = false)
        {
            Func<SqlConnection, T> func = connection =>
            {
                using (var command = connection.CreateCommand())
                    return action(command);
            };

            return sqlConnection.WithConnection(func, useTransaction);
        }
        public static void WithCommand(this SqlConnection sqlConnection, Action<SqlCommand> action, Boolean useTransaction = false)
        {
            sqlConnection.WithCommand(command =>
            {
                action(command);
                return true;
            }, useTransaction);
        }

        public static T WithTransaction<T>(this SqlConnection sqlConnection, Func<SqlConnection, SqlTransaction, T> action)
        {
            return sqlConnection.WithConnection(connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var res = action(connection, transaction);
                    transaction.Commit();
                    return res;
                }
            });
        }
        public static void WithTransaction(this SqlConnection sqlConnection, Action<SqlConnection, SqlTransaction> action)
        {
            sqlConnection.WithTransaction((connection, transaction) =>
            {
                action(connection, transaction);
                return true;
            });
        }
    }
}