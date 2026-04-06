using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportsService.Models
{
    public class PartsSummaryReportModel
    {
        public IEnumerable<PartsSummaryModel> data { get; set; } 
        public string CompanyName { get; set; }
        public string Lang { get; set; }

    }

    public class PartsSummaryModel
    {
       public int? AccountType { get; set; }
        public int? SalesType { get; set; }
        public string AccountTypePrimaryName { get; set; } = null;
        public string AccountTypeSecondaryName { get; set; } = null;
        public string SalesTypePrimaryName { get; set; } = null;
        public string SalesTypeSecondaryName { get; set; } = null;
        public int? RetailValue { get; set; } //Total
        public int? SaleValue { get; set; } //Sale price
        public decimal? Discount { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? CostValue { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Profit { get; set; }

   

    }
    
}