using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Logic.Model;

namespace Logic.Dal.NHibernate.Models
{
    public class ClusterDto
    {
        public virtual Int32 Id { get; set; }
        public virtual String Name { get; set; }
        public virtual IList<SizeDto> Sizes { get; set; }
        public virtual IList<UserProfileDto> Users { get; set; } 

        public static implicit operator Cluster(ClusterDto cluster)
        {
            var result = new Cluster {Id = cluster.Id, Name = cluster.Name};
            if (cluster.Sizes != null)
                result.SizeHistory =
                    cluster.Sizes.Select(s => new Tuple<TimeSpan, Double>(s.Learning.From, s.Size)).ToList();

            if (cluster.Users != null)
                result.UsersInfo = cluster.Users.Select(u => new Tuple<User, Double>(u.User, u.Probability)).ToList();

            result.Properties = new Collection<Property>();
            return result;
        }

        public static implicit operator ClusterDto(Cluster cluster)
        {
            return new ClusterDto {Id = cluster.Id, Name = cluster.Name};
        }
    }
}
