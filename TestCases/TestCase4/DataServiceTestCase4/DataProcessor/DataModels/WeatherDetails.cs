using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class WeatherDetails
    {
        public int Temperature { get; set; }
        public int RelativeHumidity { get; set; }
        public int Pressure { get; set; }
        public int Visibility { get; set; }
    }
}
