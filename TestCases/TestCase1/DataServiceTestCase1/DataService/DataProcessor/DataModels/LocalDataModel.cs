using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class ProcessedDataModel
    {
        public Dictionary<string, DateTime> MeterTimestamp { get; set; }
    }

    public class ProcessedSensorDataModel
    {
        public DateTime ProcessedTimestamp { get; set; }
    }
}
