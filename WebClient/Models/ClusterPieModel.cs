using System;
using System.Linq;
using Logic.Model;

namespace WebClient.Models
{
    public class ClusterPieModel
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public Double UsersCount { get; set; }

        public ClusterPieModel(Cluster cluster)
        {
            Id = cluster.Id;
            Name = cluster.Name;
            UsersCount = cluster.SizeHistory.Count != 0 ? cluster.SizeHistory.Last().Item2 : 0;
        }
    }
}