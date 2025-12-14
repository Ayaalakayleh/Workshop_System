using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MWorkOrderDetail
{
    public int Id { get; set; }

    public string? PartName { get; set; }

    public int? WorkOrderType { get; set; }

    public int? WorkOrderId { get; set; }

    public string? Note { get; set; }

    public bool? IsFix { get; set; }

    public virtual ICollection<MWorkOrdersDetailsDocument> MWorkOrdersDetailsDocuments { get; set; } = new List<MWorkOrdersDetailsDocument>();

    public virtual MWorkOrder? WorkOrder { get; set; }
}
