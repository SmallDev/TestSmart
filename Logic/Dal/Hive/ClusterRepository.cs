using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using Logic.Dal.Hive.Dto;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Dal.Hive
{
    class ClusterRepository : IHiveClusterRepository
    {
        private readonly Func<OdbcConnection> connectionFunc;
        public ClusterRepository(Func<OdbcConnection> connectionFunc)
        {
            this.connectionFunc = connectionFunc;
        }

        private List<UserDto> ReadUsers(Int32 clustersCount)
        {
            var users = new List<UserDto>();
            using (var connection = connectionFunc())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = String.Format("select mac, cluster, count from [default].[stbkmeansusers{0}]", clustersCount);
                    var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        users.Add(new UserDto
                        {
                            Mac = reader.GetFieldValue<String>(reader.GetOrdinal("mac")),
                            ClusterId = reader.GetFieldValue<Int32>(reader.GetOrdinal("cluster")),
                            XCount = reader.GetFieldValue<Int32>(reader.GetOrdinal("count")),
                        });
                    }
                }
            }

            return users;
        }

        public IList<Cluster> GetList(ClusterFilter filter)
        {
            var data = ReadUsers(6);
            var grouppedByMac = data.GroupBy(d => d.Mac).ToDictionary(d => d.Key, d => (Double) d.Sum(u => u.XCount));
            var weigtData = data.Select(d => new {d.Mac, d.ClusterId, Freq = d.XCount/grouppedByMac[d.Mac]});

            if (filter.Id.HasValue)
                weigtData = weigtData.Where(d => d.ClusterId == filter.Id.Value);

            return weigtData.GroupBy(d => d.ClusterId).Select(g =>
            {
                var cluster = new Cluster {Id = g.Key + 1};
                cluster.Name = Convert.ToString(cluster.Id);

                if (filter.WithSize)
                    cluster.SizeHistory = new[] {new Tuple<TimeSpan, Double>(TimeSpan.Zero, g.Sum(gg => gg.Freq))};

                if (filter.WithUsers)
                    cluster.UsersInfo = g.OrderByDescending(gg => gg.Freq)
                        .Select(gg => new Tuple<User, Double>(new User {MacAddress = gg.Mac}, gg.Freq)).ToList();
                return cluster;
            }).ToList();
        }
    }    
}
