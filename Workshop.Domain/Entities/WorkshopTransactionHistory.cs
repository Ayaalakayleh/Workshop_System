using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class WorkshopTransactionHistory
{
    public int Id { get; set; }

    public Guid? MasterId { get; set; }

    public int? Status { get; set; }

    public string? Reason { get; set; }

    public int? WorkshopId { get; set; }
}
