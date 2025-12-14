using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DJobCard
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? WorkOrderId { get; set; }

    public int? Status { get; set; }

    public DateTime? IssueDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? CompleteDate { get; set; }

    public int? Priority { get; set; }

    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyAt { get; set; }

    public decimal? OdoMeter { get; set; }

    public Guid? MasterId { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? CompleteTime { get; set; }

    public decimal? ReceivedMeter { get; set; }

    public decimal? TotalParts { get; set; }

    public decimal? TotalTechnicians { get; set; }

    public decimal? Discount { get; set; }

    public decimal? Tax { get; set; }

    public decimal? TotalOrder { get; set; }

    public int? WorkshopId { get; set; }

    public decimal? JobCardNo { get; set; }

    public bool IsExternal { get; set; }

    public bool OilChange { get; set; }

    public decimal OriginalMeter { get; set; }

    public decimal OilMeter { get; set; }

    public DateTime? NextOilChangeDate { get; set; }

    public virtual ICollection<DJobCardLineItem> DJobCardLineItems { get; set; } = new List<DJobCardLineItem>();
}
