using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DExternalWorkshopInvoice
{
    public int Id { get; set; }

    public int? MovementId { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public int? WorkOrderId { get; set; }

    public virtual DWorkshopMovement? Movement { get; set; }
}
