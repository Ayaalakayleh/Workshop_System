namespace Workshop.Web.Models
{
    public class PettyCashRequest
    {
        public int Id { get; set; }
        public Int64 RequestNo { get; set; }
        public decimal RequestAmount { get; set; }
        public int FK_EmployeeId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
