using System;

namespace Logic.Dal.NHibernate.Models
{
    internal class SettingsDto
    {
        public enum SettingName
        {
            AllTime,
            ReadTime,
            ReadVelocity,
            CalcTime,
            CalcVelocity,
        }

        public virtual String Name { get; set; }
        public virtual String Value { get; set; }
    }
}
