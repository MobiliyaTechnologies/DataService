﻿using System;
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
        //PiServer 7
        //public static string[] BuildingName = new string[] { "Maths Building", "Physics Building" };
        //static double[] BuildingMultiplier = new double[] { 0.7, 0.9 };
        //static string[] Type = new string[] { "PowerScout 6037", "PowerScout 5037" };
        //static List<List<string>> powerScouts = new List<List<string>> { new List<String> { "P371602068", "P371602070", "P371602072", "P371602073", "P371602075", "P371602077", "P371602079" }, new List<String> { "P371602048", "P371602050", "P371602052", "P371602053", "P371602055", "P371602057", "P371602059" } };
        //static List<List<string>> breakerDetails = new List<List<string>> { new List<String> { "New (2012) 4th floor panel - almost empty", "800A duct bank for panels B,C,D - 277v lighting", "M-Wing Addition MDP", "N-Wing Lecture halls", "208v Panel in room C2, Xfmr on roof above.", "Basement MCC", "2nd Floor Xfmr supplying panels L,M,N,U,U1,U2" }, new List<String> { "New (2015) 3rd floor panel - almost empty", "1000A duct bank for panels B,C,D - 300v lighting", "E-Wing Addition MDP", "G-Wing Lecture halls", "500v Panel in room C2, Xfmr on roof above.", "Physics Basement MCC", "5th Floor Xfmr supplying panels L,M,N,U,U1,U2" } };
        //static List<List<string>> breakerLabels = new List<List<string>> { new List<String> { "PP34 - 4th Fl Electrical Rm", "Main Lighting", "MCC", "F", "LCA", "MCC", "T-1" }, new List<String> { "PP51 - 4th F5 Electrical Rm", "Main Lighting Area", "MCC", "G", "LCA", "MCC", "T-1" } };

        //PiServer 8

        public static string[] BuildingName = new string[] { "Chemistry Building", "Science Building" }; static double[] BuildingMultiplier = new double[] { 0.5, 0.3 };
        static string[] Type = new string[] { "PowerScout 1037", "PowerScout 3037" };
        static List<List<string>> powerScouts = new List<List<string>> { new List<String> { "P371602028", "P371602030" }, new List<String> { "P371602018", "P371602020" } };
        static List<List<string>> breakerDetails = new List<List<string>> { new List<String> { "New (2018) 4th floor panel - almost empty", "Link tp P2,L3" }, new List<String> { "New (2013) 3rd floor panel - almost empty", "Lunk to P3, P4" } };
        static List<List<string>> breakerLabels = new List<List<string>> { new List<String> { "PP14 - 5th Fl Electrical Rm", "Second Floor" }, new List<String> { "PP31 - 3rd Fl Electrical Rm", "First Floor" } };



        public class Building
        {
            private static double multiplier = 0.8;
            public string type = "PowerScout 4037";
            public List<string> powerScout = new List<string>();
            public string building = "";
            public List<string> breakerDetail = new List<string>();
            public List<string> breakerLabel = new List<string>();
            public double ampsl1 = 433.619331204961 * multiplier;
            public double ampsl2 = 438.770010374815 * multiplier;
            public double ampsl3 = 436.675966480134 * multiplier;
            public double ampsSystemAvg = 436.675966480134 * multiplier;
            public double dailyElectricCost = 198.915319668451 * multiplier;
            public double dailykWhSystem = 4555.0468125888 * multiplier;
            public double monthlyElectricCost = 5827.75588031122 * multiplier;
            public double monthlykWhSystem = 133449.871605242 * multiplier;
            public double ratedAmperage = 200 * multiplier;
            public DateTime readingTime = DateTime.Now;
            public double rollingHourlykWh = 208.126197596686 * multiplier;
            public double voltsL1toNeutral_min = 238.449543151905 * multiplier;
            public double voltsL2toNeutral_min = 237.714431773496 * multiplier;
            public double voltsL3toNeutral_min = 238.806858504314 * multiplier;
            public double voltsL1toNeutral_max = 286.766991385748 * multiplier;
            public double voltsL2toNeutral_max = 285.197925792541 * multiplier;
            public double voltsL3toNeutral_max = 286.596888688812 * multiplier;
            public double kWL1 = 75.6108646752933 * multiplier;
            public double kWL2 = 75.6830692949571 * multiplier;
            public double kWL3 = 75.1904180271698 * multiplier;
            public double kWSystem = 226.575086870641 * multiplier;
            public double pIIntTSTicks_min = 636089806874751616 * (multiplier * 2);
            public double pIIntTSTicks_max = 636289444874751616 * (multiplier * 2);
            public string powerGridInsertQuery = "INSERT INTO powergridview([Id],[PowerScout],[TimeStamp],[Amps L1],[Amps L2],[Amps L3],[Amps System Avg],[Breaker Details],[Breaker Label],[Building],[Daily Electric Cost],[Daily kWh System],[Modbus Base Address],[Monthly Electric Cost],[Monthly kWh System],[Rated Amperage],[ReadingTime],[Rolling Hourly kWh System],[Serial Number],[Type],[Volts L1 to Neutral],[Volts L2 to Neutral],[Volts L3 to Neutral],[kW L1],[kW L2],[kW L3],[kW System],[PIIntTSTicks],[PIIntShapeID]) VALUES (@Id,@PowerScout,@TimeStamp,@AmpsL1,@AmpsL2,@AmpsL3,@AmpsSystemAvg,@BreakerDetails,@BreakerLabel,@Building,@DailyElectricCost,@DailykWhSystem,@ModbusBaseAddress,@MonthlyElectricCost,@MonthlykWhSystem,@RatedAmperage,@ReadingTime,@RollingHourlykWhSystem,@SerialNumber,@Type,@VoltsL1toNeutral,@VoltsL2toNeutral,@VoltsL3toNeutral,@kWL1,@kWL2,@kWL3,@kWSystem,@PIIntTSTicks,@PIIntShapeID)";


            public Building(string buildingName)
            {
                int mapperIndex = Array.IndexOf(BuildingName, buildingName);
                powerScout = powerScouts.ElementAt(mapperIndex);
                multiplier = BuildingMultiplier.ElementAt(mapperIndex);
                breakerDetail = breakerDetails.ElementAt(mapperIndex);
                breakerLabel = breakerLabels.ElementAt(mapperIndex);
                type = Type[mapperIndex];
                ampsl1 = 433.619331204961 * multiplier;
                ampsl2 = 438.770010374815 * multiplier;
                ampsl3 = 436.675966480134 * multiplier;
                ampsSystemAvg = 436.675966480134 * multiplier;
                dailyElectricCost = 198.915319668451 * multiplier;
                dailykWhSystem = 4555.0468125888 * multiplier;
                monthlyElectricCost = 5827.75588031122 * multiplier;
                monthlykWhSystem = 133449.871605242 * multiplier;
                ratedAmperage = 200 * multiplier;
                readingTime = DateTime.Now;
                rollingHourlykWh = 208.126197596686 * multiplier;
                building = buildingName;
                voltsL1toNeutral_min = 238.449543151905 * multiplier;
                voltsL2toNeutral_min = 237.714431773496 * multiplier;
                voltsL3toNeutral_min = 238.806858504314 * multiplier;
                voltsL1toNeutral_max = 286.766991385748 * multiplier;
                voltsL2toNeutral_max = 285.197925792541 * multiplier;
                voltsL3toNeutral_max = 286.596888688812 * multiplier;
                kWL1 = 75.6108646752933 * multiplier;
                kWL2 = 75.6830692949571 * multiplier;
                kWL3 = 75.1904180271698 * multiplier;
                kWSystem = 226.575086870641 * multiplier;
                pIIntTSTicks_min = 636089806874751616 * (multiplier * 2);
                pIIntTSTicks_max = 636289444874751616 * (multiplier * 2);
                powerGridInsertQuery = "INSERT INTO powergridview([Id],[PowerScout],[TimeStamp],[Amps L1],[Amps L2],[Amps L3],[Amps System Avg],[Breaker Details],[Breaker Label],[Building],[Daily Electric Cost],[Daily kWh System],[Modbus Base Address],[Monthly Electric Cost],[Monthly kWh System],[Rated Amperage],[ReadingTime],[Rolling Hourly kWh System],[Serial Number],[Type],[Volts L1 to Neutral],[Volts L2 to Neutral],[Volts L3 to Neutral],[kW L1],[kW L2],[kW L3],[kW System],[PIIntTSTicks],[PIIntShapeID]) VALUES (@Id,@PowerScout,@TimeStamp,@AmpsL1,@AmpsL2,@AmpsL3,@AmpsSystemAvg,@BreakerDetails,@BreakerLabel,@Building,@DailyElectricCost,@DailykWhSystem,@ModbusBaseAddress,@MonthlyElectricCost,@MonthlykWhSystem,@RatedAmperage,@ReadingTime,@RollingHourlykWhSystem,@SerialNumber,@Type,@VoltsL1toNeutral,@VoltsL2toNeutral,@VoltsL3toNeutral,@kWL1,@kWL2,@kWL3,@kWSystem,@PIIntTSTicks,@PIIntShapeID)";

            }


        }


        public void AddPowerGridView(Building building, DateTime timeStamp)
        {
            rnd = new Random();

            int[] modbusBaseAddress = new int[] { 2, 5, 6, 4, 3, 8, 7 };


            int number = rnd.Next(0, building.powerScout.Count - 1);
            AzurePowerGridModel azurePowerGridModel = new AzurePowerGridModel();
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationSetting.ConnectionString))
                {
                    sqlConnection.Open();
                    using (SqlTransaction sqlTransaction = sqlConnection.BeginTransaction())
                    {
                       // DateTime currentTime = DateTime.UtcNow;
                       // for (int j = 0; j <= 43200; j++)
                        {
                          //  Console.WriteLine("Processing " + j + " entries "+ " for "+ building.building);
                           
                            for (int i = 0; i < building.powerScout.Count; i++)
                            {
                                Program.Id = Program.Id+1;
                                azurePowerGridModel = new AzurePowerGridModel();
                                azurePowerGridModel.Id = Program.Id;
                                azurePowerGridModel.PowerScout = building.powerScout.ElementAt(i);
                                azurePowerGridModel.Timestamp = timeStamp;
                                azurePowerGridModel.AMPS_L1 = Program.RandomNumberBetween(0, building.ampsl1);
                                azurePowerGridModel.AMPS_L2 = Program.RandomNumberBetween(0, building.ampsl2);
                                azurePowerGridModel.AMPS_L3 = Program.RandomNumberBetween(0, building.ampsl3);
                                azurePowerGridModel.AMPS_SYSTEM_AVG = Program.RandomNumberBetween(0, building.ampsSystemAvg);
                                azurePowerGridModel.Breaker_details = building.breakerDetail.ElementAt(i);
                                azurePowerGridModel.Breaker_label = building.breakerLabel.ElementAt(i);
                                azurePowerGridModel.Building = building.building;
                                azurePowerGridModel.Daily_electric_cost = Program.RandomNumberBetween(0, building.dailyElectricCost);
                                azurePowerGridModel.Daily_KWH_System = Program.RandomNumberBetween(0, building.dailykWhSystem);
                                azurePowerGridModel.ModbusBaseAddress = modbusBaseAddress[i];
                                azurePowerGridModel.Monthly_electric_cost = Program.RandomNumberBetween(0, building.monthlyElectricCost);
                                azurePowerGridModel.Monthly_KWH_System = Program.RandomNumberBetween(0, building.monthlykWhSystem);
                                azurePowerGridModel.Rated_Amperage = Program.RandomNumberBetween(125, building.ratedAmperage);
                                azurePowerGridModel.ReadingTime = building.readingTime;
                                azurePowerGridModel.Rolling_hourly_kwh_system = Program.RandomNumberBetween(0, building.rollingHourlykWh);
                                azurePowerGridModel.Serial_number = building.powerScout.ElementAt(i);
                                azurePowerGridModel.Type = building.type;
                                azurePowerGridModel.Volts_L1_to_neutral = Program.RandomNumberBetween(building.voltsL1toNeutral_min, building.voltsL1toNeutral_max);
                                azurePowerGridModel.Volts_L2_to_neutral = Program.RandomNumberBetween(building.voltsL2toNeutral_min, building.voltsL2toNeutral_max);
                                azurePowerGridModel.Volts_L3_to_neutral = Program.RandomNumberBetween(building.voltsL3toNeutral_min, building.voltsL3toNeutral_max);
                                azurePowerGridModel.KW_L1 = Program.RandomNumberBetween(0, building.kWL1);
                                azurePowerGridModel.KW_L2 = Program.RandomNumberBetween(0, building.kWL2);
                                azurePowerGridModel.KW_L3 = Program.RandomNumberBetween(0, building.kWL3);
                                azurePowerGridModel.kW_System = Program.RandomNumberBetween(0, building.kWSystem);
                                azurePowerGridModel.PIIntTSTicks = Program.RandomNumberBetween(building.pIIntTSTicks_min, building.pIIntTSTicks_max);
                                azurePowerGridModel.PIIntShapeID = rnd.Next(1, 7);
                                UpdateDatabaseUsingCommand(building.powerGridInsertQuery, azurePowerGridModel, sqlConnection, sqlTransaction);
                            }
                        }
                        sqlTransaction.Commit();

                    }

                    sqlConnection.Close();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured in AddPowerGridView " + ex.Message);
            }

        }

        void UpdateDatabaseUsingCommand(string powerGridInsertQuery, AzurePowerGridModel model, SqlConnection sqlConnection, SqlTransaction sqlTransaction)
        {

            using (SqlCommand cmd = new SqlCommand(powerGridInsertQuery, sqlConnection, sqlTransaction))
            {
                cmd.Parameters.AddWithValue("@Id", model.Id);
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
