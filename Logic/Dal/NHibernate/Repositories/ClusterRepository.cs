using System.Collections.Generic;
using System.Linq;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class ClusterRepository : NHibernateRepositoryBase, IClusterRepository
    {
        public ClusterRepository(ISession session)
            : base(session)
        {
        }

        public IList<Cluster> GetList(ClusterFilter filter)
        {
            var query = Session.QueryOver<ClusterDto>();
            if (filter.Id.HasValue)
                query = query.Where(c => c.Id == filter.Id.Value);

            if (filter.WithSize)
                Session.QueryOver<ClusterDto>()
                    .Fetch(dto => dto.Sizes)
                    .Eager.Fetch(dto => dto.Sizes[0].Learning)
                    .Eager.Future();

            var clusters = query.Future().ToList();

            if (filter.WithUsers)
                foreach (var cluster in clusters)
                {
                    var clusterId = cluster.Id;
                    cluster.Users =
                        Session.QueryOver<UserProfileDto>()
                            .Fetch(p => p.User).Eager
                            .Where(p => p.Cluster.Id == clusterId)
                            .OrderBy(p => p.Probability)
                            .Desc.Take(20).List();
                }

            if (filter.WithProperties)
            {
                foreach (var cluster in clusters)
                {
                    var clusterId = cluster.Id;
                    cluster.RProfile = Session.QueryOver<ClusterRProfileDto>().Where(p => p.Cluster.Id == clusterId).List();
                    cluster.NProfile = Session.QueryOver<ClusterNProfileDto>().Where(p => p.Cluster.Id == clusterId).List();
                }
                
            }

            return clusters.Select(dto => (Cluster) dto).ToList();
        }

        public void Save(Cluster cluster)
        {
            Session.SaveOrUpdate(cluster);
        }
    }
}