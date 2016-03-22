using System;
using System.Linq;
using Logic.Model;

namespace WebClient.Models
{
    public class ClusterModel
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public Double UsersCount { get; set; }

        public ClusterModel(Cluster cluster)
        {
            Id = cluster.Id;
            Name = cluster.Name;
            UsersCount = cluster.SizeHistory.Last().Item2;
        }
    }
}