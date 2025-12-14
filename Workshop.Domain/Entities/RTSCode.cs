using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Entities
{
    public class RTSCode
	{
       
		public int Id { get; set; }
        public string Code { get; set; }
        public string PrimaryDescription { get; set; }
		public string SecondaryDescription { get; set; }

		public int FK_CategoryId { get; set; }
		public int FK_FranchiseId { get; set; }

		public decimal StandardHours { get; set; }
		public int FK_SkillId { get; set; }
        public int FK_MenuId { get; set; }

		public int CompanyId { get; set; }
	
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime EffectiveDate { get; set; }
    }

}
