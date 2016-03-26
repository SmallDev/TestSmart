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
        public virtual IList<ClusterRProfileDto> RProfile { get; set; }
        public virtual IList<ClusterNProfileDto> NProfile { get; set; }

        public static Dictionary<Int32, PropertyType> types = new Dictionary<Int32, PropertyType>
        {
            {3, PropertyType.Received},
            {4, PropertyType.LinkFaults},
            {6, PropertyType.Lost},
            {7, PropertyType.Restored},
            {8, PropertyType.Overflow},
            {9, PropertyType.Underflow},
            {10, PropertyType.DelayFactor},
            {13, PropertyType.MediaLossRate},
        };

        public static implicit operator Cluster(ClusterDto cluster)
        {
            var result = new Cluster {Id = cluster.Id, Name = cluster.Name};
            if (cluster.Sizes != null)
                result.SizeHistory =
                    cluster.Sizes.Select(s => new Tuple<TimeSpan, Double>(s.Learning.From, s.Size)).ToList();

            if (cluster.Users != null)
                result.UsersInfo = cluster.Users.Select(u => new Tuple<User, Double>(u.User, u.Probability)).ToList();

            result.Properties = new Collection<Property>();
            if (cluster.RProfile != null)
                foreach (var profile in cluster.RProfile)
                {
                    result.Properties.Add(new Property
                    {
                        Mean = profile.Mean,
                        Variance = profile.Variance,
                        Type = types[profile.ColumnId]
                    });
                }
            
            return result;
        }

        public static implicit operator ClusterDto(Cluster cluster)
        {
            return new ClusterDto {Id = cluster.Id, Name = cluster.Name};
        }
    }
}
