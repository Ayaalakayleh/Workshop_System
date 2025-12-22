using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
	public class ServiceHistoryDTO
	{
		public int WorkOrderNo { get; set; }
		public string WorkOrderTitle { get; set; }
		public int VehicleId { get; set; }
		public DateTime GregorianDamageDate { get; set; }
		public int acc_ServiceId { get; set; }
		public string cardDescription { get; set; }
		public int ServiceTime { get; set; }
		public int BranchId { get; set; }

	}
}
