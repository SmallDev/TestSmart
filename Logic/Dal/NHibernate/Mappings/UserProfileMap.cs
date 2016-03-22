using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;

namespace Logic.Dal.NHibernate.Mappings
{
    class UserProfileMap : ClassMap<UserProfileDto>
    {
        public UserProfileMap()
        {
            Table("UserProfile");

            CompositeId()
                .KeyReference(dto => dto.User, "UserId")
                .KeyReference(dto => dto.Cluster, "ClusterId");

            Map(dto => dto.Probability).Not.Nullable();
        }
    }
}