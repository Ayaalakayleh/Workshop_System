using Microsoft.AspNetCore.Http;

namespace Workshop.Core.DTOs.Insurance
{
	public class InsuranceClaimHistory
	{
		public int Id { get; set; }
		public int WorkOrderId { get; set; }
		public int VehicleSubStatusId { get; set; }

		public int? ClaimType { get; set; }
		public int? DriverFault { get; set; }
		public int? RejectReason { get; set; }   // 1 driver -- 2 company 
		public int? FullInsuranceStatus { get; set; } // 1 Send to insurance workshop -- 2 Compensation 

		public int CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public int Status { get; set; }          // 1 start Claim, 2 Approved, 3 need review
		public int FK_AgreementId { get; set; }
		public int InsuranceCompanyId { get; set; }
		public int vehicleId { get; set; }

		public string? ClaimHistoryStatusPrimaryName { get; set; }
		public string? ClaimHistoryStatusSecondaryName { get; set; }
		public string? userName { get; set; }
		public string? vehicleName { get; set; }
		public string? RefrenceNo { get; set; }
		public string? Note { get; set; }
		public string? Reason { get; set; }
		public string? ClaimNumber { get; set; }

		public decimal? TotalInsurance { get; set; }

		public bool CDR { get; set; }
		public bool CDW { get; set; }
		public bool TPC { get; set; }

		public string? ClaimFilePath { get; set; }
		public string? ClaimStatusFilePath { get; set; }
		public string? ClaimAmountFilePath { get; set; }

		public DateTime? ClaimAmountReceivedDate { get; set; }

		public string? TaqdeeratObjectionReason { get; set; }

		public int PathId { get; set; }
		public decimal TaqdeeratPrice { get; set; }
		public decimal WSPrice { get; set; }

		public string? TaqdeeratReportFilePath { get; set; }
		public string? TaqdeeratFeesFilePath { get; set; }
		public string? AdditionalFeesFilePath { get; set; }
		public string? CollectionProofFilePath { get; set; }
		public string? SecondPartyFaultFilePath { get; set; }
		public string? FinanceConfirmationFilePath { get; set; }

		public int BranchId { get; set; }
		public int CompanyId { get; set; }
		public decimal EstimationFees { get; set; }  // رسوم التقدير
		public decimal EstimateAmount { get; set; }  // قيمة التقدير
		public decimal TowingFees { get; set; }      // رسوم السطحة

		public bool IsClientReponsible { get; set; }
		public bool IfCancelled { get; set; }

		public decimal InsurancePricing { get; set; }
		public decimal ExternalWSPrice { get; set; }
		public int ExternalWsId { get; set; }

		public int CollectionPathStatusId { get; set; }
		public int RequestMaintinenceStatusId { get; set; }

		public bool ApplyDeductile { get; set; }

		// ↓↓↓ أنواع الملفات في ASP.NET Core ↓↓↓
		public IFormFile? SecondPartyFaultFile { get; set; }
		public IEnumerable<IFormFile>? TaqdeeratFiles { get; set; }
		public IFormFile? AdditionalFeesFile { get; set; }
		public IFormFile? CollectionProofFile { get; set; }
		public IFormFile? TaqdeeratFeesFile { get; set; }
		public IFormFile? FinanceConfirmationFile { get; set; }
		public IFormFile? ClaimAmoountFile { get; set; }
	}

	public class ClaimFileDTO
	{
		public string ClaimFilePath { get; set; }
		public string ClaimStatusFilePath { get; set; }
		public string TaqdeeratFeesFilePath { get; set; }
		public string AdditionalFeesFilePath { get; set; }
		public string SecondPartyFaultFilePath { get; set; }
		public string CollectionProofFilePath { get; set; }
		public string FinanceConfirmationFilePath { get; set; }
		public string ClaimAmountFilePath { get; set; }
	}
	public class InsuranceClaimStatistaic
	{
		public int NewClaim { set; get; }
		public int OpenClaim { set; get; }
		public int TotalClaim { set; get; }
		public int FullInsurance { set; get; }
		public int Compensation { set; get; }
		public int DriverFault { set; get; }
		public int Dropoff { set; get; }
		public int InsurancePolicyExpired { set; get; }
		public int InsuredVehicles { set; get; }
		public int UninsuredVehicles { set; get; }
		public int ByChassisNo { set; get; }
		public int ByPlateNo { set; get; }

	}
	public class WorkOrderClaimEarnings
	{
		public string AccidentNo { set; get; }
		public int AgreementId { set; get; }
		public int DamageId { set; get; }
		public string customerName { set; get; }
		public int vehicleId { set; get; }
		public decimal TotalEarnings { set; get; }
		public int JobCardNo { set; get; }

	}
	public class AmountFromClientsForAccidents
	{
		public int Id { set; get; }
		public int FK_AgreementId { set; get; }

		public decimal InsuranceTypeValue { set; get; }
		public string customerName { set; get; }
	}
	public class WorkOrderInsuranceDetails
	{
		public int Id { get; set; }
		public int VehicleId { get; set; }
		public int CustomerId { get; set; }
		public DateTime OpeningDate { get; set; }
		public int CityId { get; set; }
		public string Region { get; set; }
		public int FK_AgreementId { get; set; }
		public int ContractNo { get; set; }
		public DateTime AccidentDate { get; set; }
		public int ClaimStatus { get; set; }
		public string Status { get; set; }
		public string ClaimNo { get; set; }
		public bool IsFix { get; set; }
		public string IsFixed { get; set; }
		public int WorkOrderTypeId { get; set; }
		public string AccidentType { get; set; }
		public int Liability { get; set; }
		public int InvoicingStatus { get; set; }
		public string InvoicingStatusDes { get; set; }
		public string Workshop { get; set; }
		public int SumInsured { get; set; }
		public DateTime CreatedAt { get; set; }
		public int CreatedById { get; set; }
		public string CreatedBy { get; set; }
		public string Remarks { get; set; }
		public string Notice { get; set; }
		public DateTime ClaimDate { get; set; }
		public DateTime ApprovalDate { get; set; }
		public decimal Labors { get; set; }
		public decimal Parts { get; set; }
		public decimal LossRatio { get; set; }
		public string TypeOfLoss { get; set; }
		public int Total { get; set; }
		public int ActualTotalCost { get; set; }
		public int Sequence { get; set; }
		public int Towing { get; set; }
		public string Chass { get; set; }
		public string Plate { get; set; }
		public string Model { get; set; }
		public string Manufacturer { get; set; }
		public string ManufacturingYear { get; set; }
		public int PolicyNumber { get; set; }
		public string CustomerName { get; set; }
		public string DriverName { get; set; }
		public string IdNumber { get; set; }
		public int Age { get; set; }
		public string Mobile { get; set; }
		public decimal Excess { get; set; }
	}

}
