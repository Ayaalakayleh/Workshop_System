using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MWorkshopInvoice
{
    public int Id { get; set; }

    public int? MovementId { get; set; }

    public string? InvoiceNo { get; set; }

    public decimal? TotalAmount { get; set; }

    public int? ExternalWorkshopId { get; set; }

    public Guid? MasterId { get; set; }

    public int? WorkOrderId { get; set; }

    public decimal? DeductibleAmount { get; set; }

    public decimal? ConsumptionValueOfSpareParts { get; set; }

    public decimal? Vat { get; set; }

    public decimal? LaborCost { get; set; }

    public decimal? PartsCost { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public virtual DWorkshopMovement? Movement { get; set; }
}
