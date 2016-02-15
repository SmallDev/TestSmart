using System.Data.SqlClient;
using Logic.Dal.Repositories;

namespace Logic.Dal.Sql
{
    class SqlDataRepository : SqlRepositoryBase, IDataRepository
    {
        public SqlDataRepository(SqlConnection connection) : base(connection)
        {
        }
    }
}