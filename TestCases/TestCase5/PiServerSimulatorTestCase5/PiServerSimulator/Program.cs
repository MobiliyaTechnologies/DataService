using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
        static System.Timers.Timer processTimer;
        public static int Id = 0;
        static bool IsBacklogFinished = false;
        static int previousPendingDays = 0;
        static int sensorCount = 0;
        static void Main(string[] args)
        {
            Console.WriteLine("Enter Number of previous days for generating data:: ");
            string days = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(days))
            {
                previousPendingDays = 0;
            }
            else
            {
                previousPendingDays = Convert.ToInt32(days);
            }
            if (previousPendingDays > 0)
            {
                Console.WriteLine("You opted previous {0} days + realtime data generation", previousPendingDays);
            }
            else
            {
                Console.WriteLine("You opted realtime data generation.");
            }
            Console.WriteLine("Enter Number of Sensors want to add for generating data:: ");
            string sensors = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(sensors))
            {
                sensorCount = 0;
            }
            else
            {
                sensorCount = Convert.ToInt32(sensors);
            }
            if (sensorCount > 0)
            {
                Console.WriteLine("You opted {0} sensors to add for data generation", sensorCount);
            }
            else
            {
                Console.WriteLine("You opted 0 sensors for data generation.");
            }

            ThreadStart t1 = new ThreadStart(repeatProcess);
            Thread childMeterThread = new Thread(t1);
            childMeterThread.Start();
            Console.WriteLine("Process Started..");
        }

        private static void repeatProcess()
        {
            try
            {
                CreateTableIfNotExist();
                DateTime timeStamp = DateTime.UtcNow;
                DateTime startDate = timeStamp;
                if (previousPendingDays > 0 && (!IsBacklogFinished))//means we have previous days to complete
                {
                    startDate = timeStamp.AddDays(-previousPendingDays);
                    using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
                    {
                        DateTime dbLastDate = DateTime.MinValue;
                        sqlConnection.Open();
                        string getMaxTimeStampQuery = "select MAX(TimeStamp) from PowergridView";
                        using (SqlCommand cmd = new SqlCommand(getMaxTimeStampQuery, sqlConnection))
                        {
                            DateTime.TryParse(cmd.ExecuteScalar().ToString(), out dbLastDate);
                        }
                        string getMaxIdQuery = "select MAX(Id) from PowergridView";
                        using (SqlCommand cmd = new SqlCommand(getMaxIdQuery, sqlConnection))
                        {
                            object maxId = cmd.ExecuteScalar();
                            Id = maxId != DBNull.Value ? Convert.ToInt32(maxId) : 0;
                        }

                        //Date Handling
                        if (!(dbLastDate == null || dbLastDate == DateTime.MinValue || dbLastDate == default(DateTime)))
                        {
                            startDate = dbLastDate;
                        }
                        AddPreviousData(timeStamp, startDate);
                    }
                }
                //Insert Sensor Data 
                GetSensorData sensorData = new GetSensorData();
                for (int i = 1; i <= sensorCount; i++)
                {
                    Random rnd = new Random();
                    int index = rnd.Next(0, sensorData.wirelessTagTemplate.Length);
                    sensorData.AddSensorData(timeStamp, sensorData.wirelessTagTemplate[index] + " " + i);
                    Console.WriteLine("Inserted Sensor : "+sensorData.wirelessTagTemplate[index] + " " + i);
                }

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
        static string[] TableNames = { "PowergridView", "Weather", "SensorData" };
        private static void CreateTableIfNotExist()
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
            {
                sqlConnection.Open();
                for(int name=0;name<TableNames.Length;name++)
                {
                    string cmdText = @"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES 
                       WHERE TABLE_NAME='" + TableNames[name] + "') SELECT 1 ELSE SELECT 0";
                    SqlCommand cmdTableCheck = new SqlCommand(cmdText, sqlConnection);
                    int x = Convert.ToInt32(cmdTableCheck.ExecuteScalar());
                    if (x == 0)
                    {
                        CreateTable(TableNames[name], sqlConnection);
                    }
                }

                sqlConnection.Close();
                
            }
        }
       
        static void CreateTable(string tableName,SqlConnection connection)
        {
            string query = GetTextFromFile(tableName + ".txt");
            SqlCommand cmdCreateTable = new SqlCommand(query, connection);
            cmdCreateTable.ExecuteNonQuery();
            Console.WriteLine("Table Created :: " + tableName);
        }
        static string GetTextFromFile(string filepath)
        {
            return File.ReadAllText(filepath);
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                DateTime timeStamp = DateTime.UtcNow;
                DateTime startDate = timeStamp;
                if(previousPendingDays <=0)
                {
                    IsBacklogFinished = true;
                }
                ///Get Previous Data///////////////
                if (!IsBacklogFinished)
                {

                    using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
                    {
                        DateTime dbLastDate = DateTime.MinValue;
                        sqlConnection.Open();
                        string getMaxTimeStampQuery = "select MAX(TimeStamp) from PowergridView";
                        using (SqlCommand cmd = new SqlCommand(getMaxTimeStampQuery, sqlConnection))
                        {
                            DateTime.TryParse(cmd.ExecuteScalar().ToString(), out dbLastDate);
                        }
                        string getMaxIdQuery = "select MAX(Id) from PowergridView";
                        using (SqlCommand cmd = new SqlCommand(getMaxIdQuery, sqlConnection))
                        {
                            object maxId = cmd.ExecuteScalar();
                            Id = maxId != DBNull.Value ? Convert.ToInt32(maxId) : 0;
                        }

                        //Date Handling
                        if (!(dbLastDate == null || dbLastDate == DateTime.MinValue || dbLastDate == default(DateTime)))
                        {
                            startDate = dbLastDate;
                        }
                        AddPreviousData(timeStamp, startDate);
                        IsBacklogFinished = true;
                    }
                }
                else
                {
                    Console.WriteLine("Going to enter time entry: " + timeStamp.ToString());
                    for (int building = 0; building < GetPowerGridView.BuildingName.Length; building++)
                    {
                        Building buildingInfo = new Building(GetPowerGridView.BuildingName[building]);
                        GetPowerGridView powerGridView = new GetPowerGridView();
                        powerGridView.AddPowerGridView(buildingInfo, timeStamp);
                    }
                }

                
               
              
                //GetWeather weatherData = new GetWeather();
                //weatherData.AddWeatherData(timeStamp);
                Console.WriteLine("Complete time entry: " + timeStamp.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured " + ex.Message);
            }
        }

        static void AddPreviousData(DateTime currentDate, DateTime previousDate)
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
                Console.Write("\r Inserted value for the Day {0} ", entryTime.Date);
            }
        }

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = new Random().NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
    }
}
