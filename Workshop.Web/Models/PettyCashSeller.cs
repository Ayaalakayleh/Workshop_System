namespace Workshop.Web.Models
{
    public class PettyCashSeller
    {
        public int Id { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string TaxNumber { get; set; }
        public int CompanyId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }

    }
}
