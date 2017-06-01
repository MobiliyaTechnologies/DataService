using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            DataProcessor.DataProcessor process = new DataProcessor.DataProcessor();
            process.ProcessData();

        }
    }
}
