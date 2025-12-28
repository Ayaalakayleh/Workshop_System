using DocumentFormat.OpenXml.Bibliography;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;

namespace Workshop.Web.Models
{
    public class RepairOrderRequestReportModel
    {
        public int? WIPId { get; set; }
        public int? MovementId { get; set; }
        public int? VehicleNo { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? TimeReceived { get; set; }
        public VehicleInfoModel VehicleInfo { get; set; }
        public string? ContractExpDate { get; set; }
        public string? CompanyName { get; set; }
        public string? FuelLevel { get; set; }
        public DateTime? InsuranceExpDate { get; set; }
        public string? CustomerName { get; set; }
        //public string? ColorName { get; set; }
        public string? Trim { get; set; }
        public DateTime? EstimaraExpDate { get; set; }
        public string? MobileNumber { get; set; }
        public DateTime? MVPIExpDate { get; set; }
        public string? Complaint { get; set; }
        public string? DateIn { get; set; }
        public TimeSpan? TimeIn { get; set; }
        public string? DateOut { get; set; }
        public TimeSpan? TimeOut { get; set; }
        public DateTime? DateLastVisit { get; set; }
        public string? AccountNo { get; set; }
        //public List<string>? Services { get; set; }
        public List<VehicleChecklist>? VehicleCkecklist { get; set; }
        public List<TyreChecklist>? TyreCkecklist { get; set; }
        public List<CreateWIPServiceDTO>? Services { get; set; }
        public List<ItemModel>? Items { get; set; }
    }
}
