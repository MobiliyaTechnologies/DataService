using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static PiServerSimulator.GetPowerGridView;

namespace PiServerSimulator
{
    public class Program
    {
        static void Main(string[] args)
        {
            ThreadStart t1 = new ThreadStart(repeatProcess);
            Thread childMeterThread = new Thread(t1);
            childMeterThread.Start();
        }
        static System.Timers.Timer processTimer;
        private static void repeatProcess()
        {
            try
            {
                processTimer = new System.Timers.Timer(60000);
                processTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
                processTimer.Start();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("repeatProcess exception occured: ", e.Message);
            }
        }
        public static int Id = 0;
        static bool IsBacklogFinished = false;
        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            processTimer.Stop();
            try
            {
                
                 DateTime timeStamp = DateTime.UtcNow;
                DateTime previousDate= timeStamp;
                
                ///Get Previous Data///////////////
                ///
                if(!IsBacklogFinished)
                {

                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
                {
                    sqlConnection.Open();
                    string getMaxTimeStampQuery = "select MAX(TimeStamp) from PowergridView";
                    using (SqlCommand cmd = new SqlCommand(getMaxTimeStampQuery, sqlConnection))
                    {
                        
                            DateTime.TryParse(cmd.ExecuteScalar().ToString(), out previousDate);
                        
                    }
                    string getMaxIdQuery = "select MAX(Id) from PowergridView";
                    using (SqlCommand cmd = new SqlCommand(getMaxIdQuery, sqlConnection))
                    {

                        Id = Convert.ToInt32(cmd.ExecuteScalar());

                    }

                    if (previousDate!=timeStamp)
                    {
                        AddPreviousData(timeStamp, previousDate);
                        IsBacklogFinished = true;
                    }
                    else
                    {
                        IsBacklogFinished = true;
                    }
                }
                    var kist = new int[] { };
                }
                
                Console.WriteLine("Going to enter time entry: " + timeStamp.ToString());
                for (int building = 0; building < GetPowerGridView.BuildingName.Length; building++)
                {
                    Building buildingInfo = new Building(GetPowerGridView.BuildingName[building]);
                    GetPowerGridView powerGridView = new GetPowerGridView();
                    powerGridView.AddPowerGridView(buildingInfo, timeStamp);
                }
                GetSensorData sensorData = new GetSensorData();
                sensorData.AddSensorData(timeStamp);
                //GetWeather weatherData = new GetWeather();
                //weatherData.AddWeatherData(timeStamp);
                Console.WriteLine("Complete time entry: " + timeStamp.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured " + ex.Message);
            }
            processTimer.Start();
        }

        static void AddPreviousData(DateTime currentDate,DateTime previousDate)
        {
            int minutesDifference = Convert.ToInt32((currentDate - previousDate).TotalMinutes);
            for (int i = 1; i <= minutesDifference; i++)
            {
                DateTime entryTime = previousDate.AddMinutes(i);
                for (int building = 0; building < GetPowerGridView.BuildingName.Length; building++)
                {
                    Building buildingInfo = new Building(GetPowerGridView.BuildingName[building]);
                    GetPowerGridView powerGridView = new GetPowerGridView();
                    powerGridView.AddPowerGridView(buildingInfo, entryTime);
                }
            }
        }

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = new Random().NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
    }
}
