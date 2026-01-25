using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class MonthlyRepairCostBranchWiseReportDTO
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
        public decimal? TotalLubes { get; set; }
        public decimal? TotalPaints { get; set; }
        public decimal? TotalLabours { get; set; }
        public decimal? TotalParts { get; set; }
        public decimal? LabourCost { get; set; }
        public decimal? PartsCost { get; set; }
        public decimal? Sublet { get; set; }
        public decimal? TotalLaboursDiscount { get; set; }
        public decimal? TotalPartsDiscount { get; set; }
        public string? VIN { get; set; }
        public string? PlateNumber { get; set; }
        public string? Manufacturer { get; set; }
        public int? ManufacturerId { get; set; }
        public int? OPNumber { get; set; }
        public string? OPName { get; set; }
        public string? OPUserName { get; set; }
        public int? VehicleId { get; set; }
        public bool? IsExternal { get; set; }
        public decimal? Millage { get; set; }
        public int? workshopId { get; set; }
    }
}
