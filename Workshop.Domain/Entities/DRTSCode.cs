namespace Workshop.Infrastructure;

public partial class DRTSCode
{
	public int Id { get; set; }

	public string? Code { get; set; }

	public string? PrimaryDescription { get; set; }

	public string? SecondaryDescription { get; set; }

	public int? FK_CategoryId { get; set; }

	public int? FK_SkillId { get; set; }

	public int? CompanyId { get; set; }

	public decimal? StandardHours { get; set; }

	public bool? IsActive { get; set; }

	public DateTime? EffectiveDate { get; set; }

	public string? Notes { get; set; }

	public int? CreatedBy { get; set; }

	public DateTime? CreatedAt { get; set; }

	public int? UpdatedBy { get; set; }

	public DateTime? UpdatedAt { get; set; }

	
}