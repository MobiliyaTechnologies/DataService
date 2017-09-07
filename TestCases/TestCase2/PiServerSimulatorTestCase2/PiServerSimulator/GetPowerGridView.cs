using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PiServerSimulator
{
    public class GetPowerGridView
    {

        static Random rnd;
        static double ampsl1 = 433.619331204961;
        static double ampsl2 = 438.770010374815 ;
        static double ampsl3 = 436.675966480134 ;
        static double ampsSystemAvg = 436.675966480134  ;
        static double dailyElectricCost = 198.915319668451 ;
        static double dailykWhSystem = 4555.0468125888  ;
        static double monthlyElectricCost = 5827.75588031122 ;
        static double monthlykWhSystem = 133449.871605242  ;
        static double ratedAmperage = 200  ;
        static DateTime readingTime = Convert.ToDateTime("2016-08-19 00:00:00.000");
        static double rollingHourlykWh = 208.126197596686  ;
        static string type = "PowerScout 3037";
        static double voltsL1toNeutral_min = 238.449543151905  ;
        static double voltsL2toNeutral_min = 237.714431773496  ;
        static double voltsL3toNeutral_min = 238.806858504314  ;
        static double voltsL1toNeutral_max = 286.766991385748  ;
        static double voltsL2toNeutral_max = 285.197925792541  ;
        static double voltsL3toNeutral_max = 286.596888688812  ;
        static double kWL1 = 75.6108646752933  ;
        static double kWL2 = 75.6830692949571  ;
        static double kWL3 = 75.1904180271698  ;
        static double kWSystem = 226.575086870641 ;
        static double pIIntTSTicks_min = 636089806874751616 ;
        static double pIIntTSTicks_max = 636289444874751616 ;
        static string building = "Chemistry Building";

        string powerGridInsertQuery = "INSERT INTO powergridview([PowerScout],[TimeStamp],[Amps L1],[Amps L2],[Amps L3],[Amps System Avg],[Breaker Details],[Breaker Label],[Building],[Daily Electric Cost],[Daily kWh System],[Modbus Base Address],[Monthly Electric Cost],[Monthly kWh System],[Rated Amperage],[ReadingTime],[Rolling Hourly kWh System],[Serial Number],[Type],[Volts L1 to Neutral],[Volts L2 to Neutral],[Volts L3 to Neutral],[kW L1],[kW L2],[kW L3],[kW System],[PIIntTSTicks],[PIIntShapeID]) VALUES (@PowerScout,@TimeStamp,@AmpsL1,@AmpsL2,@AmpsL3,@AmpsSystemAvg,@BreakerDetails,@BreakerLabel,@Building,@DailyElectricCost,@DailykWhSystem,@ModbusBaseAddress,@MonthlyElectricCost,@MonthlykWhSystem,@RatedAmperage,@ReadingTime,@RollingHourlykWhSystem,@SerialNumber,@Type,@VoltsL1toNeutral,@VoltsL2toNeutral,@VoltsL3toNeutral,@kWL1,@kWL2,@kWL3,@kWSystem,@PIIntTSTicks,@PIIntShapeID)";


        public void AddPowerGridView()
        {
            rnd = new Random();

            string[] powerScout = new string[] { "P371602068", "P371602070", "P371602072", "P371602073", "P371602075", "P371602077", "P371602079" };
            string[] breakerDetails = new string[] { "New (2013) 3rd floor panel - almost empty", "800A duct bank for panels B,C,D - 277v lighting", "D-Wing Addition MDP", "A-Wing Lecture halls", "208v Panel in room C2, Xfmr on roof above.", "Basement MCC", "2nd Floor Xfmr supplying panels L,M,N,U,U1,U2" };
            string[] breakerLabel = new string[] { "PP31 - 3rd Fl Electrical Rm", "Main Lighting", "MCC", "F", "LCA", "MCC", "T-1" };
            int[] modbusBaseAddress = new int[] { 2, 5, 6, 4, 3, 8, 7 };


            int number = rnd.Next(0, powerScout.Length - 1);
            AzurePowerGridModel azurePowerGridModel = new AzurePowerGridModel();
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
            {
                sqlConnection.Open();
                using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                {
                    DateTime currentTime = DateTime.UtcNow.AddMonths(-3);
                    for (int j = 0; j <= 126000; j++)
                    {
                        DateTime timeStamp = currentTime.AddMinutes(j);
                        for (int i = 0; i < powerScout.Length; i++)
                        {
                            azurePowerGridModel = new AzurePowerGridModel();
                            azurePowerGridModel.PowerScout = powerScout[i];
                            azurePowerGridModel.Timestamp = timeStamp;
                            azurePowerGridModel.AMPS_L1 = Program.RandomNumberBetween(0, ampsl1);
                            azurePowerGridModel.AMPS_L2 = Program.RandomNumberBetween(0, ampsl2);
                            azurePowerGridModel.AMPS_L3 = Program.RandomNumberBetween(0, ampsl3);
                            azurePowerGridModel.AMPS_SYSTEM_AVG = Program.RandomNumberBetween(0, ampsSystemAvg);
                            azurePowerGridModel.Breaker_details = breakerDetails[i];
                            azurePowerGridModel.Breaker_label = breakerLabel[i];
                            azurePowerGridModel.Building = building;
                            azurePowerGridModel.Daily_electric_cost = Program.RandomNumberBetween(0, dailyElectricCost);
                            azurePowerGridModel.Daily_KWH_System = Program.RandomNumberBetween(0, dailykWhSystem);
                            azurePowerGridModel.ModbusBaseAddress = modbusBaseAddress[i];
                            azurePowerGridModel.Monthly_electric_cost = Program.RandomNumberBetween(0, monthlyElectricCost);
                            azurePowerGridModel.Monthly_KWH_System = Program.RandomNumberBetween(0, monthlykWhSystem);
                            azurePowerGridModel.Rated_Amperage = Program.RandomNumberBetween(125, ratedAmperage);
                            azurePowerGridModel.ReadingTime = readingTime;
                            azurePowerGridModel.Rolling_hourly_kwh_system = Program.RandomNumberBetween(0, rollingHourlykWh);
                            azurePowerGridModel.Serial_number = powerScout[i];
                            azurePowerGridModel.Type = type;
                            azurePowerGridModel.Volts_L1_to_neutral = Program.RandomNumberBetween(voltsL1toNeutral_min, voltsL1toNeutral_max);
                            azurePowerGridModel.Volts_L2_to_neutral = Program.RandomNumberBetween(voltsL2toNeutral_min, voltsL2toNeutral_max);
                            azurePowerGridModel.Volts_L3_to_neutral = Program.RandomNumberBetween(voltsL3toNeutral_min, voltsL3toNeutral_max);
                            azurePowerGridModel.KW_L1 = Program.RandomNumberBetween(0, kWL1);
                            azurePowerGridModel.KW_L2 = Program.RandomNumberBetween(0, kWL2);
                            azurePowerGridModel.KW_L3 = Program.RandomNumberBetween(0, kWL3);
                            azurePowerGridModel.kW_System = Program.RandomNumberBetween(0, kWSystem);
                            azurePowerGridModel.PIIntTSTicks = Program.RandomNumberBetween(pIIntTSTicks_min, pIIntTSTicks_max);
                            azurePowerGridModel.PIIntShapeID = rnd.Next(1, 7);
                            UpdateDatabaseUsingCommand(azurePowerGridModel, sqlConnection, sqlTransaction);
                        }
                    }
                        sqlTransaction.Commit();
                    }

                    sqlConnection.Close();

                }

            }

            void UpdateDatabaseUsingCommand(AzurePowerGridModel model, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
            {
                using (SqlCommand cmd = new SqlCommand(powerGridInsertQuery, sqlConnection, sqlTransaction))
                {

                    cmd.Parameters.AddWithValue("@PowerScout", model.PowerScout);
                    cmd.Parameters.AddWithValue("@TimeStamp", model.Timestamp);
                    cmd.Parameters.AddWithValue("@AmpsL1", model.AMPS_L1);
                    cmd.Parameters.AddWithValue("@AmpsL2", model.AMPS_L2);
                    cmd.Parameters.AddWithValue("@AmpsL3", model.AMPS_L3);
                    cmd.Parameters.AddWithValue("@AmpsSystemAvg", model.AMPS_SYSTEM_AVG);
                    cmd.Parameters.AddWithValue("@BreakerDetails", model.Breaker_details);
                    cmd.Parameters.AddWithValue("@BreakerLabel", model.Breaker_label);
                    cmd.Parameters.AddWithValue("@Building", model.Building);
                    cmd.Parameters.AddWithValue("@DailyElectricCost", model.Daily_electric_cost);
                    cmd.Parameters.AddWithValue("@DailykWhSystem", model.Daily_KWH_System);
                    cmd.Parameters.AddWithValue("@ModbusBaseAddress", model.ModbusBaseAddress);
                    cmd.Parameters.AddWithValue("@MonthlyElectricCost", model.Monthly_electric_cost);
                    cmd.Parameters.AddWithValue("@MonthlykWhSystem", model.Monthly_KWH_System);
                    cmd.Parameters.AddWithValue("@RatedAmperage", model.Rated_Amperage);
                    cmd.Parameters.AddWithValue("@ReadingTime", model.ReadingTime);
                    cmd.Parameters.AddWithValue("@RollingHourlykWhSystem", model.Rolling_hourly_kwh_system);
                    cmd.Parameters.AddWithValue("@SerialNumber", model.Serial_number);
                    cmd.Parameters.AddWithValue("@Type", model.Type);
                    cmd.Parameters.AddWithValue("@VoltsL1toNeutral", model.Volts_L1_to_neutral);
                    cmd.Parameters.AddWithValue("@VoltsL2toNeutral", model.Volts_L2_to_neutral);
                    cmd.Parameters.AddWithValue("@VoltsL3toNeutral", model.Volts_L3_to_neutral);
                    cmd.Parameters.AddWithValue("@kWL1", model.KW_L1);
                    cmd.Parameters.AddWithValue("@kWL2", model.KW_L2);
                    cmd.Parameters.AddWithValue("@kWL3", model.KW_L3);
                    cmd.Parameters.AddWithValue("@kWSystem", model.kW_System);
                    cmd.Parameters.AddWithValue("@PIIntTSTicks", model.PIIntTSTicks);
                    cmd.Parameters.AddWithValue("@PIIntShapeID", model.PIIntShapeID);

                    cmd.ExecuteNonQuery();

                }
            }

            public HttpResponseMessage PostDatatoServer(string RequestURI, AzurePowerGridModel model)
            {

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(model);
                StringContent strContent = new StringContent(json);
                HttpResponseMessage response = client.PostAsync(RequestURI, strContent).Result;
                return response;
            }
        }
    }
