using System.Collections.Generic;
using System.Linq;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;
using NHibernate.Util;

namespace Logic.Dal.NHibernate.Repositories
{
    class NHibernateClusterRepository : NHibernateRepositoryBase, IClusterRepository
    {
        public NHibernateClusterRepository(ISession session)
            : base(session)
        {
        }

        public void Save(IList<Cluster> clusters)
        {
            clusters.Select(c => new ClusterDto {Id = c.Id, Name = c.Name}).ForEach(c => Session.Save(c));
        }
    }
}