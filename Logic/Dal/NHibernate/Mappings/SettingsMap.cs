using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class SettingsMap : ClassMap<SettingsDto>
    {
        public SettingsMap()
        {
            Table("Settings");

            Id(dto => dto.Name).Not.Nullable();
            Map(s => s.Value).Nullable();
        }
    }
}
