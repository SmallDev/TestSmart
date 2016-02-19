using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Dal.NHibernate.Repositories
{
    internal class SqlDataRepository : IDataRepository
    {
        private readonly Func<SqlConnection> connectionFunc;

        public SqlDataRepository(Func<SqlConnection> connectionFunc)
        {
            this.connectionFunc = connectionFunc;
        }

        public void Save(IList<Data> data)
        {
            using (var connection = connectionFunc())
            {
                connection.Open();
                using (var transation = connection.BeginTransaction())
                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transation))
                {

                    bulk.BatchSize = 100;
                    bulk.DestinationTableName = "dbo.Data";
                    bulk.WriteToServer(AsDataTable(data));
                    transation.Commit();
                }
            }
        }

        private DataTable AsDataTable(IEnumerable<Data> data)
        {
            var table = new DataTable("Data");
            table.Columns.AddRange(new[]
            {
                new DataColumn
                {
                    ColumnName = "Id",
                    DataType = typeof (Int64)
                },
                new DataColumn
                {
                    ColumnName = "Timestamp",
                    DataType = typeof (DateTime)
                },
                new DataColumn
                {
                    ColumnName = "MAC",
                    DataType = typeof (String),
                    MaxLength = 17
                },
                new DataColumn
                {
                    ColumnName = "MessageType",
                    DataType = typeof (String)
                },
                new DataColumn
                {
                    ColumnName = "ContentType",
                    DataType = typeof (String)
                }
            });

            foreach (var item in data)
            {
                table.Rows.Add(null,
                    item.Timestamp, item.Mac,
                    item.MessageType.HasValue ? item.MessageType : (Object) DBNull.Value,
                    item.ContentType.HasValue ? item.ContentType : (Object) DBNull.Value);
            }

            return table;
        }

        public void Clear()
        {
            using (var connection = connectionFunc())            
            using(var command = connection.CreateCommand())
            {
                connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = "TRUNCATE TABLE Data";
                command.CommandTimeout = (Int32) TimeSpan.FromMinutes(1).TotalSeconds;
                command.ExecuteNonQuery();
            }
        }
    }
}