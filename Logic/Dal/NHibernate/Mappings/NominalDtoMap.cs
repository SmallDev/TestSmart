using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class NominalDtoMap : ClassMap<NominalDto>
    {
        public NominalDtoMap()
        {
            Table("Nominal");

            Id(dto => dto.Id).GeneratedBy.Increment();

            Map(dto => dto.ColumnId).Column("PropertyId").Not.Nullable();
            Map(dto => dto.Value).Column("Value").Not.Nullable();
        }
    }
}