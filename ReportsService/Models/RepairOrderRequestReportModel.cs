using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportsService.Models
{
    public class RepairOrderRequestReportModel
    {
        public int? WIPId { get; set; } 
        public int? MovementId { get; set; }
        public int? VehicleNo { get; set; } 
        public DateTime? Date { get; set; } = DateTime.Now;
        public TimeSpan? TimeReceived { get; set; }
        public VehicleInfoModel VehicleInfo { get; set; }
        public string ContractExpDate { get; set; } = null;
        public string CompanyName { get; set; } = null;
        public string FuelLevel { get; set; } = null;
        public string InsuranceExpDate { get; set; } = null;
        public string CustomerName { get; set; } = null;
        public string CreatedBy { get; set; } = null;
        public string CreatedDate { get; set; } = null;
        public string UserPhoeNo { get; set; } = null;
        //public string? ColorName { get; set; }
        public string Trim { get; set; } = null;
        public string EstimaraExpDate { get; set; } = null;
        public string CustomerMobileNumber { get; set; } = null;
        public string MVPIExpDate { get; set; } = null;
        public string RegistrationExpDate { get; set; } = null;
        public string RegistrationNo { get; set; } = null;
        public string Complaint { get; set; } = null;
        public string DateIn { get; set; } = null;
        public string TimeIn { get; set; } = null;
        public string DateOut { get; set; } = null;
        public string TimeOut { get; set; } = null;
        public string DateLastVisit { get; set; } = null;
        public string AccountNo { get; set; } = null;
        public string RepeatRepair { get; set; } = null;
        public string Company { get; set; } = null;
        public List<VehicleChecklist> VehicleCkecklist { get; set; } = new List<VehicleChecklist>();
        public List<TyreChecklist> TyreCkecklist { get; set; } = new List<TyreChecklist>();
        public List<CreateWIPServiceModel> Services { get; set; } = new List<CreateWIPServiceModel>();
        public List<ItemModel> Items { get; set; } = new List<ItemModel>();
        public byte[] DamageImageBytes { get; set; }
        public byte[] DamageImageBytes_Vertical { get; set; }
        public string RecallListText { get; set; } = null;
        public string Branch { get; set; } = null;
        public string Note { get; set; } = null;
        public CompanyDataModel CompanyData { get; set; } = new CompanyDataModel();
    }
}