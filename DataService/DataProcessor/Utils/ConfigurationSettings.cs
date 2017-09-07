using DataProcessor.DataModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Utils
{
    public static class ConfigurationSettings
    {
        
        public static readonly string AzureConnectionString = GetConfigData("AzureConnectionString");
  

        public static readonly string PiServer = GetConfigData("PiServer");

        public static readonly string StorageConnectionString = GetConfigData("StorageConnectionString");

        public static readonly string PiServerConnectionString = GetConfigData("PiServerConnectionString");

        

        static CloudConfigurationSettingsModel cloudConfiguration = new CloudConfigurationSettingsModel();

        private static string GetConfigData(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
        
        public static void SetCloudConfigDataModel(CloudConfigurationSettingsModel configSetting)
        {
            if(configSetting!=null)
            {
                cloudConfiguration = configSetting;
            }
        }
        public static CloudConfigurationSettingsModel GetCloudConfigDataModel()
        {
            return cloudConfiguration;
        }
        
    }
}
