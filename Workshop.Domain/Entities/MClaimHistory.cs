using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MClaimHistory
{
    public int Id { get; set; }

    public int? WorkOrderId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? Status { get; set; }

    public bool? IfCancelled { get; set; }
}
