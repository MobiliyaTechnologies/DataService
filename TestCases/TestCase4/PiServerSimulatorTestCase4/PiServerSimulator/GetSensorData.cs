using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PiServerSimulator
{
    public class GetSensorData
    {

        static Random rnd;

        static double brightness = 255.857727050781*0.5;
        static double humidity_min = 36.6181411743164*0.5;
        static double humidity_max = 52.557502746582*0.5;
        static double temperature_min = 61.5705364227295*0.6;
        static double temperature_max = 73.4625259399414*0.6;
        static double pIIntTSTicks_min = 636226286370000000*0.9;
        static double pIIntTSTicks_max = 636289634974586426*0.9;

        public void AddSensorData(DateTime timeStamp)
        {
            rnd = new Random();

            string[] wirelessTagTemplate = new string[] { "Light Sensor 1", "Office 1" };

            string sensorDataInsertQuery = "INSERT INTO SensorData([Wireless Tag Template],[TimeStamp],[Brightness],[Humidity],[Name],[Temperature],[PIIntTSTicks],[PIIntShapeID]) VALUES (@WirelessTagTemplate,@TimeStamp,@Brightness,@Humidity,@Name,@Temperature,@PIIntTSTicks,@PIIntShapeID)";

            int number = rnd.Next(0, wirelessTagTemplate.Length - 1);

            // int number = rnd.Next(0, powerScout.Length - 1);

            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
            {
                sqlConnection.Open();
                using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                {
                   
                        
                        using (SqlCommand cmd = new SqlCommand(sensorDataInsertQuery, sqlConnection, sqlTransaction))
                        {

                            cmd.Parameters.AddWithValue("@WirelessTagTemplate", wirelessTagTemplate[number]);
                            cmd.Parameters.AddWithValue("@TimeStamp", timeStamp);
                            cmd.Parameters.AddWithValue("@Brightness", Program.RandomNumberBetween(0, brightness));
                            cmd.Parameters.AddWithValue("@Humidity", Program.RandomNumberBetween(humidity_min, humidity_max));
                            cmd.Parameters.AddWithValue("@Name", wirelessTagTemplate[number]);
                            cmd.Parameters.AddWithValue("@Temperature", Program.RandomNumberBetween(temperature_min, temperature_max));
                            cmd.Parameters.AddWithValue("@PIIntTSTicks", Program.RandomNumberBetween(pIIntTSTicks_min, pIIntTSTicks_max));
                            cmd.Parameters.AddWithValue("@PIIntShapeID", rnd.Next(1, 3));


                            cmd.ExecuteNonQuery();
                        }
                        
                        sqlTransaction.Commit();
                    
                }

                sqlConnection.Close();

            }
        }
        
    }
}
