namespace Workshop.Web.Models
{
    public class RepairOrderRequestReportModel
    {
        public int? VehicleNo { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? TimeReceived { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public string? PlateNumber { get; set; }
        public string? Mileage { get; set; }
        public string? VIN { get; set; }
        public DateTime? ContractExpDate { get; set; }
        public string? CompanyName { get; set; }
        public DateTime? InsuranceExpDate { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? EstimaraExpDate { get; set; }
        public string? MobileNumber { get; set; }
        public DateTime? MVPIExpDate { get; set; }
        public string? Complaint { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeOut { get; set; }
        public string? AccountNo { get; set; }
        public List<string>? Services { get; set; }
    }
}
