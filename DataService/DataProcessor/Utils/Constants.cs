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
        public const string CLOUD_CONFIGURATION_TYPE_FIREBASE = "Firebase";
        public const string CLOUD_CONFIGURATION_TYPE_BLOB_STORAGE = "BlobStorage";
        public const string THRESHOLD_METER_STORAGE_FILENAME_PREFIX = "ProcessedMeterDataInfo_";
        public const string THRESHOLD_SENSOR_STORAGE_FILENAME_PREFIX = "ProcessedSensorDataInfo_";
        public const double TIME_WINDOW_FOR_HALF_HOURLY_DATA = 30;
        public const double SENSOR_DATA_THREAD_TIMER = 120000;
        public const string CLASS_SCHEDULE_STORAGE_FILE_PREFIX = "ClassSchedule_";
        public const string CLASS_SCHEDULE_STORAGE_FILE_EXTENSION = ".csv";
        public static string BLOB_STOARAGE_CONTAINER = "energymanagementcontainer";

        public const string CLOUD_CONFIGURATION_FIREBASE_API_KEY = "ApiKey";
        public const string CLOUD_CONFIGURATION_NOTIFICATION_SENDER_KEY = "NotificationSender";
        public const string CLOUD_CONFIGURATION_NOTIFICATION_RECEIVER_KEY = "NotificationReceiver";
        public const string CLOUD_CONFIGURATION_NOTIFICATIONAUTHORIZATION_Key = "NotificationAuthorizationKey";
        public const string CLOUD_CONFIGURATION_NOTIFICATION_CLICK_ACTION_KEY = "NotificationClickAction";
        public const string CLOUD_CONFIGURATION_FIREBASE_SEND_URL_KEY = "https://fcm.googleapis.com/fcm/send";
        public const string CLOUD_CONFIGURATION_STORAGE_CONNECTION_STRING_KEY = "BlobStorageConnectionString";
    }
}
