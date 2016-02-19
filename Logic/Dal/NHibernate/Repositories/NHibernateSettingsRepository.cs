using System;
using System.Globalization;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class NHibernateSettingsRepository :  NHibernateRepositoryBase, ISettingsRepository
    {
        private const String TimeFormat = "g";
        public NHibernateSettingsRepository(ISession session)
            : base(session)
        {
        }

        public TimeSpan? GetReadTime()
        {
            return Get(SettingsDto.SettingName.ReadTime, TimeSpan.Parse);
        }
        public void SetReadTime(TimeSpan? time)
        {
            Set(SettingsDto.SettingName.ReadTime, time, t => t.ToString(TimeFormat));
        }

        public Double? GetReadVelocity()
        {
            return Get(SettingsDto.SettingName.ReadVelocity, s => Convert.ToDouble(s, CultureInfo.InvariantCulture));
        }
        public void SetReadVelocity(Double? velocity)
        {
            Set(SettingsDto.SettingName.ReadVelocity, velocity, v => Convert.ToString(v, CultureInfo.InvariantCulture));
        }

        public TimeSpan? GetCalcTime()
        {
            return Get(SettingsDto.SettingName.CalcTime, TimeSpan.Parse);
        }
        public void SetCalcTime(TimeSpan? time)
        {
            Set(SettingsDto.SettingName.CalcTime, time, t => t.ToString(TimeFormat));
        }

        public TimeSpan? GetAllTime()
        {
            return Get(SettingsDto.SettingName.AllTime, TimeSpan.Parse);
        }
        public void SetAllTime(TimeSpan? time)
        {
            Set(SettingsDto.SettingName.AllTime, time, t => t.ToString(TimeFormat));
        }

        private T? Get<T>(SettingsDto.SettingName name, Func<String, T> parse) where T: struct
        {
            var dto = Session.Get<SettingsDto>(name.ToString());
            if (dto == null || String.IsNullOrEmpty(dto.Value))
                return null;

            return parse(dto.Value);
        }
        private void Set<T>(SettingsDto.SettingName name, T? value, Func<T, String> serialize) where T : struct
        {
            var dto = Session.Get<SettingsDto>(name.ToString()) ?? new SettingsDto {Name = name.ToString()};

            dto.Value = value.HasValue ? serialize(value.Value) : null;
            Session.Save(dto);
        }
    }
}