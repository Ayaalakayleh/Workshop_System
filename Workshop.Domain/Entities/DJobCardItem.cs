using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DJobCardItem
{
    public int Id { get; set; }

    public int? ItemId { get; set; }

    public int? LineItemId { get; set; }

    public decimal? Cost { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyAt { get; set; }

    public int? Quantity { get; set; }

    public int? Unit { get; set; }

    public int? Warehouse { get; set; }

    public string? KeyId { get; set; }

    public virtual DJobCardLineItem? LineItem { get; set; }
}
