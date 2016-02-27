using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class UserRepository :  NHibernateRepositoryBase, IUserRepository
    {
        private readonly Func<SqlConnection> connectionFunc;
        public UserRepository(ISession session, Func<SqlConnection> connectionFunc)
            : base(session)
        {
            this.connectionFunc = connectionFunc;
        }

        public IList<User> GetUsers()
        {
            throw new NotImplementedException();
        }
    }
}