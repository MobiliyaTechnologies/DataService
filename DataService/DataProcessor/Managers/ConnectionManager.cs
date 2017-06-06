using DataProcessor.DataModels;
using DataProcessor.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.Managers
{
    public class ConnectionManager
    {


        #region PUBLIC VARIABLES
        private string PIServer { get; set; }
        #endregion

        #region PRIVATE VARIABLES
        private DatabaseInfo DatabaseInfo { get; set; }
        #endregion

        #region SINGLETON
        private static ConnectionManager _instance;

        private ConnectionManager() { }

        public static ConnectionManager Instance()
        {
            if (_instance == null)
            {
                _instance = new ConnectionManager();
            }
            return _instance;
        }
        #endregion

        #region PUBLIC METHODS

        public void Initialize()
        {
            Console.WriteLine("In Initialize");
            PIServer = ConfigurationSettings.PiServer;
            Console.WriteLine("Get Pi server Settings :: " + PIServer);
            DatabaseInfo = new DatabaseInfo();

            string connectionString = null;//R GetConnectionStringFromDB(PIServer);
           
            connectionString = ConfigurationSettings.PiServerConnectionString;
           
            if (!string.IsNullOrEmpty(connectionString))
            {
                DatabaseInfo = new DatabaseInfo() { Name = PIServer, ConnectionString = connectionString };
            }
            else
            {
                Utility.Log("Failed to fetch connection string from Configuration for PI Server = " + PIServer);
            }

            ConfigurationSettings.SetCloudConfigDataModel(GeCloudConfigurationSettings(PIServer));
        }

        public string GetPIServer()
        {
            return PIServer;
        }

        private CloudConfigurationSettingsModel GeCloudConfigurationSettings(string piServerName)
        {
            CloudConfigurationSettingsModel settingModel = new CloudConfigurationSettingsModel();
            try
            {
                SqlConnection getConfigurationConnection = new SqlConnection(ConfigurationSettings.AzureConnectionString);
                getConfigurationConnection.Open();
                //SqlCommand sqlCommandConfigurationList = new SqlCommand("SELECT ApplicationId,SenderId,Receiver,FCMURL,ClickAction,Icon FROM Configuration", getConfigurationListConnection);
                SqlCommand sqlCommandConfiguration = new SqlCommand("Select ConfigurationKey,ConfigurationValue from ApplicationConfigurationEntry where ApplicationConfigurationId = (select id from applicationconfiguration where ConfigurationType = @Firebase)", getConfigurationConnection);
                sqlCommandConfiguration.Parameters.Add(new SqlParameter("@Firebase", Constants.CLOUD_CONFIGURATION_TYPE_FIREBASE));
                SqlDataReader configurationsqlDataReader = sqlCommandConfiguration.ExecuteReader();

                while (configurationsqlDataReader.Read())
                {
                    switch (configurationsqlDataReader["ConfigurationKey"].ToString())
                    {

                        case Constants.CLOUD_CONFIGURATION_NOTIFICATIONAUTHORIZATION_Key:

                            { settingModel.ApplicationId = configurationsqlDataReader["ConfigurationValue"].ToString(); }
                            break;
                        case Constants.CLOUD_CONFIGURATION_FIREBASE_API_KEY:

                            { settingModel.ApiKey = configurationsqlDataReader["ConfigurationValue"].ToString(); }
                            break;
                        case Constants.CLOUD_CONFIGURATION_NOTIFICATION_SENDER_KEY:

                            { settingModel.NotificationSender = configurationsqlDataReader["ConfigurationValue"].ToString(); }
                            break;
                        case Constants.CLOUD_CONFIGURATION_NOTIFICATION_RECEIVER_KEY:

                            { settingModel.NotificationReceiver = configurationsqlDataReader["ConfigurationValue"].ToString(); }
                            break;
                        case Constants.CLOUD_CONFIGURATION_NOTIFICATION_CLICK_ACTION_KEY:

                            { settingModel.ClickAction = configurationsqlDataReader["ConfigurationValue"].ToString(); }
                            break;
                    }

                }
                settingModel.FCMURL = Constants.CLOUD_CONFIGURATION_FIREBASE_SEND_URL_VALUE;
                settingModel.Icon = Constants.CLOUD_CONFIGURATION_FIREBASE_ICON_VALUE;
                configurationsqlDataReader.Close();

                //for blob storage configuration
                sqlCommandConfiguration = new SqlCommand("Select ConfigurationKey,ConfigurationValue from ApplicationConfigurationEntry where ApplicationConfigurationId = (select id from applicationconfiguration where ConfigurationType= @BlobStorage )", getConfigurationConnection);
                sqlCommandConfiguration.Parameters.Add(new SqlParameter("@BlobStorage", Constants.CLOUD_CONFIGURATION_TYPE_BLOB_STORAGE));

                configurationsqlDataReader = sqlCommandConfiguration.ExecuteReader();
                while (configurationsqlDataReader.Read())
                {

                    if (configurationsqlDataReader["ConfigurationKey"].ToString() == Constants.CLOUD_CONFIGURATION_STORAGE_CONNECTION_STRING_KEY)
                    {
                        settingModel.BlobStorageURL = configurationsqlDataReader["ConfigurationValue"].ToString();
                    }
                }

                ConnectionManager.Instance().CloseSQLConnection(getConfigurationConnection);
                return settingModel;
            }
            catch (Exception ex)
            {
               Utility.Log("Exception occured in get cloud Configuration settings " + ex.Message);
                return settingModel;
            }
        }



     

        /// <summary>
        /// Method accepts server name and make a new object of sql connection with connection string as per server name.
        /// </summary>
        /// <param name="serverName"></param>
        /// <returns></returns>
        public SqlConnection GetPISQLConnection(string serverName)
        {
            SqlConnection connection = new SqlConnection();
            connection.ConnectionString = GetPISQLConnectionString(serverName);
            return connection;
        }
        public void OpenSQLConnection(SqlConnection connection)
        {
            connection.Open();
        }

        public void CloseSQLConnection(SqlConnection connection)
        {
            connection.Close();
        }

        #endregion

        #region PRIVATE METHODS

        string GetPISQLConnectionString(string serverName)
        {

            return DatabaseInfo.ConnectionString;
        }

        string GetConnectionStringFromDB(string serverName)
        {
            string connectionString = null;
            string query = "Select PiServerURL from PiServer where PiServerName = @PiServerName";
            SqlConnection azureConnection = new SqlConnection(ConfigurationSettings.AzureConnectionString);
            azureConnection.Open();
            SqlCommand cmd = new SqlCommand(query, azureConnection);
            cmd.Parameters.Add(new SqlParameter("PiServerName", serverName));
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) //Runs only once
            {
                connectionString = Convert.ToString(reader[0]);
            }
            azureConnection.Close();
            return connectionString;
        }

        public void UpdateUTCTimeDifference(string serverName, double utcConversionTime)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection();
                sqlConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                sqlConnection.Open();
                SqlCommand sqlCommandUpdateUTCConversionTime = new SqlCommand("Update PiServer set UTCConversionTime = @utcConversionTime where PiServerName = @serverName", sqlConnection);
                sqlCommandUpdateUTCConversionTime.Parameters.Add(new SqlParameter("@utcConversionTime", utcConversionTime));
                sqlCommandUpdateUTCConversionTime.Parameters.Add(new SqlParameter("@serverName", serverName));

                sqlCommandUpdateUTCConversionTime.ExecuteNonQuery();


                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured ::UpdateUTCTimeDifference()  in Update UTC Time Difference "+ex.Message);
                Utility.Log("Exception occured ::UpdateUTCTimeDifference()  in Update UTC Time Difference "+ex.Message);
            }
        }

        public void InserPIConfigurationDetailsToDB(PIConfigurationInfoModel piConfig)
        {
            try
            {
                SqlConnection sqlConnection = new SqlConnection();
                sqlConnection.ConnectionString = ConfigurationSettings.AzureConnectionString;
                sqlConnection.Open();
                SqlCommand sqlCommandUpdateUTCConversionTime = new SqlCommand("Insert into PiServer(PiServerName,PiServerDesc,PiServerURL,CreatedOn,UTCConversionTime) VALUES (@PiServerName,@PiServerDesc,@PiServerURL,@CreatedOn,@UTCConversionTime)", sqlConnection);
                sqlCommandUpdateUTCConversionTime.Parameters.Add(new SqlParameter("@PiServerName", piConfig.PiServerName));
                sqlCommandUpdateUTCConversionTime.Parameters.Add(new SqlParameter("@PiServerDesc", piConfig.PiServerDesc));
                sqlCommandUpdateUTCConversionTime.Parameters.Add(new SqlParameter("@PiServerURL", piConfig.PiServerURL));
                sqlCommandUpdateUTCConversionTime.Parameters.Add(new SqlParameter("@CreatedOn", piConfig.CreatedOn));
                sqlCommandUpdateUTCConversionTime.Parameters.Add(new SqlParameter("@UTCConversionTime", piConfig.UTCConversionTime));

                sqlCommandUpdateUTCConversionTime.ExecuteNonQuery();


                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occured in InserPIConfigurationDetailsToDB", ex.Message);
                Utility.Log("Exception occured ::InserPIConfigurationDetailsToDB()" + ex.Message);
            }
        }


        #endregion

    }
}
