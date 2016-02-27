using Logic.Dal.Repositories;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class NHibernateClusterRepository : NHibernateRepositoryBase, IClusterRepository
    {
        public NHibernateClusterRepository(ISession session)
            : base(session)
        {
        }
    }
}