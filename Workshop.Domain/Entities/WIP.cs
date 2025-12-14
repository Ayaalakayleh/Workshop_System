using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Infrastructure;
using static System.Net.Mime.MediaTypeNames;

namespace Workshop.Domain.Entities
{
    public class WIP
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int WorkOrderId { get; set; } //Damage Id
        public decimal JobCardNo { get; set; } //Workorder NO.
        public int Status { get; set; }
        public string Note { get; set; }
        public decimal TotalParts { get; set; }
        public decimal TotalTechnicians { get; set; }
        public int WorkshopId { get; set; }
        public bool IsExternal { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ModifyBy { get; set; }
        public DateTime ModifyAt { get; set; }
    }
    
}
