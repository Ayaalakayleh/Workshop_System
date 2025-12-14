using System;
using System.Collections.Generic;

namespace Workshop.Core.DTOs
{
    public abstract class TechnicianWorkScheduleBaseDTO
    {
        public int FK_TechnicianId { get; set; }
        public string TechnicianIds { get; set; }
        public int? WorkshopId { get; set; }
        public DateTime? WorkingDateFrom { get; set; }
        public DateTime? WorkingDateTo { get; set; }
        public TimeSpan? WorkingTimeFrom { get; set; }
        public TimeSpan? WorkingTimeTo { get; set; }
        public int? FK_SlotMinutes_Id { get; set; }
    }

    public class TechnicianWorkScheduleDTO : TechnicianWorkScheduleBaseDTO
    {
        public int Id { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string Name { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int TotalPages { get; set; }
        public string WorkingTimes { get; set; }

        public List<int> TechnicianIds_List { get; set; } = new List<int>();
    }

    public class CreateTechnicianWorkScheduleDTO : TechnicianWorkScheduleBaseDTO
    {
        public int? CreatedBy { get; set; }
    }

    public class UpdateTechnicianWorkScheduleDTO : TechnicianWorkScheduleBaseDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class DeleteTechnicianWorkScheduleDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; } = true;
    }
    public class FilterTechnicianWorkScheduleDTO
    {
        public int? WorkshopId { get; set; }
        public string? Name { get; set; }
        public DateTime? Date { get; set; }
        public string? lang { get; set; }
        public int? PageNumber { get; set; }
    }
}
