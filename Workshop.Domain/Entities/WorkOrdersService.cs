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
