using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DMaintemanceCard
{
    public int Id { get; set; }

    public int? MovementId { get; set; }

    public int? WorkOrderId { get; set; }

    public string? Description { get; set; }

    public bool? Status { get; set; }

    public int? ServiceId { get; set; }

    public virtual MWorkOrder? WorkOrder { get; set; }
}
