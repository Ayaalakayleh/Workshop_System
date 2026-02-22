using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class PriceWorkflowDTO
    {   
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string? KeyId { get; set; }
        public int WorkflowID { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class WipItemPriceWorkflowDTO
    {
        public int Id { get; set; }
        public int WipId { get; set; }
        public int? PriceWorkflowEnumId { get; set; }
        public Guid? PriceWorkflowMasterId { get; set; }
        public bool RequiresPriceApproval { get; set; }
    }

    public class ApplyWipPriceWorkflowRequest
    {
        public int WipId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        //public List<ApplyWipPriceWorkflowLine> Lines { get; set; } = new();
        public string GroupIdsCsv { get; set; } = ""; 
    }

    public class ApplyWipPriceWorkflowLine
    {
        public int WipItemId { get; set; }   
        public decimal Price { get; set; }   
        public string PriceKeyId { get; set; } = "SalePrice";
    }
    public class ApplyWipPriceWorkflowResult
    {
        public int WipItemId { get; set; }
        public bool Created { get; set; }
        public Guid? MasterId { get; set; }
        public int? WorkflowEnumId { get; set; }
        public string? Error { get; set; }
        public int UserId { get; set; }
    }

    public class FinishWipPriceWorkflowRequest
    {
        public int WipItemId { get; set; }
        public Guid? MasterId { get; set; }
        public int Status { get; set; } // 2 Approved, 3 Rejected, 4 Failed
        public string? Reason { get; set; }
        public int UserId { get; set; }
    }
}
