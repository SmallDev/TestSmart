using System;

namespace Logic.Dal.NHibernate.Models
{
    class SettingsDto
    {
        public const String ReadTimeStamp = "TimeStamp";

        public virtual String Name { get; set; }
        public virtual String Value { get; set; }
    }
}
