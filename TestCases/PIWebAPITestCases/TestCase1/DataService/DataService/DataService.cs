﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace DataService
{
    public partial class DataService : ServiceBase
    {

        public DataService()
        {
            InitializeComponent();

            if (!EventLog.SourceExists("DataServiceEM"))
            {
                //An event log source should not be created and immediately used.
                //There is a latency time to enable the source, it should be created
                //prior to executing the application that uses the source.
                //Execute this sample a second time to use the new source.
                EventLog.CreateEventSource("DataServiceEM", "DataServiceEM");
                Console.WriteLine("CreatedEventSource");
                Console.WriteLine("Exiting, execute the application a second time to use the source.");
                // The source is created.  Exit the application to allow it to be registered.
                //  return;
            }
            else
            {
                EventLog.Source = "DataServiceEM";
                // Write an informational entry to the event log.    
                EventLog.WriteEntry("Writing to event log.");
            }


        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("Inside on Start.");
            try
            {
                DataProcessor.DataProcessor process = new DataProcessor.DataProcessor(EventLog);
                process.ProcessData();
                EventLog.WriteEntry(" Data service process started.");
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(" Service Exception occured: " + ex.Message);
            }

        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Inside ONStop");
        }
    }
}
