using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class UpdateSinglePartStatusDTO
    {
        public int WIPId { get; set; }
        public int ItemId { get; set; }
        public int? LocatorId { get; set; }
        public int StatusId { get; set; }
    }

}
