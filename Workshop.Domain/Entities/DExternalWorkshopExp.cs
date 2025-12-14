using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DExternalWorkshopExp
{
    public int Id { get; set; }

    public int? HeaderId { get; set; }

    public int? VehicleId { get; set; }

    public string? InvoiceNo { get; set; }

    public string LicensePlateNo { get; set; } = null!;

    public DateTime? InvoiceDate { get; set; }

    public string? BusinessLine { get; set; }

    public int? Milage { get; set; }

    public string? City { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public string? Maker { get; set; }

    public string? VinNo { get; set; }

    public string? Model { get; set; }

    public string? Year { get; set; }

    public decimal? SubTotalBeforVat { get; set; }

    public decimal? Vat { get; set; }

    public decimal? Total { get; set; }

    public string? ServiceType { get; set; }

    public int? WorkOrderId { get; set; }

    public virtual MExternalWorkshopExp? Header { get; set; }
}
