using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Util;

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
            var clusters = Session.Query<ClusterDto>().ToList()
                .Select(dto => (Cluster) dto).ToDictionary(c => c.Id, c => c);

            if (filter.WithSize)
                connectionFunc().WithCommand(command =>
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "dbo.ClustersSize";
                    return SizeReader(command.ExecuteReader());
                }).ForEach(t => clusters[t.Item1].Size = t.Item2);

            return clusters.Values.ToList();
        }
        public Cluster Get(ClusterFilter filter)
        {
            Cluster cluster = Session.Get<ClusterDto>(filter.Id);

            if (filter.WithSize)
                cluster.Size = connectionFunc().WithCommand(command =>
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "dbo.ClustersSize";
                    command.Parameters.AddWithValue("id", filter.Id);
                    return SizeReader(command.ExecuteReader()).First().Item2;
                });

            return cluster;
        }

        private ICollection<Tuple<Int32, Double>> SizeReader(SqlDataReader reader)
        {
            var result = new List<Tuple<Int32, Double>>();
            while (reader.Read())
                result.Add(new Tuple<Int32, Double>(reader.GetInt32(0), reader.GetDouble(1)));

            return result;
        }

        public void Save(Cluster cluster)
        {
            Session.SaveOrUpdate(cluster);
        }
    }
}