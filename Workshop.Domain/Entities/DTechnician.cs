
namespace Workshop.Infrastructure;

public partial class DTechnician
{
    public int Id { get; set; }

    public string? PrimaryName { get; set; }

    public string? SecondaryName { get; set; }

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? PrimaryAddress { get; set; }

    public string? SecondaryAddress { get; set; }

    public DateTime? BirthDate { get; set; }

    public int? UserId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? WorkshopId { get; set; }

    public bool? IsDeleted { get; set; }

    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public int? FK_SkillId { get; set; }

    public virtual ICollection<DJobCardLabor> DJobCardLabors { get; set; } = new List<DJobCardLabor>();
}
