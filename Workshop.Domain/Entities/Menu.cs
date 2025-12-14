using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Entities
{
    public class Menu
    {
        public int Id { get; set; }

        public string GroupCode { get; set; } = null!;   
        public string GroupName { get; set; } = null!;  
        public string PrimaryDescription { get; set; }
        public string SecondaryDescription { get; set; }

        public string PricingMethod { get; set; } = null!; // "Sum" or "Fixed"
        public decimal Price { get; set; }  
        public decimal TotalTime { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public int? CreatedBy { get; set; }

        public DateTime UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        public ICollection<MenuRTSCodes>? GroupCodes { get; set; }
    }


}
