using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportsService.Models
{
    public class VehicleInfoModel
    {
        public string Make { get; set; } = null;
        public string Model { get; set; } = null;
        public int? Year { get; set; }
        public string PlateNumber { get; set; } = null;
        public decimal? Mileage { get; set; }
        public string VIN { get; set; } = null;
        public string ColorName { get; set; } = null;
        public string CustomerName { get; set; } = null;
    }
}