namespace Workshop.Core.DTOs
{
    public class ServiceScheduleDTO
    {

        public int VehicleId { get; set; }
        public int WorkOrderId { get; set; }
        public decimal Meter { get; set; }
        public DateTime Date { get; set; }

    }
}
