using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PiServerSimulator
{
    public class GetWeather
    {
        static Random rnd;

        static double Pressure_min = 29.16*0.5;
        static double Pressure_max = 30.5*0.5;
        static double temperature_min = -17 * 0.5;
        static double temperature_max = 91 * 0.5;
        static double pIIntTSTicks_min = 636226286370000000 * 0.9;
        static double pIIntTSTicks_max = 636289634974586426 * 0.5;
        static string weather = "Weather";

        public void AddWeatherData(DateTime timeStamp)
        {
            rnd = new Random();

            string[] windDirection = new string[] {"East", "North", "Northeast", "Northwest", "South", "Southeast", "Southwest", "Variable", "West" };

            string[] weatherWeather = new string[] { "A Few Clouds", "A Few Clouds and Breezy", "A Few Clouds and Windy", "Fair", "Fair and Breezy", "Fair and Windy", "Fair with Haze", "Fog", "Fog/Mist", "Freezing Fog", "Heavy Rain", "Light Rain", "Light Rain and Breezy", "Light Rain and Windy", "Light Rain Fog", "Light Rain Fog/Mist", "Light Snow", "Light Snow Fog", "Light Snow Fog/Mist", "Mostly Cloudy", "Mostly Cloudy and Breezy", "Mostly Cloudy and Windy", "Overcast", "Overcast and Breezy", "Overcast with Haze", "Partly Cloudy", "Partly Cloudy and Breezy", "Partly Cloudy and Windy", "Rain", "Rain Fog/Mist", "Sky Obscured", "Snow Freezing Fog", "Thunderstorm", "Thunderstorm in Vicinity", "Thunderstorm Light Rain", "Thunderstorm Light Rain and Breezy", "Thunderstorm Rain", "Unknown Precip", "Unknown Precip Fog/Mist" };

            string sensorDataInsertQuery = "INSERT INTO Weather([Weather],[TimeStamp],[Pressure],[Relative Humidity],[Temperature],[Visibility],[Weather.Weather],[Wind Direction],[Wind Speed],[PIIntTSTicks],[PIIntShapeID]) VALUES (@Weather,@TimeStamp,@Pressure,@RelativeHumidity,@Temperature,@Visibility,@WeatherWeather,@WindDirection,@WindSpeed,@PIIntTSTicks,@PIIntShapeID)";


            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
            {
                sqlConnection.Open();
                using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                {
                       
                        using (SqlCommand cmd = new SqlCommand(sensorDataInsertQuery, sqlConnection, sqlTransaction))
                        {

                            cmd.Parameters.AddWithValue("@Weather", weather);
                            cmd.Parameters.AddWithValue("@TimeStamp", timeStamp);
                            cmd.Parameters.AddWithValue("@Pressure", Program.RandomNumberBetween(Pressure_min, Pressure_max));
                            cmd.Parameters.AddWithValue("@RelativeHumidity", rnd.Next(6, 100));
                            cmd.Parameters.AddWithValue("@Temperature", Program.RandomNumberBetween(temperature_min, temperature_max));
                            cmd.Parameters.AddWithValue("@Visibility", Program.RandomNumberBetween(0.15, 10));
                            cmd.Parameters.AddWithValue("@WeatherWeather", weatherWeather[rnd.Next(0, weatherWeather.Length - 1)]);
                            cmd.Parameters.AddWithValue("@WindDirection", windDirection[rnd.Next(0, windDirection.Length - 1)]);
                            cmd.Parameters.AddWithValue("@WindSpeed", Program.RandomNumberBetween(0, 39.1));
                            cmd.Parameters.AddWithValue("@PIIntTSTicks", Program.RandomNumberBetween(pIIntTSTicks_min, pIIntTSTicks_max));
                            cmd.Parameters.AddWithValue("@PIIntShapeID", 1);

                            cmd.ExecuteNonQuery();

                        }
                    sqlTransaction.Commit();
                }

                sqlConnection.Close();

            }
        }

    }
}
