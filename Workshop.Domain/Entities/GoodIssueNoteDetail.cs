using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class GoodIssueNoteDetail
{
    public int Id { get; set; }

    public int? HeaderId { get; set; }

    public string? KeyId { get; set; }

    public int? ItemId { get; set; }

    public int? WarehouseId { get; set; }

    public int? UnitId { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? UnitQuantity { get; set; }

    public decimal? Price { get; set; }

    public decimal? Total { get; set; }

    public virtual GoodIssueNoteHeader? Header { get; set; }
}
