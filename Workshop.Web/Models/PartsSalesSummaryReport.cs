namespace Workshop.Web.Models
{
    public class PartsSalesSummaryReport
    {
        public int? AccountType { get; set; }    
        public int? SaleType { get; set; }         

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
