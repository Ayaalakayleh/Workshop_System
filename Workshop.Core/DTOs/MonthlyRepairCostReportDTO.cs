using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class MonthlyRepairCostReportDTO
    {

        public int WIP { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int? InvoiceNumber { get; set; }
        public string? CompanyCode { get; set; }
        public string? Account { get; set; }
        public string? Department { get; set; }
        public int? CustomerId { get; set; }
        public int? PartialCustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalLabours { get; set; }
        public decimal? TotalParts { get; set; }
        public string? VIN { get; set; }
        public string? PlateNumber { get; set; }
        public int? ManufactureYear { get; set; }
        public int? OPNumber { get; set; }
        public string? OPName { get; set; }
        public int? VehServiceId { get; set; }
        public string? VehServiceCode { get; set; }
        public string? VehServiceDesc { get; set; }
        public int? VehicleId { get; set; }
        public bool? IsExternal { get; set; }
        public int? MovementId { get; set; }
        public decimal? Millage { get; set; }
        public int? workshopId { get; set; }

    }
    public class MonthlyRepairCostReportFilterDTO
    {
        public int? WIP { get; set; }
        public DateTime? InvoiceDateStart { get; set; }
        public DateTime? InvoiceDateEnd { get; set; }
        public int? CustomerId { get; set; }

    }
}
