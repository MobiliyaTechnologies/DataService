using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using DataProcessor.DataModels;
using System.Net;
using DataProcessor.Utils;

namespace DataProcessor.Managers
{
    public class ClassScheduleManager
    {
        #region SINGLETON
        private static ClassScheduleManager _instance;

        private ClassScheduleManager()
        {

        }

        public static ClassScheduleManager Instance()
        {
            if (_instance == null)
            {
                _instance = new ClassScheduleManager();
            }
            return _instance;
        }
        #endregion

        #region PRIVATE VARIABLE
        DataTable tableCSV = null;
        #endregion

        /// <summary>
        /// This method should be called when we do a new pi server processing.This method will download respective file and update Table object.
        /// </summary>
        /// <param name="serverName"></param>
        public void ReInitialize(string serverName)
        {
            string filePath = Path.Combine(
                System.Environment.CurrentDirectory,
                "\\"+Constants.CLASS_SCHEDULE_STORAGE_FILE_PREFIX + serverName + Constants.CLASS_SCHEDULE_STORAGE_FILE_EXTENSION);
            if (!File.Exists(filePath))
            {
                BlobStorageManager.Instance().DownloadFileFromBlob(Constants.CLASS_SCHEDULE_STORAGE_FILE_PREFIX+ serverName, Constants.CLASS_SCHEDULE_STORAGE_FILE_EXTENSION, filePath);
            }
            // read table.
            tableCSV = GetDataTableFromCSV(filePath);
        }

        //Converts day value to day i.e. convert 1 into M similarly 2 into T 
        string GetDayFromDayValue(int dayValue)
        {
            string[] dayStringArr = { "U", "M", "T", "W", "R", "F", "S" };
            return dayStringArr[dayValue];
        }
        DataTable GetDataTableFromCSV(string csvPath)
        {
            try
            {
                // your code here 
                string CSVFilePathName = csvPath;
                string[] Lines = File.ReadAllLines(CSVFilePathName);
                string[] Fields;
                Fields = Lines[0].Split(new char[] { ',' });
                int Cols = Fields.GetLength(0);
                DataTable dt = new DataTable();
                //1st row must be column names; force lower case to ensure matching later on.
                for (int i = 0; i < Cols; i++)
                    dt.Columns.Add(Fields[i].ToLower(), typeof(string));
                DataRow Row;
                for (int i = 1; i < Lines.GetLength(0); i++)
                {
                    Fields = Lines[i].Split(new char[] { ',' });
                    Row = dt.NewRow();
                    for (int f = 0; f < Cols; f++)
                        Row[f] = Fields[f];
                    dt.Rows.Add(Row);
                }
                return dt;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} > Exception: {1}", DateTime.Now, ex.Message);
                Console.ResetColor();
                Utility.Log("Exception Occured :: GetTableFromCSV() :: " + ex.Message);
                return null;
            }
        }

        public ClassOccupanyDetails ProcessDataRow(string serial_number, DateTime timestamp)
        {
            ClassOccupanyDetails classModel = new ClassOccupanyDetails();
            try
            {
                //Console.WriteLine("ProcessDataRow()");
                // Console.WriteLine("ProcessDataRow() :: RowObject ==" + rowObject);
                //Console.WriteLine("ProcessDataRow() :: tableCSV == " + tableCSV);
                foreach (DataRow row in tableCSV.Select().Where(x => x.Field<string>("Location").Contains("CHEM")))
                {
                    if (IsValidSessionMonth(row, timestamp))
                    {
                        //Step 3 Checking for Valid Day
                        if (IsValidDay(row, timestamp))
                        {
                            //Step 4 Checking for valid Class timing in a day
                            if (IsValidClassTiming(row, timestamp))
                            {
                                classModel.IsClassOccupied = 1;
                                classModel.ClassTotalCapacity = Convert.ToInt32(row["Capacity"].ToString());
                                classModel.ClassOccupiedValue = Convert.ToInt32(row["Actual"]);
                            }
                        }
                    }
                }


                //Below code was done after considering the class and powerscout for one to one mapping but now commented because powerscouts are common.
                /*
            //step 1 comparing serial number          
            foreach (DataRow row in tableCSV.Select().Where(x => x.Field<string>("Powerscout meter serial number").Equals(serial_number)))
            {



                //Step 2 Checking for valid month
                if (IsValidSessionMonth(row, timestamp))
                {
                    //Step 3 Checking for Valid Day
                    if (IsValidDay(row, timestamp))
                    {
                        //Step 4 Checking for valid Class timing in a day
                        if (IsValidClassTiming(row, timestamp))
                        {
                            classModel.IsClassOccupied = 1;
                            classModel.ClassTotalCapacity = Convert.ToInt64(row["Capacity"].ToString());
                            classModel.ClassOccupiedValue = Convert.ToInt64(row["Actual"]);
                        }
                    }
               

            }
             }*/
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                Console.ResetColor();
                Utility.Log("Exception Occured :: ProcessDataRow() :: " + exception.Message);
            }
            return classModel;
        }


        DateTime GetSessionStartTime(object value)
        {
            string strValue = value.ToString();
            string[] strArr = strValue.Split(new char[] { '-' });
            //string dateStr = strArr[0] + "/2016";
            string dateStr = strArr[0] + "/" + DateTime.UtcNow.Year.ToString();
            var date = DateTime.ParseExact(dateStr, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentCulture);
            return date;
        }
        DateTime GetSessionEndTime(object value)
        {
            string strValue = value.ToString();
            string[] strArr = strValue.Split(new char[] { '-' });
            //string dateStr = strArr[1] + "/2016";
            string dateStr = strArr[1] + "/" + DateTime.UtcNow.Year.ToString();
            var date = DateTime.ParseExact(dateStr, "MM/dd/yyyy", System.Globalization.CultureInfo.CurrentCulture);
            return date;
        }

        //step 2 Checking session Months
        bool IsValidSessionMonth(DataRow row, DateTime timestamp)
        {
            return (timestamp >= GetSessionStartTime(row["date (mm/dd)"]) && timestamp <= GetSessionEndTime(row["date (mm/dd)"]));
        }
        //Step 3 Checking for Valid Day
        bool IsValidDay(DataRow row, DateTime timestamp)//Day could be from MTWRF
        {
            return (row["Days"].ToString().Contains(GetDayFromDayValue(((int)((DateTime)timestamp).DayOfWeek))));
        }


        bool IsValidClassTiming(DataRow row, DateTime timestamp)
        {
            string[] timings = row["Time"].ToString().Split(new char[] { '-' });
            DateTime startClassTime = GetClassHourOfDay(timings[0], (DateTime)timestamp);
            DateTime endClassTime = GetClassHourOfDay(timings[1], (DateTime)timestamp);
            return
              (/*((((DateTime)rowObject.TimeStamp).Date == startClassTime.Date)&&(((DateTime)rowObject.TimeStamp).Date == endClassTime.Date)) && */
              ((((DateTime)timestamp).TimeOfDay.TotalMinutes >= startClassTime.TimeOfDay.TotalMinutes) &&
                (((DateTime)timestamp).TimeOfDay.TotalMinutes <= endClassTime.TimeOfDay.TotalMinutes)));

        }

        DateTime GetClassHourOfDay(string value, DateTime dt)
        {
            var minutes = DateTime.Parse(value).TimeOfDay.TotalMinutes;
            return dt.Date.AddMinutes(minutes);
        }
    }
}
