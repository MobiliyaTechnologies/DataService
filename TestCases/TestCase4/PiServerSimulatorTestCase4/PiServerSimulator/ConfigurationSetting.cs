using System;
using System.Configuration;

namespace PiServerSimulator
{
    public class ConfigurationSetting
    {

        public static readonly string ConnectionString = GetConfigData("PiConnectionString");


        private static string GetConfigData(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
