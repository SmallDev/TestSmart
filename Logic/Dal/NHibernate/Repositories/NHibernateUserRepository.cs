using System.Collections.Generic;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class NHibernateUserRepository :  NHibernateRepositoryBase, IUserRepository
    {
        public NHibernateUserRepository(ISession session)
            : base(session)
        {
        }

        public ICollection<User> GetUsers()
        {
            return null;
        }
    }
}