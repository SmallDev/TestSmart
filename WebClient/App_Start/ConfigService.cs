using System;
using System.Configuration;
using Logic.Dal;

namespace WebClient
{
    class ConfigService : IDbConfig
    {
        public ConfigService()
        {
            
        }
        public String ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["DbConnection"].ConnectionString;
            }
        }
    }
}
