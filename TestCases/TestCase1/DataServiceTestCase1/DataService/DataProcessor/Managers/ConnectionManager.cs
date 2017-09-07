using DataProcessor.DataModels;
using DataProcessor.DataModels.ApiModels;
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
            Console.WriteLine("Get Pi server Settings :: "+ PIServer);
            DatabaseInfo = new DatabaseInfo();
            //To Do
            //Looping pi server list and call get db connection string api and update DatabaseInfoList  with the same
            //Going to loop Pi server List
            //foreach (string server in PIServerList)
            //{
            //    Console.WriteLine("In Loop server :: "+server);
            //    PIConfigurationRequestModel piConfigRequestModel = new PIConfigurationRequestModel()
            //    {
            //        PiServerName = server
            //    };
            //    string requestJson = JsonConvert.SerializeObject(piConfigRequestModel);
            //    Console.WriteLine("Request Json ::"+requestJson);
            //    string responseJson = HttpManager.Instance().GetPiServerConfigurationByName(requestJson).Result;
            //    Console.WriteLine("Response Json ::" + responseJson);
            //    if (responseJson != null)
            //    {
            //        availableServers.Add(server);
            //        PIConfigurationResponseModel piConfigResponseModel = JsonConvert.DeserializeObject<PIConfigurationResponseModel>(responseJson);
            //        DatabaseInfoList.Add(new DatabaseInfo() { Name = server, ConnectionString = piConfigResponseModel.PiServerURL });
            //    }
            //}

            string connectionString = GetConnectionStringFromDB(PIServer);
            if (!string.IsNullOrEmpty(connectionString))
            {
                DatabaseInfo = new DatabaseInfo() { Name = PIServer, ConnectionString = connectionString };
            }
            else
            {
                Console.WriteLine("Failed to fetch connection string from DB for PI Server = " + PIServer);
            }
        }

        public string GetPIServer()
        {
            return PIServer;
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
                connectionString =  Convert.ToString(reader[0]);
            }
            azureConnection.Close();
            return connectionString;
        }


        #endregion

    }
}
