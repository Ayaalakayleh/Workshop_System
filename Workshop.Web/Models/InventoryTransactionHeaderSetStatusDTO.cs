namespace Workshop.Web.Models
{
    public class InventoryTransactionHeaderSetStatusDTO
    {
        public long HeaderId { get; set; }
        public int NewStatusId { get; set; }
        public int? ModifiedBy { get; set; }
    }
}
