using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Utils
{
    public static class ConfigurationSettings
    {
        public static readonly string ApplicationId = GetConfigData("ApplicationId");

        public static readonly string SenderId = GetConfigData("SenderId");

        public static readonly string Receiver = GetConfigData("Receiver");

        public static readonly string FCMURL = GetConfigData("FCMURL");

        public static readonly string ClickAction = GetConfigData("ClickAction");

        public static readonly string Icon = GetConfigData("Icon");

        public static readonly string AzureConnectionString = GetConfigData("AzureConnectionString");

        public static readonly string StorageConnectionString = GetConfigData("BlobStorageConnectionString");

        public static readonly string BlobContainerName = GetConfigData("BlobContainerName");

        public static readonly string PiServers = GetConfigData("PiServers");

        public static readonly string PIConnectionString = GetConfigData("PIConnectionString");
        
        private static string GetConfigData(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
