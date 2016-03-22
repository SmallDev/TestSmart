using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class SizeMap : ClassMap<SizeDto>
    {
        public SizeMap()
        {
            Table("ClusterSize");

            CompositeId()
                .KeyReference(dto => dto.Learning, "LearningId")
                .KeyReference(dto => dto.Cluster, "ClusterId");

            Map(dto => dto.Size).Not.Nullable();
        }
    }
}