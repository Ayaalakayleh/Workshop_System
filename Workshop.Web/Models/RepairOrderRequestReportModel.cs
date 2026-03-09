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
        public TimeSpan? TimeReceived { get; set; }
        public VehicleInfoModel VehicleInfo { get; set; }
        public string? ContractExpDate { get; set; }
        public string? CompanyName { get; set; }
        public string? FuelLevel { get; set; }
        public string? InsuranceExpDate { get; set; }
        public string? CustomerName { get; set; }
        public string? CreatedBy { get; set; }
        public string? CreatedDate { get; set; }
        public string? UserPhoeNo { get; set; }
        //public string? ColorName { get; set; }
        public string? Trim { get; set; }
        public string? EstimaraExpDate { get; set; }
        public string? CustomerMobileNumber { get; set; }
        public string? MVPIExpDate { get; set; }
        public string? RegistrationExpDate { get; set; }
        public string? RegistrationNo { get; set; }
        public string? Complaint { get; set; }
        public string? DateIn { get; set; }
        public string? TimeIn { get; set; }
        public string? DateOut { get; set; }
        public string? TimeOut { get; set; }
        public string? DateLastVisit { get; set; }
        public string? AccountNo { get; set; }
        public string? RepeatRepair { get; set; }
        public string? Company { get; set; }
        //public List<string>? Services { get; set; }
        public List<VehicleChecklist>? VehicleCkecklist { get; set; }
        public List<TyreChecklist>? TyreCkecklist { get; set; }
        public List<CreateWIPServiceDTO>? Services { get; set; }
        public List<ItemModel>? Items { get; set; }
        public byte[] DamageImageBytes { get; set; }
        public byte[] DamageImageBytes_Vertical { get; set; }
        public List<ActiveRecallDto> Recalls { get; set; } = new List<ActiveRecallDto>();
        public string RecallListText { get; set; } = null;
        public string Branch { get; set; } = null;
        public string Note { get; set; } = null;
        public CompanyDataModel CompanyData { get; set; } = new CompanyDataModel();
    }
}
