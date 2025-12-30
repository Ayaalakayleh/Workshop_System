namespace Workshop.Web.Models
{
    public class CreatePettyCashSellerDTO
    {
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string TaxNumber { get; set; }
    }

    public class PettyCashVehicleMovementDTO
    {
        // Movement fields
        public int? MovementId { get; set; }
        public int? VehicleID { get; set; }
        public int? MoveInWorkshopId { get; set; }
        public int? MoveOutWorkshopId { get; set; }
        public int? WorkshopId { get; set; }
        public int? MasterId { get; set; }
        public int? ExitDriverId { get; set; }
        public int? LastVehicleStatus { get; set; }
        public int? WorkOrderId { get; set; }

        // Movement specific fields
        public DateTime? PettyCash_MovementDate { get; set; }
        public TimeSpan? PettyCash_ReceivedTime { get; set; }
        public int? PettyCash_ReceivedMeter { get; set; }
        public int? PettyCash_FuelLevelId { get; set; }
        public string PettyCash_DriverName { get; set; }

        // Petty Cash fields
        public long PettyCash_RequestNo { get; set; }
        public int PettyCash_ExpenseType { get; set; }
        public int PettyCash_TypeOfExpense { get; set; }
        public int PettyCash_SellerId { get; set; }
        public string PettyCash_InvoiceNo { get; set; }
        public DateTime PettyCash_InvoiceDate { get; set; }
        public int PettyCash_CurrencyId { get; set; }
        public decimal PettyCash_PartsCost { get; set; }
        public decimal PettyCash_Vat { get; set; }
        public decimal PettyCash_VatRate { get; set; }
        public decimal PettyCash_LaborCost { get; set; }
        public decimal PettyCash_NetAmount { get; set; }
        public int PettyCash_TaxClassificationId { get; set; }
        public decimal PettyCash_Tax { get; set; }
        public decimal PettyCash_TotalAmount { get; set; }
        public decimal PettyCash_KM { get; set; }
        public decimal PettyCash_LastKM { get; set; }
        public string PettyCash_Description { get; set; }
        public string PettyCash_FixedServiceIds { get; set; }
        public bool PettyCash_NotTaxable { get; set; }
    }
}