using Logic.Dal.Repositories;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class NHibernateDataRepository : NHibernateRepositoryBase, IDataRepository
    {
        public NHibernateDataRepository(ISession session)
            : base(session)
        {
        }
    }
}