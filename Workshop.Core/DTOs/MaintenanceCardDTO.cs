namespace Workshop.Core.DTOs
{

    public class MaintenanceCardDTO
    {
        public int? Id { get; set; }
        public int? MovementId { get; set; }
        public int? WorkOrderId { get; set; }
        public int? ServiceId { get; set; }
        public string? Description { get; set; }
        public bool? status { get; set; }
        public List<MaintenanceCardDTO>? Cards { get; set; }
        public List<WorkOrderFilterDTO>? WorkOrders { get; set; }
        public int? ServiceTime { get; set; }
    }

    public class MaintenanceCardDt
    {
        public int? MovementId { get; set; }
        public int? WorkOrderId { get; set; }
        public int? ServiceId { get; set; }
        public string? Description { get; set; }
        public bool? status { get; set; }
        //public int? ServiceTime { get; set; }
    }

}
