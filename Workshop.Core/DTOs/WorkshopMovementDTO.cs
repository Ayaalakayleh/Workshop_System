using Workshop.Core.DTOs.Vehicle;

namespace Workshop.Core.DTOs.WorkshopMovement
{
    public class WorkshopMovementFilter
    {
        public int? MovementId { get; set; }
        public int? VehicleID { get; set; }
        public bool? Recieved { set; get; }
        public bool? NotRecieved { set; get; }
        public DateTime? GregorianDate { set; get; }
        public DateTime? hijriDate { set; get; }
        public string? VehicleName { set; get; }
        public List<VehicleNams>? vehicleNams { set; get; }
        public int? page { get; set; }
        public int? TotalPages { get; set; }
        public int? WorkshopId { get; set; }
        public string? language { get; set; }

        public DateTime? FromDate { set; get; }
        public DateTime? ToDate { set; get; }
        public bool? MovementOut { set; get; }
        public bool? MovementIN { set; get; }
    }
    public class MovementInvoice
    {
        public int Id { set; get; }
        public int MovementId { set; get; }
        public string? FilePath { set; get; }
        public string? FileName { set; get; }
        public string? InvoiceNo { set; get; }
        public Guid MasterId { get; set; }
        public decimal TotalInvoice { get; set; }
        public int ExternalWorkshopId { get; set; }
        public int WorkOrderId { set; get; }
        public decimal ConsumptionValueOfSpareParts { set; get; }
        public decimal Vat { set; get; }
        public decimal DeductibleAmount { set; get; }
        public decimal PartsCost { get; set; }
        public decimal LaborCost { get; set; }
        public DateTime? Invoice_Date { get; set; }
        public decimal TotalAmount { get; set; }

    }
    public class VehicleMovementStatusDTO
    {
        public int MovementId { get; set; }
        public DateTime lastmovemnetDate { get; set; }
        public int result { get; set; }
    }

    public class WorckshopPrice
    {
        public int WorkShopId { get; set; }
        public Int64 ServiceId { get; set; }
        public string ServiceName { get; set; }
        public string WorkShopName { get; set; }
        public decimal Price { get; set; }
    }

    public class WorckshopPriceTable
    {
        public List<WorckshopPrice> WorckshopPrice { get; set; }
        public List<Tablecol> Tablecol { get; set; }
        public List<TableRows> TableRows { get; set; }
    }

    public class TableRows
    {
        public int WorkShopId { get; set; }
        public string WorkShopName { get; set; }
    }

    public class Tablecol
    {
        public Int64 ServiceId { get; set; }
        public string ServiceName { get; set; }
    }
}
