using System;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class NHibernateSettingsRepository :  NHibernateRepositoryBase, ISettingsRepository
    {
        public NHibernateSettingsRepository(ISession session)
            : base(session)
        {
        }

        public DateTime? GetReadTimestamp()
        {
            var dto = Session.Get<SettingsDto>(SettingsDto.ReadTimeStamp);
            if (dto == null || String.IsNullOrEmpty(dto.Value))
                return null;

            return DateTime.Parse(dto.Value);
        }

        public void SetReadTimestamp(DateTime? dateTime)
        {
            var dto = Session.Get<SettingsDto>(SettingsDto.ReadTimeStamp) ??
                      new SettingsDto {Name = SettingsDto.ReadTimeStamp};

            dto.Value = dateTime.HasValue ? dateTime.Value.ToString("s") : null;
            Session.Save(dto);
        }
    }
}