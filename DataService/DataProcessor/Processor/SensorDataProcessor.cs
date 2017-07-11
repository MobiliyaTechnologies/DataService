using DataProcessor.DataModels;
using DataProcessor.Managers;
using DataProcessor.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Processor
{
    public class SensorDataProcessor
    {
        #region SINGLETON
        private static SensorDataProcessor _instance;

        private SensorDataProcessor() { }

        public static SensorDataProcessor Instance()
        {
            if (_instance == null)
            {
                _instance = new SensorDataProcessor();
            }
            return _instance;
        }
        #endregion


        public void InsertSensorData(string piServerName, double timeZone)
        {
            try
            {
                
                var sensorList = this.GetSensors(piServerName);

                ProcessedSensorDataModel processedDataInfo = BlobStorageManager.Instance().GetLastProcessedData<ProcessedSensorDataModel>(piServerName, Constants.THRESHOLD_SENSOR_STORAGE_FILENAME_PREFIX);
                if (processedDataInfo == null)
                    processedDataInfo = new ProcessedSensorDataModel();

                DateTime processedTimestamp = processedDataInfo.ProcessedTimestamp;
                DateTime firstEntryTimeStamp = processedTimestamp;
                //To do need to check this
                if (processedTimestamp == null || processedTimestamp == DateTime.MinValue || processedTimestamp == default(DateTime))
                {
                    SqlConnection sensorFirstEntryConn = ConnectionManager.Instance().GetPISQLConnection(piServerName);
                    ConnectionManager.Instance().OpenSQLConnection(sensorFirstEntryConn);
                    //Read data from PI Server
                    SqlCommand getTimestamp = new SqlCommand("SELECT TOP 1 TimeStamp FROM SensorData order by Timestamp", sensorFirstEntryConn);

                    SqlDataReader result = getTimestamp.ExecuteReader();
                    while (result.Read()) //Runs only once
                    {
                        firstEntryTimeStamp = (DateTime)result[0];
                    }
                    ConnectionManager.Instance().CloseSQLConnection(sensorFirstEntryConn);
                }

                SqlConnection getPiSensorDataConn = ConnectionManager.Instance().GetPISQLConnection(piServerName);
                ConnectionManager.Instance().OpenSQLConnection(getPiSensorDataConn);
                SqlCommand getPiSensorDataCommand = (processedTimestamp == null || processedTimestamp == DateTime.MinValue || processedTimestamp == default(DateTime)) ? new SqlCommand("SELECT * FROM SensorData WHERE TimeStamp >= @TimeStamp", getPiSensorDataConn) : new SqlCommand("SELECT * FROM SensorData WHERE TimeStamp > @TimeStamp", getPiSensorDataConn);
                processedTimestamp = firstEntryTimeStamp;

                getPiSensorDataCommand.Parameters.Add(new SqlParameter("@TimeStamp", processedTimestamp));
                SqlDataReader piSensorDataReader = getPiSensorDataCommand.ExecuteReader();
                try
                {
                    while (piSensorDataReader.Read())
                    {
                        var sensorDetail = sensorList.Where(sensor => sensor.Sensor_Name.Equals(piSensorDataReader["Name"])).FirstOrDefault();
                        if (sensorDetail == null)
                        {
                            this.AddNewSensorToAzureAndGenerateNotification(piSensorDataReader["Name"].ToString(), piServerName);
                            //reinit sensor list after insertion of new sensor
                            sensorList = this.GetSensors(piServerName);
                            sensorDetail = sensorList.Where(sensor => sensor.Sensor_Name.Equals(piSensorDataReader["Name"])).FirstOrDefault();
                            this.AddNewAlert(0, sensorDetail.Sensor_Id, "Device Alert", "New device found with name " + sensorDetail.Sensor_Name, DateTime.UtcNow, 0, piServerName);
                        }
                        DateTime utcDate;
                        DateTime localDate;
                        DateTime.TryParse(piSensorDataReader["TimeStamp"].ToString(), out utcDate);
                        localDate = utcDate.AddHours(Convert.ToDouble(timeZone));
                        var formattedDate = localDate.ToString(Constants.DATE_TIME_FORMAT);

                        SqlConnection azureSQLConnection = new SqlConnection();
                        azureSQLConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                        azureSQLConnection.Open();
                        SqlCommand updateAzureDBCommand = new SqlCommand("INSERT INTO SensorLiveData (Sensor_Id,Temperature,Brightness,Humidity,Timestamp,PiServerName) VALUES (@SensorId,@Temperature,@Brightness,@Humidity,@Timestamp,@PiServerName)", azureSQLConnection);
                        updateAzureDBCommand.Parameters.Add(new SqlParameter("@SensorId", sensorDetail.Sensor_Id));
                        updateAzureDBCommand.Parameters.Add(new SqlParameter("@Temperature", piSensorDataReader["Temperature"]));
                        updateAzureDBCommand.Parameters.Add(new SqlParameter("@Brightness", piSensorDataReader["Brightness"]));
                        updateAzureDBCommand.Parameters.Add(new SqlParameter("@Humidity", piSensorDataReader["Humidity"]));
                        updateAzureDBCommand.Parameters.Add(new SqlParameter("@Timestamp", formattedDate));
                        updateAzureDBCommand.Parameters.Add(new SqlParameter("@PiServerName", piServerName));
                        updateAzureDBCommand.ExecuteNonQuery();
                        processedTimestamp = Convert.ToDateTime(piSensorDataReader["TimeStamp"]);
                        azureSQLConnection.Close();
                    }
                }
                catch (Exception e)
                {

                }
                finally
                {
                    ConnectionManager.Instance().CloseSQLConnection(getPiSensorDataConn);
                }
                processedDataInfo.ProcessedTimestamp = processedTimestamp;
                Console.WriteLine("Storing Sensor Details : " + processedDataInfo);
                BlobStorageManager.Instance().SetLastProcessedData<ProcessedSensorDataModel>(piServerName, Constants.THRESHOLD_SENSOR_STORAGE_FILENAME_PREFIX, processedDataInfo);

            }
            catch (Exception ex)
            {

            }
        }

        private void AddNewSensorToAzureAndGenerateNotification(string sensorName, string piServerName)
        {
            try
            {
                //Insert New Sensor into Azure
                SqlConnection azureSQLConnection = new SqlConnection();
                azureSQLConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                azureSQLConnection.Open();
                SqlCommand cmdInsertSensors = new SqlCommand("INSERT INTO SensorMaster(Sensor_Name, PiServerName) VALUES (@SensorName,@PiServerName)", azureSQLConnection);
                cmdInsertSensors.Parameters.Add(new SqlParameter("@SensorName", sensorName));
                cmdInsertSensors.Parameters.Add(new SqlParameter("@PiServerName", piServerName));
                cmdInsertSensors.ExecuteNonQuery();
                azureSQLConnection.Close();
                //Generate Notification
                string title = "Device Alert";
                string body = "New device with name " + sensorName + " has been added";
                Utility.SendNotification(title, body);
            }
            catch (Exception e)
            {

            }

        }

        public void AddNewAlert(int sensorLogId, int sensor_Id, string alertType, string description, DateTime timestamp, int isAcknowledged, string piServerName)
        {
            try
            {
                //Insert New Sensor into Azure
                SqlConnection azureSQLConnection = new SqlConnection();
                azureSQLConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                azureSQLConnection.Open();
                SqlCommand cmdInsertSensors = new SqlCommand("INSERT INTO Alerts(Sensor_Log_Id,Sensor_Id,Alert_Type,Description,Timestamp,Is_Acknowledged, PiServerName) VALUES (@Sensor_Log_Id,@Sensor_Id,@Alert_Type,@Description,@Timestamp,@Is_Acknowledged,@PiServerName)", azureSQLConnection);
                cmdInsertSensors.Parameters.Add(new SqlParameter("@Sensor_Log_Id", sensorLogId));
                cmdInsertSensors.Parameters.Add(new SqlParameter("@Sensor_Id", sensor_Id));
                cmdInsertSensors.Parameters.Add(new SqlParameter("@Alert_Type", alertType));
                cmdInsertSensors.Parameters.Add(new SqlParameter("@Description", description));
                cmdInsertSensors.Parameters.Add(new SqlParameter("@Timestamp", timestamp));
                cmdInsertSensors.Parameters.Add(new SqlParameter("@Is_Acknowledged", (byte)isAcknowledged));
                cmdInsertSensors.Parameters.Add(new SqlParameter("@PiServerName", piServerName));
                cmdInsertSensors.ExecuteNonQuery();
                azureSQLConnection.Close();
            }
            catch (Exception ex)
            {
            }
        }

        private List<SensorMasterModel> GetSensors(string piServerName)
        {
            List<SensorMasterModel> sensorList = new List<SensorMasterModel>();
            SqlConnection azureSQLConnection = new SqlConnection();
            try
            {
                azureSQLConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                azureSQLConnection.Open();
                SqlCommand cmdGetSensors = new SqlCommand("Select * from SensorMaster where PiServerName = @PiServerName", azureSQLConnection);
                cmdGetSensors.Parameters.Add(new SqlParameter("@PiserverName", piServerName));
                SqlDataReader sensorListDataReader = cmdGetSensors.ExecuteReader();

                while (sensorListDataReader.Read())
                {
                    SensorMasterModel sensor = new SensorMasterModel();
                    if (sensorListDataReader["Sensor_Id"] != DBNull.Value)
                        sensor.Sensor_Id = Convert.ToInt32(sensorListDataReader["Sensor_Id"]);
                    if (sensorListDataReader["Sensor_Name"] != DBNull.Value)
                        sensor.Sensor_Name = sensorListDataReader["Sensor_Name"].ToString();
                    if (sensorListDataReader["Room_Id"] != DBNull.Value)
                        sensor.Room_Id = Convert.ToInt32(sensorListDataReader["Room_Id"]);
                    if (sensorListDataReader["PiServerName"] != DBNull.Value)
                        sensor.PiServerName = Convert.ToString(sensorListDataReader["PiServerName"]);

                    sensorList.Add(sensor);
                }
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                azureSQLConnection.Close();
            }
                         
           
            return sensorList;
        }


    }
}
