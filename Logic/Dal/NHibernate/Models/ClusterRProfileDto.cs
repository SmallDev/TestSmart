using System;

namespace Logic.Dal.NHibernate.Models
{
    public class ClusterRProfileDto
    {
        public virtual ClusterDto Cluster { get; set; }
        public virtual Int32 ColumnId { get; set; }
        public virtual Double Mean { get; set; }
        public virtual Double Variance { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var profile = obj as ClusterRProfileDto;
            if (profile == null)
                return false;

            return Cluster.Id == profile.Cluster.Id && ColumnId == profile.ColumnId;
        }

        public override int GetHashCode()
        {
            return Cluster.Id ^ ColumnId;
        }
    }
}