using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using Newtonsoft.Json;
using DataProcessor.Utils;

namespace DataProcessor.Managers
{
    public class BlobStorageManager
    {
        #region PRIVATE_VARIABLES
        string storageFileExtension = ".txt";
        CloudBlobClient blobClient;
        #endregion

        #region SINGLETON
        private static BlobStorageManager _instance;

        private BlobStorageManager() { }

        public static BlobStorageManager Instance()
        {
            if (_instance == null)
            {
                _instance = new BlobStorageManager();
            }
            return _instance;
        }
        #endregion

        #region PUBLIC METHODS
        public void ConfigureBlobStorage()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationSettings.StorageConnectionString);
            blobClient = storageAccount.CreateCloudBlobClient();
        }

        public T GetLastProcessedData<T>(string serverName, string fileNamePrefix) where T : class
        {
            try
            {
                string data = ReadValueFromBlobStorage(serverName, fileNamePrefix);
                return JsonConvert.DeserializeObject<T>(data);
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        public void SetLastProcessedData<T>(string serverName, string fileNamePrefix, T value) where T : class
        {
            string jsonData = JsonConvert.SerializeObject(value);
            SaveValueToBlobStorage(serverName, fileNamePrefix, jsonData);
        }

        #endregion

        #region PRIVATE METHODS

        private void SaveValueToBlobStorage(string serverName, string fileNamePrefix, string value)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(Constants.BLOB_STOARAGE_CONTAINER);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileNamePrefix + serverName + storageFileExtension);
            blockBlob.UploadText(value);
        }

        private string ReadValueFromBlobStorage(string serverName, string fileNamePrefix)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(Constants.BLOB_STOARAGE_CONTAINER);
            container.CreateIfNotExists();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileNamePrefix + serverName + storageFileExtension);
            string text = null;
            if (blockBlob.Exists())
            {
                using (var memoryStream = new MemoryStream())
                {
                    blockBlob.DownloadToStream(memoryStream);
                    text = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
            return text;
        }

        public void DownloadFileFromBlob(string fileName,string fileExtension,string storePath)
        {
            CloudBlobContainer container = blobClient.GetContainerReference(Constants.BLOB_STOARAGE_CONTAINER);
            container.CreateIfNotExists();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName + fileExtension);
           
            if (blockBlob.Exists())
            {
                blockBlob.DownloadToFile(storePath, FileMode.Create);
            }
        }
        #endregion
    }
}
