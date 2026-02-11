using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportsService.Models
{
    public class VehicleChecklist
    {
        public int Id { get; set; }
        public int LookupId { get; set; }
        public string LookupPrimaryDescription { get; set; } = null;
        public string LookupSecondaryDescription { get; set; } = null;
        public int MovementId { get; set; }
        public bool Pass { get; set; }
        public string Description { get; set; } = null;
    }
    public class TyreChecklist
    {
        public int Id { get; set; }
        public int LookupId { get; set; }
        public string LookupPrimaryDescription { get; set; } = null;
        public string LookupSecondaryDescription { get; set; } = null;
        public int MovementId { get; set; }
        public string Brand { get; set; } = null;
        public string DOT { get; set; } = null;
        public string WearLevel { get; set; } = null;
    }
}