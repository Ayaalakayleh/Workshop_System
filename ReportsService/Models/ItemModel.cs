using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportsService.Models
{
    public class ItemModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string ItemNumber { get; set; }
        public string Name { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string UnitPrimaryName { get; set; }
        public string UnitSecondaryName { get; set; }
        public decimal? Qty { get; set; }
    }
}