using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Utils
{
    public class Utility
    {
        public static T GetDataFromLocal<T>(string fileName) where T : class
        {
            try
            {
                string str = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + fileName);
                return JsonConvert.DeserializeObject<T>(str);

            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        public static void SaveDataToLocal<T>(string fileName, T data) where T : class
        {
            try
            {
                string jsonStr = JsonConvert.SerializeObject(data);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + fileName, jsonStr);
            }
            catch (Exception e)
            {
            }
        }
        /// <summary>
        /// Method accepts a date and removes seconds from it
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime TrimDateToMinute(DateTime date)
        {
            return new DateTime(
                date.Year,
                date.Month,
                date.Day,
                date.Hour,
                date.Minute,
                0);
        }

        public static void SendNotification(string title, string body)
        {
            if (!((string.IsNullOrEmpty(ConfigurationSettings.ApplicationId)) || (string.IsNullOrEmpty(ConfigurationSettings.SenderId)) || (string.IsNullOrEmpty(ConfigurationSettings.Receiver))
                || (string.IsNullOrEmpty(ConfigurationSettings.ClickAction)) || (string.IsNullOrEmpty(ConfigurationSettings.Icon)) || (string.IsNullOrEmpty(ConfigurationSettings.FCMURL))))
            {
                try
                {
                    //var applicationID = "AAAAGQBSH1c:APA91bEcYFZQMez7DyNTgphhxk1Sw4uKgss0xW7qBqiMX9QBHPNeIItIrw8VhvCJVWi8WUGUMPdRrx64P82lUtzmPUdvKFKYdr_UJHQl6lnWrXeK0J6-QHZaqkhsAKw1J3TwUievGRA2";
                    var applicationID = ConfigurationSettings.ApplicationId;

                    //var senderId = "107379564375";
                    var senderId = ConfigurationSettings.SenderId;

                    //string receiver = "/topics/Alerts";
                    string receiver = ConfigurationSettings.Receiver;

                    //WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
                    WebRequest tRequest = WebRequest.Create(ConfigurationSettings.FCMURL);
                    tRequest.Method = "post";
                    tRequest.ContentType = "application/json";
                    var data = new
                    {
                        //data = new
                        //{
                        notification = new
                        {
                            body = body,
                            title = title,
                            click_action = ConfigurationSettings.ClickAction,
                            icon = ConfigurationSettings.Icon,
                            sound = "default"
                            //  }
                        },
                        to = receiver
                    };

                    var json = JsonConvert.SerializeObject(data);

                    //var json = "{ \"notification\": {\"title\": \"Notification from csu\",\"text\": \"Notification from csu\",\"click_action\": \"http://localhost:65159/#/login \"},\"to\" : \"/topics/Alerts\"}";

                    Byte[] byteArray = Encoding.UTF8.GetBytes(json);
                    tRequest.Headers.Add(string.Format("Authorization: key={0}", applicationID));
                    tRequest.Headers.Add(string.Format("Sender: id={0}", senderId));
                    tRequest.ContentLength = byteArray.Length;

                    using (Stream dataStream = tRequest.GetRequestStream())
                    {
                        dataStream.Write(byteArray, 0, byteArray.Length);
                        using (WebResponse tResponse = tRequest.GetResponse())
                        {
                            using (Stream dataStreamResponse = tResponse.GetResponseStream())
                            {
                                using (StreamReader tReader = new StreamReader(dataStreamResponse))
                                {
                                    String sResponseFromServer = tReader.ReadToEnd();
                                    string str = sResponseFromServer;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string str = ex.Message;
                }

            }
        }
    }
}
