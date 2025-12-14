using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MWorkOrdersDetailsDocument
{
    public int Id { get; set; }

    public int? WorkOrderDetailsId { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public virtual MWorkOrderDetail? WorkOrderDetails { get; set; }
}
