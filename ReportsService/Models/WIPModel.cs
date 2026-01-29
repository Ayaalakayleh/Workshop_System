using System;

namespace ReportsService.Models {

    public class WIPModel
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string AccountNo { get; set; }
        public DateTime Date { get; set; }
        public DateTime TimeReceived { get; set; }
        public DateTime InsuranceExpDate { get; set; }
        public DateTime EstimaraExpDate { get; set; }
        public DateTime MVPIExpDate { get; set; }
        public string Company { get; set; }
        public string CustomerName { get; set; }
        public string Complaint { get; set; }
        public string CustomerMobileNumber { get; set; }
    }
}