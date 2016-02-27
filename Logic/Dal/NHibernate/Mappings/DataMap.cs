using FluentNHibernate.Mapping;
using Logic.Dal.NHibernate.Models;
using Logic.Model;

namespace Logic.Dal.NHibernate.Mappings
{
    class DataMap : ClassMap<DataDto>
    {
        public DataMap()
        {
            Table("Data");

            Id(d => d.Id).GeneratedBy.Identity();

            Map(d => d.Timestamp).CustomType("Timestamp").Not.Nullable();
            Map(d => d.Mac).Length(17).Not.Nullable();
            Map(d => d.MessageType).CustomType<MessageType>().Nullable();
            Map(d => d.ContentType).CustomType<ContentType>().Nullable();
        }
    }
}