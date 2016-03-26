using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Threading.Tasks;
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
        private List<ClusterDto> ReadClusters(Int32 clustersCount)
        {
            var clusters = new List<ClusterDto>();
            using (var connection = connectionFunc())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = String.Format("select cluster, col, value from [default].[stbkmeansclusters{0}]", clustersCount);
                    var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        clusters.Add(new ClusterDto
                        {

                            ClusterId = reader.GetFieldValue<Int32>(reader.GetOrdinal("cluster")),
                            ColumnId = reader.GetFieldValue<Int32>(reader.GetOrdinal("col")),
                            Value = Convert.ToDouble(reader.GetValue(reader.GetOrdinal("value")))
                        });
                    }
                }
            }

            return clusters;
        }

        public IList<Cluster> GetList(Int32 allCount, ClusterFilter filter)
        {
            var usersData = new List<UserDto>();
            var clustersData = new List<ClusterDto>();

            Task.WaitAll(Task.Run(() =>
            {
                usersData = ReadUsers(allCount);
            }), Task.Run(() =>
            {
                if (filter.WithProperties)
                    clustersData = ReadClusters(allCount);
            }));

            var grouppedByMac = usersData.GroupBy(d => d.Mac).ToDictionary(d => d.Key, d => (Double)d.Sum(u => u.XCount));
            var weigtData = usersData.Select(d => new { d.Mac, d.ClusterId, Freq = d.XCount / grouppedByMac[d.Mac] });

            if (filter.Id.HasValue)
                weigtData = weigtData.Where(d => d.ClusterId + 1 == filter.Id.Value);

            return weigtData.GroupBy(d => d.ClusterId).Select(g =>
            {
                var cluster = new Cluster {Id = g.Key + 1};
                cluster.Name = Convert.ToString(cluster.Id);

                if (filter.WithSize)
                    cluster.SizeHistory = new[] {new Tuple<TimeSpan, Double>(TimeSpan.Zero, g.Sum(gg => gg.Freq))};

                if (filter.WithUsers)
                    cluster.UsersInfo = g.OrderByDescending(gg => gg.Freq)
                        .Select(gg => new Tuple<User, Double>(new User {MacAddress = gg.Mac}, gg.Freq)).ToList();

                if (filter.WithProperties)
                    cluster.Properties = clustersData.Where(c => c.ClusterId == cluster.Id).Select(c => new Property
                    {
                        Type = (PropertyType) c.ColumnId,
                        Mean = c.Value
                    }).ToList();

                return cluster;
            }).ToList();
        }
    }
}
