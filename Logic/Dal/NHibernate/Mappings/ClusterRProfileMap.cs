using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class ClusterRProfileMap : ClassMap<ClusterRProfileDto>
    {
        public ClusterRProfileMap()
        {
            Table("ClusterRProfile");

            CompositeId()
                .KeyReference(dto => dto.Cluster, "ClusterId")
                .KeyProperty(dto => dto.ColumnId, "PropertyId");

            Map(dto => dto.Mean).Not.Nullable();
            Map(dto => dto.Variance).Not.Nullable();
        }
    }
}