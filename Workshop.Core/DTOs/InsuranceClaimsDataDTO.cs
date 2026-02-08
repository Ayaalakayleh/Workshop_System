using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class InsuranceClaimsDataDTO
    {
        public int JobCardNo { get; set; }
        public DateTime? IntimationDate { get; set; }
        public int BranchId { get; set; }
        public string AccidentPlace { get; set; }
        public DateTime AccidentDate { get; set; }
        public int WorkshopId { get; set; }
        public DateTime SentForApprovalDate { get; set; }
        public DateTime ApprovalDate { get; set; }
        public string IncidentNo { get; set; }
        public int ReportType { get; set; }
        public int InvLaborCharge { get; set; }
        public int InvPartsCharge { get; set; }
        public int InvOtherCharge { get; set; }
        public decimal ExternalWSPrice { get; set; }
        public decimal ExternalWSPriceParts { get; set; }
        public decimal TowingFees { get; set; }
        public int VAT { get; set; }
        public string CliamNumber { get; set; }
        public int ClaimStatus { get; set; }
        public int TotalLoss { get; set; }
        public int PolicyDeduction { get; set; }
        public int DepMissingParts { get; set; }
        public int ReceivedRecovery { get; set; }
        public int Salvage { get; set; }
        public int NetClaim { get; set; }
        public int WipNo { get; set; }
        public int AccountNumber { get; set; }
        public int InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? DriverName { get; set; }
        public int CompanyId { get; set; }
        public int OriBRN { get; set; }
        public int CNDate { get; set; }
        public int CreditNoteAmount { get; set; }
        public int CnDocNo { get; set; }
        public int BatchNumber { get; set; }
        public int OriginalReceivedT { get; set; }
        public int ApprovedAmount { get; set; }
        public int DriverFaultInPercent { get; set; }
        public int SecondPartyFaultInPercent { get; set; }



    }
}
