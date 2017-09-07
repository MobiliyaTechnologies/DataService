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
        //testcase1

        static void Main(string[] args)
        {
            ThreadStart t1 = new ThreadStart(repeatProcess);
            Thread childMeterThread = new Thread(t1);
            childMeterThread.Start();
        }

        private static void repeatProcess()
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


        //for two building and 7 power scouts per building
        /*
        static void Main(string[] args)
        {
            //GetPowerGridView powerGridView = new GetPowerGridView();
            //for (int building = 0; building < GetPowerGridView.BuildingName.Length; building++)
            //{
            //    ThreadStart t1 = new ThreadStart(new Processor(GetPowerGridView.BuildingName[building]).repeatProcess);
            //    Thread childMeterThread = new Thread(t1);
            //    childMeterThread.Start();
            //}
            for (int building = 0; building < GetPowerGridView.BuildingName.Length; building++)
            {
                try
                {
                    Building buildingInfo = new Building(GetPowerGridView.BuildingName[building]);
                    GetPowerGridView powerGridView = new GetPowerGridView();
                    powerGridView.AddPowerGridView(buildingInfo);
                }
                catch (Exception e)
                {


                }
            }
            //GetSensorData sensorData = new GetSensorData();
            //sensorData.AddSensorData();
            //GetWeather weatherData = new GetWeather();
            //weatherData.AddWeatherData();


        }

        private static void repeatProcess()
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
            GetPowerGridView powerGridView = new GetPowerGridView();
            GetSensorData sensorData = new GetSensorData();
            sensorData.AddSensorData();
            GetWeather weatherData = new GetWeather();
            weatherData.AddWeatherData();
        }

        public static double RandomNumberBetween(double minValue, double maxValue)
        {
            var next = new Random().NextDouble();

            return minValue + (next * (maxValue - minValue));
        }

        class Processor
        {

            string building = "";
            public Processor(string building)
            {
                this.building = building;
            }

            public void repeatProcess()
            {
                Building buildingInfo = new Building(this.building);
                GetPowerGridView powerGridView = new GetPowerGridView();
                powerGridView.AddPowerGridView(buildingInfo);
            }
        }*/
    }
}
