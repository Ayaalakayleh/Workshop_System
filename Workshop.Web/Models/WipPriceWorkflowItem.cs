namespace Workshop.Web.Models
{
    public class WipPriceWorkflowItem
    {
        public int WipId { get; set; }
        public int WipItemId { get; set; }
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? SalePrice { get; set; }
        public Guid? MasterId { get; set; }
    }

    public class ApproveDto
    {
        public int Id { get; set; }
        public Guid MasterId { get; set; }
        public int WipItemId { get; set; }
        public int WIPId { get; set; }
        public int KeyId { get; set; }
        public int ActionId { get; set; }
        public string Reason { get; set; }
    }

    public class RejectDto
    {
        public int Id { get; set; }
        public Guid MasterId { get; set; }
        public int WipItemId { get; set; }
        public string Reason { get; set; }
        public int WIPId { get; set; }
        public int KeyId { get; set; }
    }
}
