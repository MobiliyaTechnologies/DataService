using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProcessor.DataModels.ApiModels
{
    public class PIConfigurationRequestModel
    {
        public string PiServerName { get; set; }
    }

    public class PIConfigurationResponseModel
    {
        public int PiServerID { get; set; }
        public string PiServerName { get; set; }
        public string PiServerDesc { get; set; }
        public int CampusID { get; set; }
        public string PiServerURL { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public bool IsDeleted { get; set; }
    }
}
