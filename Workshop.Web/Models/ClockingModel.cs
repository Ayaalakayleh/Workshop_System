using Microsoft.AspNetCore.Mvc.Rendering;
using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    
    public class ClockingModel
    {
        public IEnumerable<TechnicianDTO>? Technicians { get; set; }

        public IEnumerable<WIPDTO>? WIPS { get; set; }

        public IEnumerable<CreateWIPServiceDTO>? Labourlines { get; set; }
        public IEnumerable<RTSCodeDTO>? RTSCodes { get; set; }

        public List<SelectListItem>? TechniciansSelectList { get; set; }

        public List<SelectListItem>? WIPSSelectList { get; set; }

        public List<SelectListItem>? LabourlinesSelectList { get; set; }

        public ClockingDTO? ClockingForm { get; set; }
        public List<ClockingDTO>? ClockingList { get; set; }

        public List<ClockingDTO>? ClockingHistory { get; set; }
        public ClockingBreakDTO? ClockingBreakForm { get; set; }
        public IEnumerable<LookupDetailsDTO>? Reasons { get; set; }
        public int? LogIndex { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}
