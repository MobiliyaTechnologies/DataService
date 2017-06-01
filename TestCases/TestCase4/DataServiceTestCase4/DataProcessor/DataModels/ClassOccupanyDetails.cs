using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class ClassOccupanyDetails
    {
        public int IsClassOccupied { get; set; }//0 for false and 1 for true
        public int ClassTotalCapacity { get; set; }
        public int ClassOccupiedValue { get; set; }
        public int ClassOccupanyRemaining
        {
            get
            {
                return ClassTotalCapacity - ClassOccupiedValue;
            }
        }
    }
}
