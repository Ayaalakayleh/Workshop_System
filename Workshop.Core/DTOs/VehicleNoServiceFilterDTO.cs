using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class VehicleNoServiceFilterDTO
    {
        public string VehicleIds { get; set; } = "";

        public int BranchId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
