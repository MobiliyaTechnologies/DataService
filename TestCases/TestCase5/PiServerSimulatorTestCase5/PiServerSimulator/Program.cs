using System;
using System.Collections.Generic;
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
            try
            {
                DateTime timeStamp = DateTime.UtcNow;
                Console.WriteLine("Going to enter time entry: " + timeStamp.ToString());
                for (int building = 0; building < GetPowerGridView.BuildingName.Length; building++)
                {
                    Building buildingInfo = new Building(GetPowerGridView.BuildingName[building]);
                    GetPowerGridView powerGridView = new GetPowerGridView();
                    powerGridView.AddPowerGridView(buildingInfo, timeStamp);
                }
                GetSensorData sensorData = new GetSensorData();
                sensorData.AddSensorData(timeStamp);
                GetWeather weatherData = new GetWeather();
                weatherData.AddWeatherData(timeStamp);
                Console.WriteLine("Complete time entry: " + timeStamp.ToString());
                //ThreadStart t1 = new ThreadStart(RepeatProcess);
                //Thread childMeterThread = new Thread(t1);
                //childMeterThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured " + ex.Message);
            }
        }

        private static void RepeatProcess()
        {
            try
            {
                var processTimer = new System.Timers.Timer(60000);
                processTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimerElapsed);
                processTimer.Start();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("repeatProcess exception occured: ", e.Message);
            }
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            DateTime timeStamp = DateTime.UtcNow;
            Console.WriteLine("Going to enter time entry: " + timeStamp.ToString());
            for (int building = 0; building < GetPowerGridView.BuildingName.Length; building++)
            {
                Building buildingInfo = new Building(GetPowerGridView.BuildingName[building]);
                GetPowerGridView powerGridView = new GetPowerGridView();
                powerGridView.AddPowerGridView(buildingInfo, timeStamp);
            }
            GetSensorData sensorData = new GetSensorData();
            sensorData.AddSensorData(timeStamp);
            GetWeather weatherData = new GetWeather();
            weatherData.AddWeatherData(timeStamp);
            Console.WriteLine("Complete time entry: " + timeStamp.ToString());
        }

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = new Random().NextDouble();

            return minValue + (next * (maxValue - minValue));
        }
    }
}
