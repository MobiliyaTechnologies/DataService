using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ServiceProcess;

namespace LocalStarter
{
    static class Program
    {
        static void Main(string[] args)
        {
            EventLog eventLog = new EventLog();
            eventLog.Source = "Application";
            DataProcessor.DataProcessor process = new DataProcessor.DataProcessor(eventLog);
            process.ProcessData();

        }
    }
}
