using System.Collections.Generic;
using System.Data.SqlClient;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Dal.Sql
{
    class SqlUserRepository :  SqlRepositoryBase, IUserRepository
    {
        public SqlUserRepository(SqlConnection connection) : base(connection)
        {
        }

        public ICollection<User> GetUsers()
        {
            return null;
        }
    }
}