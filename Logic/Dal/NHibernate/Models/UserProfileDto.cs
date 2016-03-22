using System;

namespace Logic.Dal.NHibernate.Models
{
    public class UserProfileDto
    {
        public virtual UserDto User { get; set; }
        public virtual ClusterDto Cluster { get; set; }
        public virtual Double Probability { get; set; }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var size = obj as SizeDto;
            if (size == null)
                return false;

            return User.Id == size.Learning.Id && Cluster.Id == size.Cluster.Id;
        }

        public override int GetHashCode()
        {
            return User.Id ^ Cluster.Id;
        }
    }
}