using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DJobCardLabor
{
    public int Id { get; set; }

    public int? LineItemId { get; set; }

    public int? LaborId { get; set; }

    public int? Hours { get; set; }

    public decimal? HourRate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyAt { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int? Status { get; set; }

    public virtual ICollection<DJobCardLaborsProgress> DJobCardLaborsProgresses { get; set; } = new List<DJobCardLaborsProgress>();

    public virtual DTechnician? Labor { get; set; }

    public virtual DJobCardLineItem? LineItem { get; set; }
}
