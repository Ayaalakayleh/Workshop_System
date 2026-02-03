namespace Workshop.Web.Models
{
    public class CreateInventoryTransactionResult
    {
        public bool Success { get; set; }
        public long? HeaderId { get; set; }
        public string Message { get; set; } = "";
        public List<InventoryIssueFailureRow> Shortages { get; set; } = new();
    }
    public class InventoryIssueFailureRow
    {
        public long ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        public int? LocatorId { get; set; }
        public string? LocatorCode { get; set; }

        public int RequestedUnitId { get; set; }
        public string? RequestedUnitName { get; set; }
        public string? RequestedUnitSecondaryName { get; set; }

        public decimal RequestedQty { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal ShortageQty { get; set; }
    }
}
