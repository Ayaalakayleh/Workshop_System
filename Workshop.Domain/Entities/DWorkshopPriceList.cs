using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DWorkshopPriceList
{
    public int WorkshopId { get; set; }

    public long ItemId { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyAt { get; set; }
}
