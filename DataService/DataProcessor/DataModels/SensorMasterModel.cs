using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class SensorMasterModel
    {
        public int Sensor_Id { get; set; }
        public string Sensor_Name { get; set; }
        public string PiServerName { get; set; }
        public Nullable<int> Room_Id { get; set; }        
    }
}
