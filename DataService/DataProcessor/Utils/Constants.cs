using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Utils
{
    public class Constants
    {
        public const string DATE_TIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public const string THRESHOLD_METER_STORAGE_FILENAME_PREFIX = "ProcessedMeterDataInfo_";
        public const string THRESHOLD_SENSOR_STORAGE_FILENAME_PREFIX = "ProcessedSensorDataInfo_";
        public const double TIME_WINDOW_FOR_HALF_HOURLY_DATA = 30;
        public const double SENSOR_DATA_THREAD_TIMER = 120000;
        public const string CLASS_SCHEDULE_STORAGE_FILE_PREFIX = "ClassSchedule_";
        public const string CLASS_SCHEDULE_STORAGE_FILE_EXTENSION = ".csv";
        public const string BLOB_STOARAGE_CONTAINER = "EnergyManagementContainer";


    }
}
