using System;

namespace Logic.Dal.NHibernate.Models
{
    public class ClusterNProfileDto
    {
        public virtual ClusterDto Cluster { get; set; }
        public virtual Int32 ColumnId { get; set; }
        public virtual NominalDto Nominal { get; set; }

        public virtual Double Probability { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            var profile = obj as ClusterNProfileDto;
            if (profile == null)
                return false;

            return Cluster.Id == profile.Cluster.Id && ColumnId == profile.ColumnId && Nominal.Id == profile.Nominal.Id;
        }

        public override int GetHashCode()
        {
            return Cluster.Id ^ ColumnId ^ Nominal.Id;
        }
    }
}