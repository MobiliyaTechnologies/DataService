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
        private List<string> PIServerList { get; set; }
        #endregion

        #region PRIVATE VARIABLES
        private List<DatabaseInfo> DatabaseInfoList { get; set; }
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
            PIServerList = new List<string>();
            Console.WriteLine("In Initialize");
            string piServers = ConfigurationSettings.PiServers;
            Console.WriteLine("Get Pi server Settings :: "+piServers);
            PIServerList = piServers.Split(';').ToList<string>();
            DatabaseInfoList = new List<DatabaseInfo>();
            //Available server requires if user added multiple server in config file and those server is not available in database
            List<string> availableServers = new List<string>();

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
            availableServers.Add(PIServerList.ElementAt(0));
            DatabaseInfoList.Add(new DatabaseInfo() { Name = PIServerList.ElementAt(0), ConnectionString = ConfigurationSettings.PIConnectionString});

            PIServerList = availableServers;
        }

        public List<string> GetPIServerList()
        {
            return PIServerList;
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
            //TO DO
            return DatabaseInfoList.Where(x => x.Name == serverName).FirstOrDefault().ConnectionString;
        }


        #endregion

    }
}
