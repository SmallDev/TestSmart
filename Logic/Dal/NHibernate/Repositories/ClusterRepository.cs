using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class ClusterRepository : NHibernateRepositoryBase, IClusterRepository
    {
        private readonly Func<SqlConnection> connectionFunc;
        public ClusterRepository(ISession session, Func<SqlConnection> connectionFunc)
            : base(session)
        {
            this.connectionFunc = connectionFunc;
        }

        public IList<Cluster> GetList(ClusterFilter filter)
        {
            var query = Session.QueryOver<ClusterDto>().Future();

            if (filter.WithSize)
                Session.QueryOver<ClusterDto>()
                    .Fetch(dto => dto.Sizes)
                    .Eager.Fetch(dto => dto.Sizes[0].Learning)
                    .Eager.Future();

            var clusters = query.ToList();
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

            return clusters.Select(dto => (Cluster) dto).ToList();
        }

        public void Save(Cluster cluster)
        {
            Session.SaveOrUpdate(cluster);
        }
    }
}