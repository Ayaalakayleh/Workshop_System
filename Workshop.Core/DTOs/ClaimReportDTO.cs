using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
	public class ClaimReportDTO
	{
		public int ClaimHistoryID { get; set; }
		public int Status { get; set; }
		public DateTime ClaimDate { get; set; }
		public int WorkOrderId { get; set; }
		public int WorkOrderType { get; set; }
		public string SystemClaims { get; set; }
		public int WorkOrderVehicleId { get; set; }
		public string AccidentNo { get; set; }
		public bool IsInsuraceDamege { get; set; }
		public int InsuranceCompanyId { get; set; }
		public string BTCClaim { get; set; }
		public DateTime AccidentDate { get; set; }
		public string VehicleDamageSide { get; set; }
		public decimal EstimationAmt { get; set; }
		public decimal RepairCost { get; set; }
		public decimal ApprovedRepairCost { get; set; }
		public decimal CollectionAmtfromcust { get; set; }
		public int RA { get; set; }
		public string Remarks { get; set; }
		public int Fault { get; set; }
		public int WorkshopId { get; set; }
		public string WorkshopNameEn { get; set; }
		public string WorkshopNameAr { get; set; }
	}
}
