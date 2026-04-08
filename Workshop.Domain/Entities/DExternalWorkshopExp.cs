using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DExternalWorkshopExp
{
    public int ID { get; set; }

    public int? HeaderId { get; set; }

    public int? VehicleId { get; set; }

    public string? Invoice_No { get; set; }

    public string License_Plate_No { get; set; } = null!;

    public DateTime? Invoice_Date { get; set; }

    public string? Business_Line { get; set; }

    public int? MILAGE { get; set; }

    public string? City { get; set; }

    public int? Quantity { get; set; }

    public decimal? Price { get; set; }

    public string? Description { get; set; }

    public string? Maker { get; set; }

    public string? VinNo { get; set; }

    public string? Model { get; set; }

    public string? Year { get; set; }

    public decimal? SubTotal_BeforVat { get; set; }

    public decimal? Vat { get; set; }

    public decimal? Total { get; set; }

    public string? Service_Type { get; set; }

    public int? WorkOrderId { get; set; }

    public virtual MExternalWorkshopExp? Header { get; set; }
}
