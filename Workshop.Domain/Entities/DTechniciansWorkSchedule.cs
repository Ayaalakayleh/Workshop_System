
namespace Workshop.Infrastructure;

public partial class DTechniciansWorkSchedule
{
    public int Id { get; set; }
    public int FK_TechnicianId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? WorkshopId { get; set; }

    public bool? IsDeleted { get; set; }
    public DateTime? WorkingDateFrom { get; set; }
    public DateTime? WorkingDateTo { get; set; }
    public TimeSpan? WorkingTimeFrom { get; set; }
    public TimeSpan? WorkingTimeTo { get; set; }
    public int? FK_SlotMinutes_Id { get; set; }


    public virtual ICollection<DJobCardLabor> DJobCardLabors { get; set; } = new List<DJobCardLabor>();
}
