namespace Workshop.Web.Models
{
    public class MovementOperatorsViewModel
    {
        public string DueInDate { get; set; }
        public decimal? ReceivedMeter { get; set; }
        public string CreatingOperator { get; set; }
        public string DueOutDate { get; set; }
        public string BookedOutOperator { get; set; }
        public string InvoicingOperator { get; set; }
    }
}
