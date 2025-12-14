using System.ComponentModel.DataAnnotations;
using Workshop.Core.Helpers;

namespace Workshop.Core.DTOs
{
    public abstract class BaseShiftDTO
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [StringLength(100)]
        public string PrimaryName { get; set; }

        [Required]
        [StringLength(100)]
        public string SecondaryName { get; set; }

        [Required]
        [StringLength(20)]
        public string Short { get; set; }

        [StringLength(20)]
        public string Color { get; set; }

        public bool SpanMidnight { get; set; }
        public bool Sun { get; set; }
        public bool Mon { get; set; }
        public bool Tue { get; set; }
        public bool Wed { get; set; }
        public bool Thu { get; set; }
        public bool Fri { get; set; }
        public bool Sat { get; set; }
        public TimeSpan? WorkingFromTime { get; set; }
        public TimeSpan? WorkingToTime { get; set; }
        public TimeSpan? BreakFromTime { get; set; }
        public TimeSpan? BreakToTime { get; set; }

        [StringLength(30)]
        public string DOW { get; set; }

        [Required]
        public int CompanyId { get; set; }
    }

    public class ShiftDTO : BaseShiftDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public TimeSpan? StartBreakTime { get; set; }
        public TimeSpan? EndBreakTime { get; set; }

        public void UpdateDOW()
        {
            DOW = DatesHelper.ConvertToDOW(Sun, Mon, Tue, Wed, Thu, Fri, Sat);
        }

        public string DOWVisualRepresentation()
        {
            return DatesHelper.ToVisualRepresentation(Sun, Mon, Tue, Wed, Thu, Fri, Sat);
        }
    }

    public class ShiftListItemDTO : ShiftDTO
    {
        public int TotalCount { get; set; }
    }

    public class CreateShiftDTO : BaseShiftDTO
    {
        [Required]
        public int CreatedBy { get; set; }
    }

    public class UpdateShiftDTO : BaseShiftDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int UpdatedBy { get; set; }
    }

    public class ShiftFilterDTO
    {
        public int CompanyId { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 25;
    }

}
