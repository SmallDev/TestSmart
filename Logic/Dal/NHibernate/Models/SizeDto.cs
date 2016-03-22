using System;

namespace Logic.Dal.NHibernate.Models
{
    public class SizeDto
    {
        public virtual LearningDto Learning { get; set; }
        public virtual ClusterDto Cluster { get; set; }
        public virtual Double Size { get; set; }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var size = obj as SizeDto;
            if (size == null)
                return false;

            return Learning.Id == size.Learning.Id && Cluster.Id == size.Cluster.Id;
        }

        public override int GetHashCode()
        {
            return Learning.Id ^ Cluster.Id;
        }
    }
}
