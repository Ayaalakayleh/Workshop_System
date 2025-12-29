namespace Workshop.Core.DTOs
{
	public abstract class RecallBaseDTO
	{
		public string? Code { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool IsActive { get; set; }
		public List<VehicleRecallDTO>? Vehicles { get; set; }
	}

	public class RecallDTO : RecallBaseDTO
	{
		public int Id { get; set; }
		public DateTime? CreatedAt { get; set; }
		public int? CreatedBy { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int? UpdatedBy { get; set; }
	}
	public class CreateRecallDTO : RecallBaseDTO
	{
		public int? CreatedBy { get; set; }
	}

	public class UpdateRecallDTO : RecallBaseDTO
	{
		public int Id { get; set; }
		public int? UpdatedBy { get; set; }
	}
	public class DeleteRecallDTO
	{
		public int Id { get; set; }
		public int? UpdatedBy { get; set; }
		public bool IsActive { get; set; }
	}


	public class FilterRecallDTO
	{
		public string? Tittle { get; set; }
		public string? Code { get; set; }
		public int? PageNumber { get; set; }
	}

	public class VehicleRecallDTO
	{
		public int? Id { get; set; }
		public int? RecallID { get; set; }
		public int? MakeID { get; set; }
		public int? ModelID { get; set; }
		public string? Chassis { get; set; }
		public int? RecallStatus { get; set; }
	}

	public class ActiveRecallDto
	{
		public int RecallId { get; set; }
		public string Code { get; set; } = default!;
		public string Title { get; set; } = default!;
		public string? Description { get; set; }

		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }

		public bool IsActive { get; set; }

		public DateTime? CreatedAt { get; set; }
		public int? CreatedBy { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int? UpdatedBy { get; set; }
	}

	public class ActiveRecallsByChassisResponseDto
	{
		public string ChassisNo { get; set; } = default!;
		public bool HasActiveRecall => Recalls != null && Recalls.Count > 0;
		public List<ActiveRecallDto> Recalls { get; set; } = new();
	}
	public enum VehicleRecallStatus {
		Open = 1, Done = 2
    }
}
