using DataModels;
using DataProcessor.DataModels;
using DataProcessor.Managers;
using DataProcessor.Processor;
using DataProcessor.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DataProcessor
{
    /// <summary>
    /// This class is written for the process of getting half an hour of data from OSI Soft's PI server and 
    /// process it here using weather and class schedule information and update processed 
    /// data to Azure SQL server.
    /// </summary>
    public class DataProcessor
    {

        #region EVENTS AND CALLBACKS
        #endregion

        #region PRIVATE_VARIABLES
        System.Timers.Timer processTimer;
        static double utcConversionTime = 0;



       
        public DataProcessor()
        {   
        }
        #endregion

        #region PUBLIC METHODS
        public void ProcessData()
        {
            try
            {

                Utility.Log("PI server : " + ConfigurationSettings.PiServer);
                Utility.Log("Azure ConnectionString : " + ConfigurationSettings.AzureConnectionString);
                Utility.Log("Pi Server ConnectionString : " + ConfigurationSettings.PiServerConnectionString);

                if (!((string.IsNullOrEmpty(ConfigurationSettings.PiServer)) || (string.IsNullOrEmpty(ConfigurationSettings.AzureConnectionString)) || (string.IsNullOrEmpty(ConfigurationSettings.PiServerConnectionString))
              ))
                {


                    Console.WriteLine("Init ProcessData");
                    Utility.Log("Init ProcessData");
                    //if (utcConversionTime != GetAndTimezone())
                    //{
                    utcConversionTime = GetAndTimezone();
                    PIConfigurationInfoModel piConfig = new PIConfigurationInfoModel();
                    string serverName = ConfigurationSettings.PiServer;
                    piConfig.PiServerDesc = serverName;
                    piConfig.PiServerName = serverName;
                    piConfig.PiServerURL = ConfigurationSettings.PiServerConnectionString;
                    piConfig.UTCConversionTime = utcConversionTime;
                    piConfig.CreatedOn = DateTime.UtcNow;
                    ConnectionManager.Instance().InserPIConfigurationDetailsToDB(piConfig);
                    //}
                    ConnectionManager.Instance().Initialize();
                    Console.WriteLine("Done with Console manager initialization");


                    string storageConnectionString = ConfigurationSettings.GetCloudConfigDataModel().BlobStorageURL;
                    //patch for now

                    storageConnectionString = ConfigurationSettings.StorageConnectionString;

                    //
                    if (!string.IsNullOrEmpty(storageConnectionString))
                    {
                        BlobStorageManager.Instance().ConfigureBlobStorage(storageConnectionString);
                        Console.WriteLine("Done with Blob Storage configuration\n");
                        Utility.Log("Done with Console manager initialization");

                        string piServer = ConnectionManager.Instance().GetPIServer();

                        Thread piThread = new Thread(() => { ProcessDataByPIServer(piServer); });
                        piThread.Start();
                        Thread sensorThread = new Thread(() => { InsertSensorData(piServer); });
                        sensorThread.Start();
                    }
                    else
                    {
                        Utility.Log("Does not have storage connection string ");
                        Console.WriteLine("Does not have storage connection string");
                    }
                }
                else
                {
                    Console.WriteLine("Doesn't have sufficient data in app config for connection");
                    Console.ReadLine();
                    Utility.Log("Doesn't have sufficient data in app config for connection");
                }
            }
            catch (Exception e)
            {

                Console.WriteLine("Error occured in processData" + e.Message);
                Utility.Log("Error occured in processData" + e.Message);
                Utility.LogException(e);
            }
        }

        #endregion

        #region PRIVATE METHODS
        void ProcessDataByPIServer(string piServerName)
        {

            Utility.Log("Process started" + piServerName + " " + DateTime.Now.ToString());
            while (true)
            {
                try
                {
                    //if (utcConversionTime != GetAndTimezone())
                    //{
                    utcConversionTime = GetAndTimezone();
                    ConnectionManager.Instance().UpdateUTCTimeDifference(piServerName, utcConversionTime);
                    //}


                    //Class Schedule File initialization
                    bool isClassScheduleFileExist = ClassScheduleManager.Instance().ReInitialize(piServerName);
                    ////


                    double sleepTimeInMins = 30;
                    //To Do get connection basis of PI server using Connection Manager
                    SqlConnection piConnection = ConnectionManager.Instance().GetPISQLConnection(piServerName);
                    ConnectionManager.Instance().OpenSQLConnection(piConnection);
                    Console.WriteLine("Pi SQL Connection Opened");
                    //Utility.Log("Pi SQL Connection Opened");

                    SqlConnection weatherConnection = ConnectionManager.Instance().GetPISQLConnection(piServerName);
                    ConnectionManager.Instance().OpenSQLConnection(weatherConnection);


                    //We need this meterlist, bcoz we going to process data meter by meter
                    List<string> meterList = null;
                    var meterDetails = getMeterDetails(piServerName);
                    //Need to call this again and again, if any new meter gets added in PI Server.
                    var meterAndBreakerDetailsListFromPI = getMeterAndBreakerDetailsList(piServerName);
                    if (meterAndBreakerDetailsListFromPI != null && meterAndBreakerDetailsListFromPI.Count != 0)
                    {

                        meterList = new List<string>();
                        if (meterDetails != null && meterDetails.Count != 0)
                        {
                            var metersToInsert = meterAndBreakerDetailsListFromPI.Where(p => !meterDetails.Any(p2 => p2.PowerScout == p.PowerScout)).ToList();
                            if(metersToInsert!=null && metersToInsert.Count != 0 )
                            {
                                InsertMetersIntoAzure(metersToInsert, utcConversionTime);
                            }
                        }
                        else
                        {
                            InsertMetersIntoAzure(meterAndBreakerDetailsListFromPI, utcConversionTime);
                        }
                        meterList = meterAndBreakerDetailsListFromPI.Select(x => x.PowerScout).ToList();
                    }


                    ProcessedDataModel processedDataInfo = BlobStorageManager.Instance().GetLastProcessedData<ProcessedDataModel>(piServerName, Constants.THRESHOLD_METER_STORAGE_FILENAME_PREFIX);
                    if (processedDataInfo == null)
                        processedDataInfo = new ProcessedDataModel { MeterTimestamp = new Dictionary<string, DateTime>() };
                    Dictionary<string, DateTime> meterTimestamp = processedDataInfo.MeterTimestamp;
                    //Todo need to add validation here at timestamp whether it is null or not, if it is null then have to add default value

                    Utility.Log("Meter Count : " + meterList.Count);

                    meterList.All(meter =>
                    {
                        if (!meterTimestamp.ContainsKey(meter))
                        {
                            SqlConnection meterFirstEntryConn = ConnectionManager.Instance().GetPISQLConnection(piServerName);
                            ConnectionManager.Instance().OpenSQLConnection(meterFirstEntryConn);
                            SqlCommand getTimestamp = new SqlCommand("SELECT TOP 1 Timestamp FROM PowergridView WHERE PowerScout = @meter order by Timestamp", meterFirstEntryConn);
                            getTimestamp.Parameters.Add(new SqlParameter("meter", meter));
                            SqlDataReader result = getTimestamp.ExecuteReader();
                            while (result.Read()) //Runs only once
                            {
                                DateTime updatedTime = Utility.TrimDateToMinute(((DateTime)result[0]).AddMinutes(-1));
                                meterTimestamp.Add(meter, Convert.ToDateTime(updatedTime));
                            }
                            ConnectionManager.Instance().CloseSQLConnection(meterFirstEntryConn);
                        }

                        //This is bcoz we are saving value in threshold file with addition of utcconversion time in pidb time.So this condition should be basedon pi time
                        DateTime startTime = Utility.TrimDateToMinute(meterTimestamp[meter]);
                        DateTime endTime = startTime.AddMinutes(Constants.TIME_WINDOW_FOR_HALF_HOURLY_DATA);
                        SqlCommand command;
                        //I know this code is wrong have to covert into timestamp string or add a certain value to timestamp to make proper timestamp
                        command = new SqlCommand("SELECT * FROM PowergridView WHERE PowerScout = @meter AND Timestamp > @startTime AND Timestamp <= @endTime ORDER BY Timestamp", piConnection);

                        // Add the parameters.
                        command.Parameters.Add(new SqlParameter("@startTime", startTime.ToString(Constants.DATE_TIME_FORMAT)));
                        command.Parameters.Add(new SqlParameter("@endTime", endTime.ToString(Constants.DATE_TIME_FORMAT)));
                        command.Parameters.Add(new SqlParameter("@meter", meter));

                        SqlDataReader pireader = command.ExecuteReader();
                        List<AzureDataModel> meterDataList = new List<AzureDataModel>();
                        DateTime lastProcessedDate = DateTime.Now;
                        while (pireader.Read())
                        {
                            AzureDataModel data = new AzureDataModel();
                            string serialNumber = Convert.ToString(pireader["Serial Number"]);
                            DateTime utcDate;
                            DateTime.TryParse(pireader["Timestamp"].ToString(), out utcDate);

                            WeatherDetails weatherDetails = GetWeatherDetails(utcDate, weatherConnection);
                            lastProcessedDate = utcDate;
                            utcDate = utcDate.AddHours(utcConversionTime);
                            var utcSQLFormattedDate = utcDate.ToString(Constants.DATE_TIME_FORMAT);




                            if (pireader["Id"] != DBNull.Value)
                                data.Id = Convert.ToInt32(pireader["Id"]);
                            if (pireader["Amps L1"] != DBNull.Value)
                                data.AMPS_L1 = Convert.ToDouble(pireader["Amps L1"]);
                            if (pireader["Amps L2"] != DBNull.Value)
                                data.AMPS_L2 = Convert.ToDouble(pireader["Amps L2"]);
                            if (pireader["Amps L3"] != DBNull.Value)
                                data.AMPS_L3 = Convert.ToDouble(pireader["Amps L3"]);
                            if (pireader["Amps System Avg"] != DBNull.Value)
                                data.AMPS_SYSTEM_AVG = Convert.ToDouble(pireader["Amps System Avg"]);
                            if (pireader["Breaker Details"] != DBNull.Value)
                                data.Breaker_details = Convert.ToString(pireader["Breaker Details"]);
                            if (pireader["Breaker Label"] != DBNull.Value)
                                data.Breaker_label = Convert.ToString(pireader["Breaker Label"]);
                            if (pireader["Building"] != DBNull.Value)
                                data.Building = Convert.ToString(pireader["Building"]);

                            if (isClassScheduleFileExist)
                            {
                                ClassOccupanyDetails classDetails = ClassScheduleManager.Instance().ProcessDataRow(serialNumber, utcDate);
                                data.ClassOccupanyRemaining = classDetails.ClassOccupanyRemaining;

                                data.ClassOccupiedValue = classDetails.ClassOccupiedValue;

                                data.TotalClassCapacity = classDetails.ClassTotalCapacity;

                                data.isClassOccupied = classDetails.IsClassOccupied;
                            }

                            if (pireader["Daily Electric Cost"] != DBNull.Value)
                                data.Daily_electric_cost = Convert.ToDouble(pireader["Daily Electric Cost"]);
                            if (pireader["Daily kWh System"] != DBNull.Value)
                                data.Daily_KWH_System = Convert.ToDouble(pireader["Daily kWh System"]);
                            if (pireader["kW L1"] != DBNull.Value)
                                data.KW_L1 = Convert.ToDouble(pireader["kW L1"]);
                            if (pireader["kW L2"] != DBNull.Value)
                                data.KW_L2 = Convert.ToDouble(pireader["kW L2"]);
                            if (pireader["kW L3"] != DBNull.Value)
                                data.KW_L3 = Convert.ToDouble(pireader["kW L3"]);
                            if (pireader["Monthly Electric Cost"] != DBNull.Value)
                                data.Monthly_electric_cost = Convert.ToDouble(pireader["Monthly Electric Cost"]);
                            if (pireader["Monthly kWh System"] != DBNull.Value)
                                data.Monthly_KWH_System = Convert.ToDouble(pireader["Monthly kWh System"]);
                            if (pireader["PowerScout"] != DBNull.Value)
                                data.PowerScout = Convert.ToString(pireader["PowerScout"]);
                            if (pireader["Rated Amperage"] != DBNull.Value)
                                data.Rated_Amperage = Convert.ToDouble(pireader["Rated Amperage"]);



                            if (pireader["Rolling Hourly kWh System"] != DBNull.Value)
                                data.Rolling_hourly_kwh_system = Convert.ToDouble(pireader["Rolling Hourly kWh System"]);

                            if (pireader["Serial Number"] != DBNull.Value)
                                data.Serial_number = Convert.ToString(pireader["Serial Number"]);

                            if (weatherDetails != null)
                            {
                                data.Pressure = Convert.ToDouble(weatherDetails.Pressure);
                                data.Relative_humidity = Convert.ToDouble(weatherDetails.RelativeHumidity);
                                data.Temperature = Convert.ToDouble(weatherDetails.Temperature);
                                data.Visibility = Convert.ToDouble(weatherDetails.Visibility);
                            }
                            else
                            {
                                data.Pressure = 0;
                                data.Relative_humidity = 0;
                                data.Temperature = 0;
                                data.Visibility = 0;
                            }
                            //Check here
                            data.Timestamp = Convert.ToDateTime(utcSQLFormattedDate);

                            if (pireader["Type"] != DBNull.Value)
                                data.Type = Convert.ToString(pireader["Type"]);


                            if (pireader["Volts L1 to Neutral"] != DBNull.Value)
                                data.Volts_L1_to_neutral = Convert.ToDouble(pireader["Volts L1 to Neutral"]);
                            if (pireader["Volts L2 to Neutral"] != DBNull.Value)
                                data.Volts_L2_to_neutral = Convert.ToDouble(pireader["Volts L2 to Neutral"]);
                            if (pireader["Volts L3 to Neutral"] != DBNull.Value)
                                data.Volts_L3_to_neutral = Convert.ToDouble(pireader["Volts L3 to Neutral"]);
                            if (pireader["kW System"] != DBNull.Value)
                                data.kW_System = Convert.ToDouble(pireader["kW System"]);

                            data.PiServerName = piServerName;
                            meterDataList.Add(data);
                            meterTimestamp[meter] = lastProcessedDate;
                            //count++;
                        }
                        pireader.Close();
                        //Hack Hack Hack
                        if (meterDataList != null && meterDataList.Count != 0)
                        {
                           // Utility.Log("ProcessDataByPIServer() , MeterDataList Count == "+ meterDataList.Count);
                            //    This condition means we get all(29)entries of that perticular half hour
                            if (Utility.TrimDateToMinute(lastProcessedDate) == endTime.AddMinutes(-1))
                            {
                                Console.WriteLine("Now going to update Database");
                                Utility.Log("Now going to update Database");
                                sleepTimeInMins = 0;
                                updateDatabase(meterDataList);
                                processedDataInfo.MeterTimestamp = meterTimestamp;
                                Console.Write("Storing value to Blob : " + processedDataInfo);
                                Utility.Log("Storing value to Blob : " + processedDataInfo);
                                BlobStorageManager.Instance().SetLastProcessedData<ProcessedDataModel>(piServerName, Constants.THRESHOLD_METER_STORAGE_FILENAME_PREFIX, processedDataInfo);
                            }
                            else
                            {
                                if (sleepTimeInMins != 0)
                                {
                                    if (sleepTimeInMins > (endTime - Utility.TrimDateToMinute(lastProcessedDate)).Minutes)
                                    {
                                        sleepTimeInMins = (endTime - Utility.TrimDateToMinute(lastProcessedDate)).Minutes;
                                    }
                                }
                            }
                        }

                        return true;
                    });


                    ConnectionManager.Instance().CloseSQLConnection(piConnection);
                    ConnectionManager.Instance().CloseSQLConnection(weatherConnection);
                    Utility.Log("**************Sleeping for " + (Convert.ToInt32(sleepTimeInMins) * 60 * 1000) + " Millis**************");
                    //Convert Minutes to millis
                    Thread.Sleep(Convert.ToInt32(sleepTimeInMins) * 60 * 1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine("*********Exception Occured Message******" + e.Message);
                    Console.WriteLine("*********Exception Occured StackTrace******" + e.StackTrace);
                    Utility.Log("*********Exception Occured In Main Loop******" + e.Message);
                    Utility.LogException(e);
                }
            }
        }

        public void updateDatabase(List<AzureDataModel> dataList)
        {

            try
            {
                if (dataList == null || dataList.Count < 1)
                    return;

                HalfHourlyDataModel averageData = new HalfHourlyDataModel();
                averageData.AMPS_L1 = dataList.Average(data => data.AMPS_L1);
                averageData.AMPS_L2 = dataList.Average(data => data.AMPS_L2);
                averageData.AMPS_L3 = dataList.Average(data => data.AMPS_L3);
                averageData.AMPS_SYSTEM_AVG = dataList.Average(data => data.AMPS_SYSTEM_AVG);
                averageData.Breaker_details = dataList[0].Breaker_details;
                averageData.Breaker_label = dataList[0].Breaker_label;
                averageData.Building = dataList[0].Building;
                //If class schedule file doesn't exist, below parameters set to a default value 0
                averageData.ClassOccupanyRemaining = (!dataList.Any(x => x.ClassOccupanyRemaining == null)) ? ((int)dataList.Average(data => data.ClassOccupanyRemaining)) : 0;
                averageData.ClassOccupiedValue = (!dataList.Any(x => x.ClassOccupiedValue == null)) ? ((int)dataList.Average(data => data.ClassOccupiedValue)) : 0;
                averageData.TotalClassCapacity = (!dataList.Any(x => x.TotalClassCapacity == null)) ? ((int)dataList.Average(data => data.TotalClassCapacity)) : 0;
                averageData.isClassOccupied = (!dataList.Any(x => x.isClassOccupied == null)) ? ((int)dataList.Average(data => data.isClassOccupied)) : 0;
                //
                averageData.Daily_electric_cost = dataList.Max(data => data.Daily_electric_cost);
                averageData.Daily_KWH_System = dataList.Max(data => data.Daily_KWH_System);
                averageData.KW_L1 = dataList.Average(data => data.KW_L1);
                averageData.KW_L2 = dataList.Average(data => data.KW_L2);
                averageData.KW_L3 = dataList.Average(data => data.KW_L3);
                averageData.Monthly_electric_cost = dataList.Average(data => data.Monthly_electric_cost);
                averageData.Monthly_KWH_System = dataList.Average(data => data.Monthly_KWH_System);
                averageData.PowerScout = dataList[0].PowerScout;
                averageData.Rated_Amperage = dataList.Average(data => data.Rated_Amperage);
                averageData.Pressure = dataList.Average(data => data.Pressure);
                averageData.Relative_humidity = dataList.Average(data => data.Relative_humidity);
                averageData.Rolling_hourly_kwh_system = dataList.Average(data => data.Rolling_hourly_kwh_system);
                averageData.Serial_number = dataList[0].Serial_number;
                averageData.Temperature = dataList.Average(data => data.Temperature);
                averageData.Timestamp = dataList[dataList.Count - 1].Timestamp;
                averageData.Type = dataList[0].Type;
                averageData.Visibility = dataList.Average(data => data.Visibility);
                averageData.Volts_L1_to_neutral = dataList.Average(data => data.Volts_L1_to_neutral);
                averageData.Volts_L2_to_neutral = dataList.Average(data => data.Volts_L2_to_neutral);
                averageData.Volts_L3_to_neutral = dataList.Average(data => data.Volts_L3_to_neutral);
                averageData.kW_System = dataList.Average(data => data.kW_System);
                averageData.days = GetDayFromTimestamp((DateTime)dataList[dataList.Count - 1].Timestamp);
                averageData.time_period = GetTimePriodFromTimestamp((DateTime)dataList[dataList.Count - 1].Timestamp);
                averageData.PiServerName = dataList.ElementAt(0).PiServerName;


                string query = "INSERT INTO LiveData VALUES (@AMPS_L1,@AMPS_L2,@AMPS_L3,@AMPS_SYSTEM_AVG,@Breaker_details,@Breaker_label,@Building,@ClassOccupanyRemaining,@ClassOccupiedValue,@TotalClassCapacity,@Daily_electric_cost,@Daily_KWH_System,@isClassOccupied,@KW_L1,@KW_L2,@KW_L3,@Monthly_electric_cost,@Monthly_KWH_System,@PowerScout,@Rated_Amperage,@Pressure,@Relative_humidity,@Rolling_hourly_kwh_system,@Serial_number,@Temperature,@Timestamp,@Type,@Visibility,@Volts_L1_to_neutral,@Volts_L2_to_neutral,@Volts_L3_to_neutral,@kW_System,@days,@time_period,@PiServerName)";
                SqlConnection azureConnection = new SqlConnection(Utils.ConfigurationSettings.AzureConnectionString);
                azureConnection.Open();
                SqlCommand cmd = new SqlCommand(query, azureConnection);
                cmd.Parameters.Add(new SqlParameter("AMPS_L1", averageData.AMPS_L1));
                cmd.Parameters.Add(new SqlParameter("AMPS_L2", averageData.AMPS_L2));
                cmd.Parameters.Add(new SqlParameter("AMPS_L3", averageData.AMPS_L3));
                cmd.Parameters.Add(new SqlParameter("AMPS_SYSTEM_AVG", averageData.AMPS_SYSTEM_AVG));
                cmd.Parameters.Add(new SqlParameter("Breaker_details", averageData.Breaker_details));
                cmd.Parameters.Add(new SqlParameter("Breaker_label", averageData.Breaker_label));
                cmd.Parameters.Add(new SqlParameter("Building", averageData.Building));
                cmd.Parameters.Add(new SqlParameter("ClassOccupanyRemaining", averageData.ClassOccupanyRemaining));
                cmd.Parameters.Add(new SqlParameter("ClassOccupiedValue", averageData.ClassOccupiedValue));
                cmd.Parameters.Add(new SqlParameter("TotalClassCapacity", averageData.TotalClassCapacity));
                cmd.Parameters.Add(new SqlParameter("Daily_electric_cost", averageData.Daily_electric_cost));
                cmd.Parameters.Add(new SqlParameter("Daily_KWH_System", averageData.Daily_KWH_System));
                cmd.Parameters.Add(new SqlParameter("isClassOccupied", averageData.isClassOccupied));
                cmd.Parameters.Add(new SqlParameter("KW_L1", averageData.KW_L1));
                cmd.Parameters.Add(new SqlParameter("KW_L2", averageData.KW_L2));
                cmd.Parameters.Add(new SqlParameter("KW_L3", averageData.KW_L3));
                cmd.Parameters.Add(new SqlParameter("Monthly_electric_cost", averageData.Monthly_electric_cost));
                cmd.Parameters.Add(new SqlParameter("Monthly_KWH_System", averageData.Monthly_KWH_System));
                cmd.Parameters.Add(new SqlParameter("PowerScout", averageData.PowerScout));
                cmd.Parameters.Add(new SqlParameter("Rated_Amperage", averageData.Rated_Amperage));
                cmd.Parameters.Add(new SqlParameter("Pressure", averageData.Pressure));
                cmd.Parameters.Add(new SqlParameter("Relative_humidity", averageData.Relative_humidity));
                cmd.Parameters.Add(new SqlParameter("Rolling_hourly_kwh_system", averageData.Rolling_hourly_kwh_system));
                cmd.Parameters.Add(new SqlParameter("Serial_number", averageData.Serial_number));
                cmd.Parameters.Add(new SqlParameter("Temperature", averageData.Temperature));
                cmd.Parameters.Add(new SqlParameter("Timestamp", ((DateTime)averageData.Timestamp).ToString(Constants.DATE_TIME_FORMAT)));
                cmd.Parameters.Add(new SqlParameter("Type", averageData.Type));
                cmd.Parameters.Add(new SqlParameter("Visibility", averageData.Visibility));
                cmd.Parameters.Add(new SqlParameter("Volts_L1_to_neutral", averageData.Volts_L1_to_neutral));
                cmd.Parameters.Add(new SqlParameter("Volts_L2_to_neutral", averageData.Volts_L2_to_neutral));
                cmd.Parameters.Add(new SqlParameter("Volts_L3_to_neutral", averageData.Volts_L3_to_neutral));
                cmd.Parameters.Add(new SqlParameter("kW_System", averageData.kW_System));
                cmd.Parameters.Add(new SqlParameter("days", averageData.days));
                cmd.Parameters.Add(new SqlParameter("time_period", averageData.time_period));
                cmd.Parameters.Add(new SqlParameter("PiServerName", averageData.PiServerName));
                cmd.ExecuteNonQuery();
                azureConnection.Close();



                //To Do update this data to queue



                Console.WriteLine(averageData.PowerScout + " => " + ((DateTime)averageData.Timestamp).ToString(Constants.DATE_TIME_FORMAT));


            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured in update database"+ ex.Message);
                Utility.Log("Exception occured in Update Database method" + ex.Message);
                Utility.LogException(ex);
            }
        }



        WeatherDetails GetWeatherDetails(DateTime utcDate, SqlConnection weatherConnection)
        {

            var startTime = utcDate.AddMinutes(-5);
            var endTime = utcDate.AddMinutes(5);


            var startSQLFormattedDate = startTime.ToString(Constants.DATE_TIME_FORMAT);
            var endSQLFormattedDate = endTime.ToString(Constants.DATE_TIME_FORMAT);

            SqlCommand weatherCmd = new SqlCommand("SELECT * FROM Weather where TimeStamp >= @startTime AND TimeStamp <= @endTime", weatherConnection);

            string query = "SELECT * FROM Weather where TimeStamp >= " + startSQLFormattedDate + " AND TimeStamp <= " + endSQLFormattedDate;

            //log.Debug(query);
            // Console.WriteLine(query);
            weatherCmd.Parameters.Add("@startTime", SqlDbType.DateTime);
            weatherCmd.Parameters["@startTime"].Value = startSQLFormattedDate;
            weatherCmd.Parameters.Add("@endTime", SqlDbType.DateTime);
            weatherCmd.Parameters["@endTime"].Value = endSQLFormattedDate;
            SqlDataReader weatherReader = null;
            try
            {
                weatherReader = weatherCmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Weather table does not exists.");
                Utility.Log("Weather table does not exists.");
                Utility.LogException(ex);
            }


            WeatherDetails weatherDetails = new WeatherDetails();

            if (weatherReader != null)
            {
                while (weatherReader.Read())
                {
                    if (weatherReader["Temperature"] != DBNull.Value)
                        weatherDetails.Temperature = Convert.ToInt32(weatherReader["Temperature"]);
                    if (weatherReader["Pressure"] != DBNull.Value)
                        weatherDetails.Pressure = Convert.ToInt32(weatherReader["Pressure"]);
                    if (weatherReader["Relative Humidity"] != DBNull.Value)
                        weatherDetails.RelativeHumidity = Convert.ToInt32(weatherReader["Relative Humidity"]);
                    if (weatherReader["visibility"] != DBNull.Value)
                        weatherDetails.Visibility = Convert.ToInt32(weatherReader["visibility"]);

                    if (weatherDetails.Temperature == 0)
                    {
                        continue;
                    }

                    break;
                }
                weatherReader.Close();
                return weatherDetails;
            }
            else
            {
                return null;
            }
        }

        string GetDayFromTimestamp(DateTime timeStamp)
        {
            return (timeStamp).ToString("dddd");
        }

        string GetTimePriodFromTimestamp(DateTime timeStamp)
        {
            string timeperiod = string.Empty;
            if (timeStamp.Hour >= 0 && timeStamp.Hour <= 3)
            {
                timeperiod = "Mid Night";
            }
            else if (timeStamp.Hour >= 4 && timeStamp.Hour <= 7)
            {
                timeperiod = "Early Morning";
            }
            else if (timeStamp.Hour >= 8 && timeStamp.Hour <= 11)
            {
                timeperiod = "Morning";
            }
            else if (timeStamp.Hour >= 12 && timeStamp.Hour <= 15)
            {
                timeperiod = "Afternoon";
            }
            else if (timeStamp.Hour >= 16 && timeStamp.Hour <= 19)
            {
                timeperiod = "Evening";
            }
            else if (timeStamp.Hour >= 20 && timeStamp.Hour <= 23)
            {
                timeperiod = "Night";
            }
            return timeperiod;
        }

        public List<MeterInfoModel> getMeterAndBreakerDetailsList(string serverName)
        {
            List<MeterInfoModel> meterList = new List<MeterInfoModel>();
            try
            {
                SqlConnection getMeterListConnection = ConnectionManager.Instance().GetPISQLConnection(serverName);
                ConnectionManager.Instance().OpenSQLConnection(getMeterListConnection);

                SqlCommand cmdMeterList = new SqlCommand("SELECT DISTINCT(PowerScout), [Breaker Details],[Building] FROM (SELECT TOP 1000 * FROM PowergridView order by TimeStamp DESC) as a", getMeterListConnection);
                SqlDataReader meterListRawDataReader = cmdMeterList.ExecuteReader();
                while (meterListRawDataReader.Read())
                {
                    meterList.Add(
                        new MeterInfoModel()
                        {
                            PowerScout = meterListRawDataReader["PowerScout"].ToString(),
                            Breaker_details = meterListRawDataReader["Breaker Details"].ToString(),
                            BuildingName = meterListRawDataReader["Building"].ToString(),
                            PiServerName = serverName
                        }
                       );
                }
                ConnectionManager.Instance().CloseSQLConnection(getMeterListConnection);
                return meterList;
            }
            catch (Exception ex)
            {
                Utility.Log("Exception occured in get Meter and Breaker details List " + ex.Message);
                Utility.LogException(ex);
                return meterList;
            }
        }

        public List<MeterInfoModel> getMeterDetails(string serverName)
        {

            List<MeterInfoModel> meterList = new List<MeterInfoModel>();
            SqlConnection azureSQLConnection = new SqlConnection();
            try
            {
                azureSQLConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                azureSQLConnection.Open();
                SqlCommand cmdGetMeters = new SqlCommand("Select DISTINCT(PowerScout),PiServerName,Breaker_details,UTCConversionTime from MeterDetails ", azureSQLConnection);

                SqlDataReader meterListDataReader = cmdGetMeters.ExecuteReader();

                while (meterListDataReader.Read())
                {
                    MeterInfoModel meter = new MeterInfoModel();

                    if (meterListDataReader["PowerScout"] != DBNull.Value)
                        meter.PowerScout = meterListDataReader["PowerScout"].ToString();
                    if (meterListDataReader["Breaker_details"] != DBNull.Value)
                        meter.Breaker_details = meterListDataReader["Breaker_details"].ToString();
                    if (meterListDataReader["PiServerName"] != DBNull.Value)
                        meter.PiServerName = meterListDataReader["PiServerName"].ToString();
                    if (meterListDataReader["UTCConversionTime"] != DBNull.Value)
                        meter.UTCConversionTime = Convert.ToDouble(meterListDataReader["UTCConversionTime"]);

                    meterList.Add(meter);
                }
            }
            catch (Exception e)
            {
                Utility.Log("Exception Occured getMeterDetails()" + e.Message);
                Utility.LogException(e);
                throw e;
            }
            finally
            {
                azureSQLConnection.Close();
            }
            return meterList;
        }


        private void InsertSensorData(string piServerName)
        {
            while (true)
            {
                double timeZone = GetAndTimezone();
                SensorDataProcessor.Instance().InsertSensorData(piServerName, timeZone);
            }

        }


        //public void InsertMeterData(string piServerName, string powerScout)
        // {
        //     try
        //     {

        //         var meterList = this.getMeterDetails(piServerName);

        //         //ProcessedDataModel processedDataInfo = BlobStorageManager.Instance().GetLastProcessedData<ProcessedDataModel>(piServerName, Constants.THRESHOLD_SENSOR_STORAGE_FILENAME_PREFIX);
        //         //if (processedDataInfo == null)
        //         //    processedDataInfo = new ProcessedDataModel();

        //         //DateTime processedTimestamp = Convert.ToDateTime(processedDataInfo.MeterTimestamp);
        //         //DateTime firstEntryTimeStamp = processedTimestamp;
        //         //To do need to check this
        //         //if (processedTimestamp == null || processedTimestamp == DateTime.MinValue || processedTimestamp == default(DateTime))
        //         //{
        //         //    SqlConnection meterFirstEntryConn = ConnectionManager.Instance().GetPISQLConnection(piServerName);
        //         //    ConnectionManager.Instance().OpenSQLConnection(meterFirstEntryConn);
        //         //    //Read data from PI Server
        //         //    SqlCommand getTimestamp = new SqlCommand("SELECT TOP 1 TimeStamp FROM SensorData order by Timestamp", sensorFirstEntryConn);

        //         //    SqlDataReader result = getTimestamp.ExecuteReader();
        //         //    while (result.Read()) //Runs only once
        //         //    {
        //         //        firstEntryTimeStamp = (DateTime)result[0];
        //         //    }
        //         //    ConnectionManager.Instance().CloseSQLConnection(sensorFirstEntryConn);
        //         //}

        //         //SqlConnection azureSQLConnection = new SqlConnection();
        //         //azureSQLConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
        //         //azureSQLConnection.Open();

        //         SqlConnection getMeterDataConn = ConnectionManager.Instance().GetPISQLConnection(piServerName);
        //         ConnectionManager.Instance().OpenSQLConnection(getMeterDataConn);
        //         SqlCommand getMeterDataCommand = new SqlCommand("SELECT top 1 [Breaker Details] FROM PowerGridView where powerScout = @meter", getMeterDataConn);
        //         // processedTimestamp = firstEntryTimeStamp;

        //         getMeterDataCommand.Parameters.Add(new SqlParameter("@meter", powerScout));
        //         SqlDataReader piMeterDataReader = getMeterDataCommand.ExecuteReader();
        //         try
        //         {
        //             while (piMeterDataReader.Read())
        //             {
        //                 var meterDetail = meterList.Where(meter => meter.PowerScout.Equals(powerScout)).FirstOrDefault();
        //                 if (meterDetail == null)
        //                 {
        //                     this.AddNewMeterToAzureAndGenerateNotification(powerScout, piMeterDataReader["Breaker Details"].ToString(), piServerName);

        //                     meterList = this.getMeterDetails(piServerName);
        //                     // meterDetail = meterList.Where(meter => meter.PowerScout.Equals(piMeterDataReader["PowerScout"])).FirstOrDefault();
        //                     // this.AddNewAlert(0, sensorDetail.Sensor_Id, "Device Alert", "New device found with name " + sensorDetail.Sensor_Name, DateTime.UtcNow, 0, piServerName);
        //                 }

        //             }
        //         }
        //         catch (Exception e)
        //         {

        //         }
        //         finally
        //         {
        //             ConnectionManager.Instance().CloseSQLConnection(getMeterDataConn);
        //         }
        //         //processedDataInfo.MeterTimestamp = proc;
        //         //Console.WriteLine("Storing Sensor Details : " + processedDataInfo);
        //         //BlobStorageManager.Instance().SetLastProcessedData<ProcessedSensorDataModel>(piServerName, Constants.THRESHOLD_SENSOR_STORAGE_FILENAME_PREFIX, processedDataInfo);

        //     }
        //     catch (Exception ex)
        //     {

        //     }
        // }



        void InsertMetersIntoAzure(List<MeterInfoModel> meterList, double UTCConversionDifference)
        {
            try
            {
                foreach (var meter in meterList)
                {
                    //Insert New Sensor into Azure
                    SqlConnection azureSQLConnection = new SqlConnection();
                    azureSQLConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                    azureSQLConnection.Open();

                    int buildingId = 0;
                    Utility.Log("Inserting Building  : " + meter.BuildingName);
                    //if exist building name in building table
                    //insert this building into building table
                    string query = " IF NOT EXISTS(SELECT * FROM[dbo].[Building] WHERE [BuildingName] = @BuildingName) BEGIN INSERT INTO [dbo].[Building]([BuildingName]) OUTPUT INSERTED.[BuildingID] VALUES (@BuildingName) END";
                    SqlCommand getBuildingIdCmd = new SqlCommand(query, azureSQLConnection);
                    getBuildingIdCmd.Parameters.Add(new SqlParameter("@BuildingName", meter.BuildingName));
                    SqlDataReader result = getBuildingIdCmd.ExecuteReader();
                    while (result.Read()) //Runs only once
                    {
                        buildingId = Convert.ToInt32(result[0]);
                    }
                    result.Close();
                    //
                    if (buildingId == 0)
                    {
                        string getBuildingIDQuery = "Select [BuildingID] from [dbo].[Building] where BuildingName = @BuildingName";
                        SqlCommand getBuildingId = new SqlCommand(getBuildingIDQuery, azureSQLConnection);
                        getBuildingId.Parameters.Add(new SqlParameter("@BuildingName", meter.BuildingName));
                        SqlDataReader buildingIdReader = getBuildingId.ExecuteReader();
                        while (buildingIdReader.Read()) //Runs only once
                        {
                            buildingId = Convert.ToInt32(buildingIdReader[0]);
                        }
                        buildingIdReader.Close();
                    }
                    if (buildingId != 0)
                    {
                        meter.BuildingId = buildingId;
                        meter.UTCConversionTime = UTCConversionDifference;
                        Utility.Log("Inserting Meter  : " + meter.PowerScout);
                        SqlCommand cmdInsertMeters = new SqlCommand("INSERT INTO MeterDetails(PowerScout,Breaker_details,PiServerName,UTCConversionTime,BuildingId) VALUES (@PowerScout,@Breaker_details,@PiServerName,@UTCConversionTime,@BuildingId)", azureSQLConnection);
                        cmdInsertMeters.Parameters.Add(new SqlParameter("@PowerScout", meter.PowerScout));
                        cmdInsertMeters.Parameters.Add(new SqlParameter("@Breaker_details", meter.Breaker_details));
                        cmdInsertMeters.Parameters.Add(new SqlParameter("@PiServerName", meter.PiServerName));
                        cmdInsertMeters.Parameters.Add(new SqlParameter("@UTCConversionTime", meter.UTCConversionTime));
                        cmdInsertMeters.Parameters.Add(new SqlParameter("@BuildingId", meter.BuildingId));
                        cmdInsertMeters.ExecuteNonQuery();
                    }
                    else
                    {
                        Utility.Log("InsertMetersIntoAzure() Building iD is 0 for meter :: " + meter.PowerScout);
                    }

                    azureSQLConnection.Close();

                }
                Utility.Log("End of Method InsertMetersIntoAzure()");
            }
            catch (Exception e)
            {
                Utility.Log("Exception Occured : InsertMetersIntoAzure()" + e.Message);
                Utility.LogException(e);
            }
        }
        public double GetAndTimezone()
        {
            TimeZone localZone = TimeZone.CurrentTimeZone;
            TimeSpan localTimeOffset = localZone.GetUtcOffset(DateTime.Now);
            return localTimeOffset.TotalHours;
        }
        #endregion
    }
}

