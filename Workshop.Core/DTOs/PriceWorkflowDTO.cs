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
}
