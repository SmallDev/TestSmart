using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class ClusterNProfileMap : ClassMap<ClusterNProfileDto>
    {
        public ClusterNProfileMap()
        {
            Table("ClusterNProfile");

            CompositeId()
                .KeyReference(dto => dto.Cluster, "ClusterId")
                .KeyProperty(dto => dto.ColumnId, "PropertyId")
                .KeyReference(dto => dto.Nominal, "NominalId");

            Map(dto => dto.Probability).Not.Nullable();
        }
    }
}