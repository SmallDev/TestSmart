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
            connectionFunc().WithTransaction((connection, transaction) =>
            {
                using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulk.BulkCopyTimeout = (Int32)TimeSpan.FromMinutes(5).TotalSeconds;
                    bulk.BatchSize = 100;
                    bulk.DestinationTableName = "dbo.Data";
                    bulk.WriteToServer(AsDataTable(data));
                }
            });
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
                    DataType = typeof (TimeSpan)
                },
                new DataColumn
                {
                    ColumnName = "Mac",
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
                    ColumnName = "StreamType",
                    DataType = typeof (String)
                },
                new DataColumn
                {
                    ColumnName = "ReceivedRate",
                    DataType = typeof (Double)
                },
                new DataColumn
                {
                    ColumnName = "LinkFaultsRate",
                    DataType = typeof (Double)
                },
                new DataColumn
                {
                    ColumnName = "LostRate",
                    DataType = typeof (Double)
                },
                new DataColumn
                {
                    ColumnName = "RestoredRate",
                    DataType = typeof (Double)
                },
                new DataColumn
                {
                    ColumnName = "OverflowRate",
                    DataType = typeof (Double)
                },
                new DataColumn
                {
                    ColumnName = "UnderflowRate",
                    DataType = typeof (Double)
                },
                new DataColumn
                {
                    ColumnName = "DelayFactor",
                    DataType = typeof (Double)
                },
                new DataColumn
                {
                    ColumnName = "MediaLossRate",
                    DataType = typeof (Double)
                }
            });

            foreach (var item in data)
            {
                table.Rows.Add(null,
                    item.Timestamp, item.Mac,
                    item.MessageType.HasValue ? item.MessageType : (Object) DBNull.Value,
                    item.StreamType.HasValue ? item.StreamType : (Object) DBNull.Value,
                    item.ReceivedRate.HasValue ? item.ReceivedRate : (Object) DBNull.Value,
                    item.LinkFaultsRate.HasValue ? item.LinkFaultsRate : (Object) DBNull.Value,
                    item.LostRate.HasValue ? item.LostRate : (Object) DBNull.Value,
                    item.RestoredRate.HasValue ? item.RestoredRate : (Object) DBNull.Value,
                    item.OverflowRate.HasValue ? item.OverflowRate : (Object) DBNull.Value,
                    item.UnderflowRate.HasValue ? item.UnderflowRate : (Object) DBNull.Value,
                    item.DelayFactor.HasValue ? item.DelayFactor : (Object) DBNull.Value,
                    item.MediaLossRate.HasValue ? item.MediaLossRate : (Object) DBNull.Value);
            }

            return table;
        }

        public void Clear()
        {
            connectionFunc().WithCommand(command =>
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.Clear";
                command.Parameters.AddWithValue("readData", true);
                command.CommandTimeout = (Int32) TimeSpan.FromMinutes(10).TotalSeconds;
                command.ExecuteNonQuery();
            });
        }
    }
}