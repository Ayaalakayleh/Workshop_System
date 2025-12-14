using System.ComponentModel.DataAnnotations;

namespace Workshop.Core.DTOs
{
    public abstract class BaseAllowedTimeDTO
    {

        public int Make { get; set; }


        public int Model { get; set; }


       // [Range(1900, 2100)]
        public int Year { get; set; }


        public int RTSCode { get; set; }


       // [Range(0, int.MaxValue)]
        public decimal AllowedHours { get; set; }

        [StringLength(60)]
        public string? SupervisorOverride { get; set; }


        public int? VehicleClass { get; set; }
    }

    public class AllowedTimeDTO : BaseAllowedTimeDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class AllowedTimeListItemDTO : AllowedTimeDTO
    {
        public int TotalCount { get; set; }
        public string? MakeName { get; set; }
        public string? ModelName { get; set; }
        public string? RTSCode_Code { get; set; }
    }

    public class CreateAllowedTimeDTO : BaseAllowedTimeDTO
    {
        [Required]
        public int CreatedBy { get; set; }
    }

    public class UpdateAllowedTimeDTO : BaseAllowedTimeDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int UpdatedBy { get; set; }
    }

    public class AllowedTimeFilterDTO
    {
        public int? Make { get; set; }
        public int? Model { get; set; }
        public int? Year { get; set; }
        public string? Engine { get; set; }
        public int? RTSCode { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 25;
    }
}