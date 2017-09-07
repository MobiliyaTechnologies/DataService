using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace PiServerSimulator
{
    public class Program
    {
        static void Main(string[] args)
        {
            GetPowerGridView powerGridView = new GetPowerGridView();
            powerGridView.AddPowerGridView();
            GetSensorData sensorData = new GetSensorData();
            sensorData.AddSensorData();
            GetWeather weatherData = new GetWeather();
            weatherData.AddWeatherData();
            //ThreadStart t1 = new ThreadStart(repeatProcess);
            //Thread childMeterThread = new Thread(t1);
            //childMeterThread.Start();
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
            powerGridView.AddPowerGridView();
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
    }
}
