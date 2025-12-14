using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Entities
{
	public partial class DRecall
	{
		public int Id { get; set; }
		public string? Code { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool IsActive { get; set; }
		public DateTime? CreatedAt { get; set; }
		public int? CreatedBy { get; set; }
		public DateTime? UpdatedAt { get; set; }
		public int? UpdatedBy { get; set; }
	}

}
