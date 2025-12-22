using Microsoft.AspNetCore.Http;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Infrastructure;

namespace Workshop.Core.DTOs
{

	public enum JobCardType { Accident = 1, Maintenance = 2 }
	public enum InvoicingStatus { HasNoInvoice = 1, HasInvoice = 2 }
	public enum JobCardStatus { Open = 1, Pending = 2, UnderRepair = 3, Closed = 4, Cancelled = 5 }
	public enum WorkOrderStatus { Open = 1, InProgress = 2, UnderRepair = 3, Closed = 4, Cancelled = 5 }
	public enum VehicleTypeId { Internal = 1, External = 2 }

	public enum WorkOrderType { }
	public class MWorkOrderDTO
	{
		public int Id { get; set; }
		public int WorkOrderType { get; set; }
		public string? WorkOrderTitle { get; set; }
		public int VehicleId { get; set; }
		public int ReportType { get; set; }
		public string? AccidentPlace { get; set; }
		public int? DriverFaultInPercent { get; set; }
		public string? Description { get; set; }
		public string? ImagesFilePath { get; set; }
		public string? Note { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public int? ModifyBy { get; set; }
		public DateTime? ModifyAt { get; set; }
		public DateTime? GregorianDamageDate { get; set; }
		public DateOnly? HijriDamagetDate { get; set; }
		public int CompanyId { get; set; }
		public int BranchId { get; set; }
		public bool IsFix { get; set; }
		public int FkAgreementId { get; set; }
		public int FkVehicleMovementId { get; set; }
		public string? AgreementName { get; set; }
		public string? VehicleMovementName { get; set; }
		public int RelatedId { get; set; }
		public decimal? EstimatedAmount { get; set; }
		public decimal TotalCost { get; set; }
		public TimeSpan AccidentTime { get; set; }
		public DateTime? AccidentDate { get; set; }
		public string? ReportNo { get; set; }
		public string? AccidentNo { get; set; }
		public bool? IsInsuraceDamege { get; set; }
		public int ClaimStatus { get; set; }
		public string? RefrenceNo { get; set; }
		public decimal TotalInsurance { get; set; }
		public int? InsuranceCompanyId { get; set; }
		public int? SecondPartyFaultInPercent { get; set; }
		public bool ThereIsAsecondParty { get; set; }
		public int? ClaimType { get; set; }
		public int? RejectReason { get; set; }
		public string? Reason { get; set; }
		public int? FullInsuranceStatus { get; set; }
		public decimal? ReceivedKm { get; set; }
		public int? WorkOrderNo { get; set; }
		public bool IsExternal { get; set; }
		public int? WorkOrderStatus { get; set; }
		public string? WorkOrderStatusName { get; set; }
		public int? TotalRows { get; set; }
		public int? TotalPages { get; set; }
		public int InvoicingStatus { get; set; }
		public Guid? MasterId { get; set; }
		public int Wfstatus { get; set; }
		public bool IsFinished { get; set; }
		public string? ClaimFilePath { get; set; }
		public string? CliamNumber { get; set; }
		public string? ClaimStatusFilePath { get; set; }
		public string? InsuranceWorkshop { get; set; }
		public string? InsuranceWorkshopConcernedPerson { get; set; }
		public string? card { get; set; }
		public DateTime? ClaimAmountReceivedDate { get; set; }
		public decimal ActualTotalCost { get; set; }
		public int? ProjectId { get; set; }
		public int? CustomerId { get; set; }
		public int? CityId { get; set; }
		public int? WorkshopId { get; set; }
		public decimal TaqdeeratPrice { get; set; }
		public string? TaqdeeratReportFilePath { get; set; }
		public int CollectionPathStatusId { get; set; }
		public int RequestMaintinenceStatusId { get; set; }
		public int? ThitdPathStatusId { get; set; }
		public decimal? Wsprice { get; set; }
		public string? TaqdeeratPriceRejectReason { get; set; }
		public string? TaqdeeratFeesFilePath { get; set; }
		public string? AdditionalFeesFilePath { get; set; }
		public decimal EstimateAmount { get; set; }
		public decimal EstimationFees { get; set; }
		public decimal TowingFees { get; set; }
		public bool HasInsurance { get; set; }
		public bool NeedsInsuranceApproval { get; set; }
		public bool? IsClientResponsible { get; set; }
		public string? SecondPartyFaultFilePath { get; set; }
		public string? CollectionProofFilePath { get; set; }
		public decimal ExternalWsprice { get; set; }
		public int? ExternalWsId { get; set; }
		public decimal InsurancePricing { get; set; }
		public string? FinanceConfirmationFilePath { get; set; }
		public string? ClaimAmoountFilePath { get; set; }
		public bool HasEngineDamage { get; set; }
		public bool HasTransmissionDamage { get; set; }
		public bool HasChassisDamage { get; set; }
		public bool CanRepaired { get; set; }
		public string? VehicleName { get; set; }
		public int VehicleType { get; set; }
		public string? FileName { get; set; }
		public string? CollectionPathPrimaryName { get; set; }
		public string? CollectionPathSecondaryName { get; set; }
		public string? RequestMaintenancePrimaryName { get; set; }
		public string? RequestMaintenanceSecondaryName { get; set; }
		// Navigation properties as DTOs
		public List<DMaintenanceCardDTO>? DMaintenanceCards { get; set; } = new List<DMaintenanceCardDTO>();
		public List<DWorkOrderReportDTO>? DWorkOrderReports { get; set; } = new List<DWorkOrderReportDTO>();
		public List<MTaqdeeratDocumentDTO>? MTaqdeeratDocuments { get; set; } = new List<MTaqdeeratDocumentDTO>();
		public List<MWorkOrderDetailDTO>? MWorkOrderDetails { get; set; } = new List<MWorkOrderDetailDTO>();
		public List<InsuranceClaimHistory>? claimHistories { set; get; } = new List<InsuranceClaimHistory>();
	}

	public class DMaintenanceCardDTO
	{
		public int Id { get; set; }
		public int? MovementId { get; set; }
		public int? WorkOrderId { get; set; }
		public string? Description { get; set; }
		public bool? Status { get; set; }
		public int? ServiceId { get; set; }

		// Navigation property as DTO
		public MWorkOrderDTO? WorkOrder { get; set; }
	}

	public class DWorkOrderReportDTO
	{
		public int Id { get; set; }
		public int WorkOrderId { get; set; }
		public string? FilePath { get; set; }
		public string? FileName { get; set; }

		// Navigation property as DTO
		public MWorkOrderDTO WorkOrder { get; set; } = null!;
	}

	public class MTaqdeeratDocumentDTO
	{
		public int Id { get; set; }
		public int? WorkOrderId { get; set; }
		public string? FilePath { get; set; }
		public string? TaqFileName { get; set; }
		public int? CreatedBy { get; set; }
		public DateTime? CreatedDate { get; set; }
		public bool? IsDeleted { get; set; }
	}

	public class MWorkOrderDetailDTO
	{
		public int Id { get; set; }
		public string? PartName { get; set; }
		public int? WorkOrderType { get; set; }
		public int? WorkOrderId { get; set; }
		public string? Note { get; set; }
		public bool? IsFix { get; set; }
		public virtual ICollection<MWorkOrdersDetailsDocument> MWorkOrdersDetailsDocuments { get; set; } = new List<MWorkOrdersDetailsDocument>();

	}

	public class MWorkOrdersDetailsDocumentDTO
	{
		public int? WorkOrderListId { get; set; }
		public string? FilePath { get; set; }
		public string? FileName { get; set; }

	}

	public class WorkOrderFilterDTO
	{
		public DateTime? FromDate { set; get; }
		public DateTime? ToDate { set; get; }
		public int? VehicleID { get; set; }
		public int? WorkOrderTypeId { get; set; }
		public List<VehicleNams>? VehicleNams { get; set; }
		public List<VehicleNams>? ExternalVehicleNams { get; set; }
		public int? page { get; set; }
		public int? TotalPages { get; set; }
		public int? CompanyId { get; set; }
		public int? BranchId { get; set; }
		public bool? IsInsuraceDamege { get; set; }
		public string? language { get; set; }
		public int? Type { get; set; }
		public int? Id { get; set; }
		public bool? IsExternal { get; set; }
		public int? ExternalVehicleID { get; set; }
		public int? CreatedBy { get; set; }
		public List<UserDTO>? Users { get; set; }
		public int? WorkOrderStatus { get; set; }
		public int? InvoicingStatus { get; set; }
		public int? WorkOrderNo { get; set; }

	}

	public class WorkshopWorkOrderStatusReportDTO
	{
		public int VehicleID { set; get; }
		public string WorkOrderTitle { set; get; }
		public string Vehicle { set; get; }
		public DateTime GregorianMovementDate { set; get; }
		public bool isRegularMaintenance { set; get; }
		public bool status { set; get; }
		public string MaintenanceType { set; get; }
		public bool IsExternal { set; get; }

	}
}