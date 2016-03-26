using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Dal.NHibernate.Repositories
{
    class FakeHiveRepository : IHiveClusterRepository
    {
        public IList<Cluster> GetList(Int32 allCount, ClusterFilter filter)
        {
            var clusters =  new[]
            {
                new Cluster
                {
                    Id = 1,
                    Name = "Hive 1",
                    SizeHistory = new[]{new Tuple<TimeSpan, double>(TimeSpan.Zero, 1) },
                    UsersInfo = new Collection<Tuple<User, double>>{new Tuple<User, double>(new User{MacAddress = "11"}, 0.9)},
                    Properties = new Collection<Property>()
                },
                new Cluster
                {
                    Id = 2,
                    Name = "Hive 2",
                    SizeHistory = new[]{new Tuple<TimeSpan, double>(TimeSpan.Zero, 2) },
                    UsersInfo = new Collection<Tuple<User, double>>
                    {
                        new Tuple<User, double>(new User{MacAddress = "13"}, 0.6),
                        new Tuple<User, double>(new User{MacAddress = "16"}, 0.8),
                    },
                    Properties = new Collection<Property>()
                }
            }.ToList();

            if (filter.Id.HasValue)
                clusters = clusters.Where(c => c.Id == filter.Id.Value).ToList();

            return clusters;
        }
    }
}