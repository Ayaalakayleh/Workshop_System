namespace Workshop.Web.Models
{
    public class PettyCashExpenses
    {
        public int Id { get; set; }
        public Int64 RequestNo { get; set; }
        public int FK_TypeOfExpense { get; set; }
        public int FK_ExpenseType { get; set; }
        public int FK_EmployeeId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int FK_CurrencyId { get; set; }
        public int FK_SellerId { get; set; }
        public decimal NetAmount { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
        public int FK_VehicleId { get; set; }
        public int KM { get; set; }
        public int LastKM { get; set; }
        public int CreatedBy { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int TaxClassificationId { get; set; }
    }
}
