using System.Data.SqlClient;

namespace Logic.Dal.Sql
{
    class SqlRepositoryBase : IRepository
    {
        public SqlConnection Connection { get; private set; }

        public SqlRepositoryBase(SqlConnection connection)
        {
            Connection = connection;
        }
    }
}