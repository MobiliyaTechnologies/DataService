using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class ConfigurationSettingsModel
    {
        public string ApplicationId { get; set; }
        public string SenderId { get; set; }
        public string Receiver { get; set; }
        public string FCMURL { get; set; }
        public string ClickAction { get; set; }
        public string Icon { get; set; }
    }
}


