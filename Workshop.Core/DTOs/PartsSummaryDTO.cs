using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class PartsSummaryDTO
    {
        public int? AccountType { get; set; }
        public int? SalesType { get; set; }
        public string? AccountTypePrimaryName { get; set; }
        public string? AccountTypeSecondaryName { get; set; }
        public string? SalesTypePrimaryName { get; set; }
        public string? SalesTypeSecondaryName { get; set; }
        public int? RetailValue { get; set; } //Total
        public int? SaleValue { get; set; } //Sale price
        public decimal? Discount { get; set; } 
        public decimal? DiscountPercentage { get; set; }
        public decimal? CostValue { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Profit { get; set; }
    }
    public class PartsSummaryFilterDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? AccountType { get; set; }
        public int? SalesType { get; set; }
        public bool IsAll { get; set; } = false;
        public bool IsWIPClose { get; set; } = false;

    }
}
