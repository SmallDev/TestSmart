using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;
using NHibernate.Type;

namespace Logic.Dal.NHibernate.Mappings
{
    class LearningMap : ClassMap<LearningDto>
    {
        public LearningMap()
        {
            Table("Learning");

            Id(l => l.Id).GeneratedBy.Identity();
            Map(l => l.From).Column("[From]").CustomType<TimeAsTimeSpanType>().Not.Nullable();
            Map(l => l.To).Column("[To]").CustomType<TimeAsTimeSpanType>().Not.Nullable();
            Map(l => l.StartLikelihood);
            Map(l => l.EndLikelihood);

            Map(l => l.CreatedOn);
        }
    }
}