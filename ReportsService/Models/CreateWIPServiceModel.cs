using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportsService.Models
{
    public class CreateWIPServiceModel
    {
        public int Id { get; set; }
        public int? WIPId { get; set; }
        public string Code { get; set; }
        public string StatusCode { get; set; } = null;
        public string Description { get; set; } = null;
        public string LongDescription { get; set; } = null;
        public decimal StandardHours { get; set; }
        //public decimal Allowed { get; set; }
        public decimal Rate { get; set; }
        public decimal BaseRate { get; set; }
        public decimal Total { get; set; }
        public decimal? Discount { get; set; } = 0;
        public decimal TimeTaken { get; set; }
        public int Status { get; set; }
        public int? AccountType { get; set; }
        public string StatusText { get; set; } = null;
        public string StatusPrimaryName { get; set; } = null;
        public string StatusSecondaryName { get; set; } = null;
        public int? KeyId { get; set; }
        public int? TechnicianId { get; set; }
        public int? tableId { get; set; }// The actual Id in WIP_Service table
        public bool IsExternal { get; set; }
        public bool IsFixed { get; set; } = false;
    }
}