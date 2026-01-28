using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class WIPReportDTO
    {
        public int WIP { get; set; }
        public int? StatusId { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? DueIn { get; set; }
        public string? DueOut { get; set; }
        public int? Ageing { get; set; }
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
        public int? MakeId { get; set; }
        public int? ModelId { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? VehicleId { get; set; }
        public bool? IsExternal { get; set; }
        public int? MovementId { get; set; }
        public int? ServiceAdvisorId { get; set; }
        public string? ServiceAdvisor { get; set; }
        public int? Branch { get; set; }
        public string? Message { get; set; }
        public int? Count { get; set; }
        public string? VehicleInOut { get; set; }


    }

    public class WIPReportFilterDTO
    {
        public int? WIP { get; set; }
        public int? StatusId { get; set; }
        public int? CustomerId { get; set; }

    }

}
