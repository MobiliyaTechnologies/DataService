using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels
{
    public class PIConfigurationInfoModel
    {
        public int PiServerID { get; set; }
        public string PiServerName { get; set; }
        public string PiServerDesc { get; set; }
        public string PiServerURL { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
        public double UTCConversionTime { get; set;}
    }
}
