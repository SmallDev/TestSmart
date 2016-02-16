using Logic.Dal.Repositories;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    abstract class NHibernateRepositoryBase : IRepository
    {
        public ISession Session { get; private set; }
        protected NHibernateRepositoryBase(ISession session)
        {
            Session = session;
        }
    }
}