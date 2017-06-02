using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class CloudConfigurationSettingsModel
    {
        public string ApplicationId { get; set; }
        public string NotificationAuthorizationKey { get; set; }
        public string NotificationSender { get; set; }
        public string NotificationReceiver { get; set; }
        public string FCMURL { get; set; }
        public string ClickAction { get; set; }
        public string Icon { get; set; }
        public string ConfigurationKey { get; set; }
        public string ConfigurationValue { get; set; }
        public string BlobStorageURL { get; set; }
        public string ApiKey { get; set; }
        public string DatabaseURL { get; set; }
        public string StorageBucket { get; set; }
        public string AuthDomain { get; set; }

    }
}


