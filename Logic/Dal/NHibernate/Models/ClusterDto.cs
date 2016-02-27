using System;
using Logic.Model;

namespace Logic.Dal.NHibernate.Models
{
    internal class ClusterDto
    {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }

        public static implicit operator Cluster(ClusterDto cluster)
        {
            return new Cluster {Id = cluster.Id, Name = cluster.Name};
        }

        public static implicit operator ClusterDto(Cluster cluster)
        {
            return new ClusterDto {Id = cluster.Id, Name = cluster.Name};        
        }
    }
}
