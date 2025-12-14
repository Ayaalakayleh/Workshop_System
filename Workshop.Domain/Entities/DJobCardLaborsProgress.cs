using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DJobCardLaborsProgress
{
    public int Id { get; set; }

    public DateTime? StartDate { get; set; }

    public decimal? WorkingHour { get; set; }

    public DateTime? PauseOrFinish { get; set; }

    public int? LaborLineItemId { get; set; }

    public virtual DJobCardLabor? LaborLineItem { get; set; }
}
