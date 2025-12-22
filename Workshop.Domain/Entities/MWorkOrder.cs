using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MWorkOrder
{
	public int Id { get; set; }

	public int? WorkOrderType { get; set; }

	public string? WorkOrderTitle { get; set; }

	public int VehicleId { get; set; }

	public int ReportType { get; set; }

	public string? AccidentPlace { get; set; }

	public int? DriverFaultInPercent { get; set; }

	public string? Description { get; set; }

	public string? ImagesFilePath { get; set; }

	public string? Note { get; set; }

	public int? CreatedBy { get; set; }

	public DateTime? CreatedAt { get; set; }

	public int? ModifyBy { get; set; }

	public DateTime? ModifyAt { get; set; }

	public DateTime? GregorianDamageDate { get; set; }

	public DateOnly? HijriDamagetDate { get; set; }

	public int? CompanyId { get; set; }

	public int? BranchId { get; set; }

	public bool? IsFix { get; set; }

	public int? FkAgreementId { get; set; }

	public int? FkVehicleMovementId { get; set; }

	public int? RelatedId { get; set; }

	public decimal? EstimatedAmount { get; set; }

	public decimal? TotalCost { get; set; }

	public TimeOnly? AccidentTime { get; set; }

	public string? ReportNo { get; set; }

	public string? AccidentNo { get; set; }

	public bool? IsInsuraceDamege { get; set; }

	public int? ClaimStatus { get; set; }

	public string? RefrenceNo { get; set; }

	public decimal? TotalInsurance { get; set; }

	public int? InsuranceCompanyId { get; set; }

	public int? SecondPartyFaultInPercent { get; set; }

	public bool? ThereIsAsecondParty { get; set; }

	public int? ClaimType { get; set; }

	public int? RejectReason { get; set; }

	public string? Reason { get; set; }

	public int? FullInsuranceStatus { get; set; }

	public decimal? ReceivedKm { get; set; }

	public int? WorkOrderNo { get; set; }

	public bool IsExternal { get; set; }

	public int WorkOrderStatus { get; set; }

	public int InvoicingStatus { get; set; }

	public Guid? MasterId { get; set; }

	public int Wfstatus { get; set; }

	public bool IsFinished { get; set; }

	public string? ClaimFilePath { get; set; }

	public string? CliamNumber { get; set; }

	public string? ClaimStatusFilePath { get; set; }

	public string? InsuranceWorkshop { get; set; }

	public string? InsuranceWorkshopConcernedPerson { get; set; }

	public DateTime? ClaimAmountReceivedDate { get; set; }

	public decimal? ActualTotalCost { get; set; }

	public int? ProjectId { get; set; }

	public int? CustomerId { get; set; }

	public int? CityId { get; set; }

	public int? WorkshopId { get; set; }

	public decimal? TaqdeeratPrice { get; set; }

	public string? TaqdeeratReportFilePath { get; set; }

	public int? CollectionPathStatusId { get; set; }

	public int? RequestMaintinenceStatusId { get; set; }

	public int? ThitdPathStatusId { get; set; }

	public decimal? Wsprice { get; set; }

	public string? TaqdeeratPriceRejectReason { get; set; }

	public string? TaqdeeratFeesFilePath { get; set; }

	public string? AdditionalFeesFilePath { get; set; }

	public decimal? EstimateAmount { get; set; }

	public decimal? EstimationFees { get; set; }

	public decimal? TowingFees { get; set; }

	public bool? HasInsurance { get; set; }

	public bool? NeedsInsuranceApproval { get; set; }

	public bool? IsClientResponsible { get; set; }

	public string? SecondPartyFaultFilePath { get; set; }

	public string? CollectionProofFilePath { get; set; }

	public decimal? ExternalWsprice { get; set; }

	public int? ExternalWsId { get; set; }

	public decimal? InsurancePricing { get; set; }

	public string? FinanceConfirmationFilePath { get; set; }

	public string? ClaimAmoountFilePath { get; set; }

	public bool? HasEngineDamage { get; set; }

	public bool? HasTransmissionDamage { get; set; }

	public bool? HasChassisDamage { get; set; }

	public bool? CanRepaired { get; set; }

	public virtual ICollection<DMaintemanceCard> DMaintemanceCards { get; set; } = new List<DMaintemanceCard>();

	public virtual ICollection<DWorkOrderReport> DWorkOrderReports { get; set; } = new List<DWorkOrderReport>();

	public virtual ICollection<MTaqdeeratDocument> MTaqdeeratDocuments { get; set; } = new List<MTaqdeeratDocument>();

	public virtual ICollection<MWorkOrderDetail> MWorkOrderDetails { get; set; } = new List<MWorkOrderDetail>();
}
public class WorkOrderStatusDTO
{
	public int Id { get; set; }

	public string PrimaryName { get; set; }
	public string SecondaryName { get; set; }

	public int StatusValue { get; set; }

	public int HeaderId { get; set; }
	public int RoleId { get; set; }


}
public class StatusRoleViewModel
{
	public int StatusId { get; set; }
	public int RoleId { get; set; }
}