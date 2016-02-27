using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class UserMap : ClassMap<UserDto>
    {
        public UserMap()
        {
            Table("[User]");

            Id(d => d.Id).GeneratedBy.Identity();
            Map(d => d.Mac).Length(20);
        }
    }
}