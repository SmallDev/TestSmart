using System;
using System.Collections.Specialized;
using System.Configuration;
using Logic.Dal;
using Logic.Facades;

namespace WebClient
{
    class ConfigService : IDbConfig, IEmulatorConfig
    {
        private const String AppSettingsSection = "appSettings";
        private const String ConnectionStringsSection = "connectionStrings";

        public String ConnectionString
        {
            get { return GetKey(ConnectionStringsSection, "DbConnection"); }
        }

        public String FileName
        {
            get { return GetKey(AppSettingsSection, "FileName"); }
        }

        private static String GetKey(String sectionName, String key, Boolean required = true)
        {
            String value;
            if (String.Equals(sectionName, AppSettingsSection, StringComparison.InvariantCultureIgnoreCase))
                value = ConfigurationManager.AppSettings[key];

            else if (String.Equals(sectionName, ConnectionStringsSection, StringComparison.InvariantCultureIgnoreCase))
            {
                var strings = ConfigurationManager.ConnectionStrings[key];
                value = strings != null ? strings.ConnectionString : null;
            }

            else
            {
                var section = ((NameValueCollection) ConfigurationManager.GetSection(sectionName));
                if (section == null)
                    throw new ConfigurationErrorsException(String.Format("Missing section: {0}", sectionName));

                value = section.Get(key);
            }

            if (required && String.IsNullOrEmpty(value))
                throw new ConfigurationErrorsException(String.Format("Missing key: {0}/{1}", sectionName, key));

            return value;
        }
    }
}
