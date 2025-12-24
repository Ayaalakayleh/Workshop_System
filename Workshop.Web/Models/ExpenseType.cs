using Workshop.Core.DTOs.AccountingDTOs;

namespace Workshop.Web.Models
{
    public class ExpenseType
    {
        public int Id { get; set; }
        public int ExpenseNumber { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int FK_AccountingId { get; set; }
        public int FK_TypeOfExpenseId { get; set; }
        public List<AccountTable> oLAccountTable { get; set; }
        public List<LKP_TypeOfExpense> oLKP_TypeOfExpense { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public int CompanyId { get; set; }
    }

    public class LKP_TypeOfExpense
    {
        public int Id { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public string ModifiedDate { get; set; }
        public int CompanyId { get; set; }
    }
}
