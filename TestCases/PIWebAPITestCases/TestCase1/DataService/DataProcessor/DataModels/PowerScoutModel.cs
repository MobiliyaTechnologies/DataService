using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class MeterInfoModel
    {
        public string PowerScout { get; set; }
        public string Breaker_details { get; set; }
        public string BuildingName { get; set; }
        public string PiServerName { get; set; }
        public double UTCConversionTime { get; set; }
        public int BuildingId { get; set; }
    }
}
