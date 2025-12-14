using System;

namespace Workshop.Core.DTOs
{
    public abstract class LabourRateBaseDTO
    {
        public string LabourCode { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
    }

    public class LabourRateDTO : LabourRateBaseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? TotalPages { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class CreateLabourRateDTO : LabourRateBaseDTO
    {
        public int? CreatedBy { get; set; }
    }

    public class UpdateLabourRateDTO : LabourRateBaseDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class DeleteLabourRateDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class FilterLabourRateDTO
    {
        public string? Name { get; set; }
        public string? lang { get; set; }
        public int? PageNumber { get; set; }
    }

    //=================================================================================
    //public class GetLabourRateDTO
    //{
    //    public int Rate { get; set; }
    //    public string Source { get; set; }

    //}

    public class LabourRateFilterDTO
    {
        public int? TechnicianId { get; set; }
        public int? AccountType { get; set; }
        public int? SalesType { get; set; }
        public int? CustomerId { get; set; }
        public int? WIPId { get; set; }
        public int? RTSId { get; set; }
        public int? SkillId { get; set; }
        public List<int>? Skills { get; set; }
        public int? Make { get; set; }

    }
}
