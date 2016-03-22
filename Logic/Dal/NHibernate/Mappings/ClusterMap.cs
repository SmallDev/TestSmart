using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class ClusterMap : ClassMap<ClusterDto>
    {
        public ClusterMap()
        {
            Table("Cluster");

            Id(d => d.Id).GeneratedBy.Identity();
            Map(d => d.Name).Length(50);

            HasMany(dto => dto.Sizes).KeyColumn("ClusterId").LazyLoad();
        }
    }
}