using System.Data.SqlClient;

namespace Logic.Dal.Sql
{
    class SqlClusterRepository : SqlRepositoryBase, IClusterRepository
    {
        public SqlClusterRepository(SqlConnection connection) : base(connection)
        {
        }
    }
}