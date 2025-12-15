using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Workshop.Core.DTOs
{
    public abstract class TechnicianBaseDTO
    {
        public string? PrimaryName { get; set; }
        public string? SecondaryName { get; set; }
        public string? Email { get; set; }
        //public string? Phone { get; set; }
        //public string? PrimaryAddress { get; set; }
        //public string? SecondaryAddress { get; set; }
        //public DateTime? BirthDate { get; set; }
        public int? WorkshopId { get; set; }
        public int? Type { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public List<int>? FK_SkillId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Code { get; set; }
        //public decimal? HourCost { get; set; }
        public int? FordPID { get; set; }
        public int? PIN { get; set; }
        public bool IsResigned { get; set; }
        public DateTime? ResignedDate { get; set; }
        public int? FK_ShiftId { get; set; }
        public List<int>? Teams { get; set; }
    }

    public class TechnicianDTO : TechnicianBaseDTO
    {
        public int Id { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public int? TotalPages { get; set; }
    }

    public class CheckPinRequest
    {
        public TechnicianDTO Technician { get; set; }
        public int PIN { get; set; }
    }

    public class CreateDTechnicianDto : TechnicianBaseDTO
    {
        public int? CreatedBy { get; set; }
    }

    public class UpdateDTechnicianDto : TechnicianBaseDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
        public bool? IsDeleted { get; set; } = false;
    }

    public class DeleteDTechnicianDto
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class FilterTechnicianDTO
    {
        public int? WorkshopId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int? PageNumber { get; set; }
    }
    public class TechnicianAvailabilityDTO
    {
        public int TechnicianId { get; set; }
        public string PrimaryName { get; set; }
        public string? SecondaryName { get; set; }
        public string? Email { get; set; }

        public string? FreeIntervals { get; set; }
        [JsonPropertyName("freeIntervalsList")]
        public List<FreeIntervalDTO> FreeIntervalsList { get; set; } = new();
    }

    public class FreeIntervalDTO
    {
        public string StartFree { get; set; }
        public string EndFree { get; set; }
    }

    public class TechniciansNameDTO
    {

        public int TechId { get; init; }
        public string TechName { get; init; } = string.Empty;
        public string TechSecondaryName { get; init; } = string.Empty;

        public DateTime Date { get; init; }
        public TimeSpan StartTime { get; init; }
        public int Duration { get; init; }
        public TimeSpan EndTime { get; init; }

    }
    public sealed class TechnicianAvailabiltyDTO
    {
        public int TechId { get; set; }
        public string Code { get; set; } = string.Empty;

        public string TechName { get; set; } = string.Empty;
        public string TechSecondaryName { get; set; } = string.Empty;

        public TimeSpan StartWorkingTime { get; set; }
        public TimeSpan EndWorkingTime { get; set; }

        public string WorkingHours { get; set; } = "00:00";
        public string Assigned { get; set; } = "00:00";
        public string Available { get; set; } = "00:00";
    }

}
