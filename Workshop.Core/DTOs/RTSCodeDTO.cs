using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using Workshop.Core.DTOs.Vehicle;

namespace Workshop.Core.DTOs
{
    public abstract class RTSCodeBaseDTO
    {
	
		public string? Code { get; set; }
		public string? PrimaryDescription { get; set; } 
		public string? SecondaryDescription { get; set; } 
		public int? FK_CategoryId { get; set; }
		public decimal? StandardHours { get; set; }
		public int? FK_SkillId { get; set; }
		public int? CompanyId { get; set; }
		public bool IsActive { get; set; }
		public DateTime? EffectiveDate { get; set; }
        public string? Notes { get; set; }

		public List<int>? FranchiseIds { get; set; } 
		public List<int>? VehicleClassIds { get; set; } 
		public decimal? Hours { get; set; }
		public decimal? Price { get; set; }
        public decimal? DefaultRate { get; set; } = 0;
        public string? PrimaryName { get; set; }
        public string? SecondaryName { get; set; }

    }

	public class RTSCodeDTO : RTSCodeBaseDTO
	{
		public int Id { get; set; }
        public int? TotalPages { get; set; }
		public DateTime? CreatedAt { get; set; }
		public int? CreatedBy { get; set; }
	}

    public class CreateRTSCodeDTO : RTSCodeBaseDTO
    {
        public int? CreatedBy { get; set; }
	}

    public class UpdateRTSCodeDTO : RTSCodeBaseDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class DeleteRTSCodeDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
    }

    public class FilterRTSCodeDTO
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? PageNumber { get; set; }
    }
}
