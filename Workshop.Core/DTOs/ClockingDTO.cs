using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{

    public enum Status { None = 0, Working = 1, Break = 2, ClockOut = 3 }
    public class ClockingDTO
    {
        public int? ID { get; set; }
        public int? TechnicianID { get; set; }
        public int? WIPID { get; set; }
        public int? RTSID { get; set; }
        public int? StatusID { get; set; }
        public Status? StatusName { get; set; }
        public string? TechnicianName { get; set; }
        public string? WIPName { get; set; }
        public string? RTSName { get; set; }
        public TimeSpan? Elapsed { get; set; }
        public Decimal? AllowedTime { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? Breaks { get; set; } = 0;
        public ClockingBreakDTO? LastBreak { get; set; }
        public List<ClockingBreakDTO>? ClockingBreaksLogs { get; set; }
        public ShiftDTO? TechnicianDefaultShift { get; set; }
        public TechnicianDTO? Technician { get; set; }
    }

    public class ClockingBreakDTO
    {
        public int? ID { get; set; }
        public int? ClockingID { get; set; }
        public string? Note { get; set; }
        public string? Hint { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public int? Reason { get; set; }
        public string? ReasonString { get; set; }
    }

    public class ClockingFilterDTO
    {
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }


    public class GetClockingFilter
    {
        public int? WIPId { get; set; }
        public int? TechnicianId { get; set; }
        public int? RTSId { get; set; }
        public int? KeyId { get; set; }
        public string? PrimaryTechniciansName { get; set; }
        public string? SecondaryTechniciansName { get; set; }
        public string? RTSPrimaryName { get; set; }
        public string? RTSSecondaryName { get; set; }
        public string? RTSPrimaryDescription { get; set; }
        public string? RTSSecondaryDescription { get; set; }
        public decimal? StandardHours { get; set; }
    }

}