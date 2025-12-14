using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MTaqdeeratDocument
{
    public int Id { get; set; }

    public int? WorkOrderId { get; set; }

    public string? FilePath { get; set; }

    public string? TaqFileName { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual MWorkOrder? WorkOrder { get; set; }
}
