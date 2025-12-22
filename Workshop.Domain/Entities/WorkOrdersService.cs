using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class WorkOrdersService
{
	public int Id { get; set; }

	public int? WsWorkOrderId { get; set; }

	public int? VcVehicleId { get; set; }

	public int? AccServiceId { get; set; }

	public string? CardDescription { get; set; }

	public int? ServiceTime { get; set; }
}
public class VehicleWorkOrderSummery
{
	public int Id { get; set; }
	public int WorkOrderType { get; set; }
	public int VehicleId { get; set; }
	public int FK_VehicleMovementId { set; get; }
	public int FK_AgreementId { set; get; }
	public DateTime GregorianDamageDate { set; get; }
	public bool IsFix { set; get; }
	public int WorkOrderNo { get; set; }
	public int WorkOrderStatus { get; set; } // 1== Open 2== Inprogress 3== under reper 4== closed
	public string Services { get; set; }
	public string WorkOrderTitle { get; set; }
}
