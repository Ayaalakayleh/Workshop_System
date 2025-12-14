using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DWorkOrderReport
{
    public int Id { get; set; }

    public int WorkOrderId { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public virtual MWorkOrder WorkOrder { get; set; } = null!;
}
