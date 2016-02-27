using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;
using NHibernate.Linq;

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

        public User GetUser(UserFilter filter)
        {
            return Session.Get<UserDto>(filter.Id);
        }

        public IList<User> GetUsers(UserFilter filter)
        {
            var query = Session.Query<UserDto>().Where(u => u.Mac.StartsWith(filter.Mac));
            if (filter.PageSize != 0)
                query = query.Skip((filter.PageNumber - 1)*filter.PageSize).Take(filter.PageSize);
            
            return query.ToList().Select(u => (User) u).ToList();
        }
    }
}