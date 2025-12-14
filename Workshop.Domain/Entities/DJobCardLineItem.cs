using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DJobCardLineItem
{
    public int Id { get; set; }

    public int? ServiceId { get; set; }

    public decimal? TotalLaborCost { get; set; }

    public decimal? TotalPartsCost { get; set; }

    public int? JobCardId { get; set; }

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyAt { get; set; }

    public string? WorkOrderId { get; set; }

    public decimal? TotalPartsTax { get; set; }

    public decimal? TotalLaborTax { get; set; }

    public virtual ICollection<DJobCardItem> DJobCardItems { get; set; } = new List<DJobCardItem>();

    public virtual ICollection<DJobCardLabor> DJobCardLabors { get; set; } = new List<DJobCardLabor>();

    public virtual DJobCard? JobCard { get; set; }
}
