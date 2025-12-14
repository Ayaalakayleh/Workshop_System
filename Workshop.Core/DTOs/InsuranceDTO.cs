namespace Workshop.Core.DTOs.Insurance
{
    public class InsuranceClaimHistory
    {
        public int Id { get; set; }
        public int WorkOrderId { get; set; }
        public int? ClaimType { get; set; }
        public int? DriverFault { get; set; }
        public int? RejectReason { get; set; } //1 driver -- 2 company 
        public int? FullInsuranceStatus { get; set; } //1 Send to insurance workshop -- 2 Compensation 
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Status { get; set; } //1 -- start Calim , 2 Approved,3 need review
        public int FK_AgreementId { get; set; }
        public int InsuranceCompanyId { get; set; }
        public int vehicleId { get; set; }
        public string userName { get; set; }
        public string vehicleName { get; set; }
        public string RefrenceNo { get; set; }
        public string Note { get; set; }
        public string Reason { get; set; }
        public decimal? TotalInsurance { get; set; }

        public bool CDR { set; get; }
        public bool CDW { set; get; }
        public bool TPC { set; get; }
        public string ClaimNumber { get; set; }
        public string ClaimFilePath { get; set; }
        public string ClaimStatusFilePath { get; set; }
        public DateTime? ClaimAmountReceivedDate { get; set; }

        public string TaqdeeratObjectionReason { get; set; }

        public int PathId { get; set; }
        public decimal TaqdeeratPrice { get; set; }
        public decimal WorkShopPrice { get; set; }
        public string TaqdeeratReportFilePath { get; set; }
        public string TaqdeeratFeesFilePath { get; set; }
        public string AdditionalFeesFilePath { get; set; }
        public int BranchId { get; set; }

        public int CompanyId { get; set; }
        public string ClaimHistoryStatusPrimaryName { get; set; }
        public string ClaimHistoryStatusSecondaryName { get; set; }

        public decimal EstimationFees { get; set; }  // رسوم التقدير
        public decimal EstimateAmount { get; set; }  // قيمة التقدير
        public decimal TowingFees { get; set; }  // رسوم السطحة

        public bool IsClientReponsible { get; set; }
        public bool IfCancelled { get; set; }
        public string SecondPartyFaultFilePath { get; set; }
        public string CollectionProofFilePath { get; set; }
        public string FinanceConfirmationFilePath { get; set; }
        public string ClaimAmountFilePath { get; set; }
        public decimal InsurancePricing { get; set; }
        public decimal ExternalWSPrice { get; set; }
        public int ExternalWsId { get; set; }
    }
}
