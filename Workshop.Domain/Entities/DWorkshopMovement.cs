using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DWorkshopMovement
{
    public int MovementId { get; set; }

    public int? ExitBranchId { get; set; }

    public TimeOnly? ExitTime { get; set; }

    public int? VehicleId { get; set; }

    public decimal? ExitMeter { get; set; }

    public decimal? ReceivedMeter { get; set; }

    public string? ExitDriverId { get; set; }

    public string? ResivedDriverId { get; set; }

    public int? ReceivedBranchId { get; set; }

    public TimeOnly? ReceivedTime { get; set; }

    public string? Note { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyAt { get; set; }

    public bool? MovementOut { get; set; }

    public bool? MovementIn { get; set; }

    public DateTime? GregorianMovementDate { get; set; }

    public DateOnly? HijriMovementDate { get; set; }

    public bool? IshijriMovement { get; set; }

    public int? CompanyId { get; set; }

    public int? WorkshopId { get; set; }

    public int? MovementInId { get; set; }

    public Guid? MasterId { get; set; }

    public int? FuelLevelId { get; set; }

    public bool? WorkOrderStatus { get; set; }

    public DateTime? GregorianMovementEndDate { get; set; }

    public int? MoveInWorkshopId { get; set; }

    public int? MoveOutWorkshopId { get; set; }

    public int? MovementOutId { get; set; }

    public bool? IsRegularMaintenance { get; set; }

    public string? EmployeeSignature { get; set; }

    public string? DriverSignature { get; set; }

    public int? MaintenanceStatus { get; set; }

    public int? LastVehicleStatus { get; set; }

    public bool IsExternal { get; set; }

    public int? WorkOrderId { get; set; }

    public virtual ICollection<DExternalWorkshopInvoice> DExternalWorkshopInvoices { get; set; } = new List<DExternalWorkshopInvoice>();

    public virtual ICollection<MMovementDocument> MMovementDocuments { get; set; } = new List<MMovementDocument>();

    public virtual ICollection<MWorkshopInvoice> MWorkshopInvoices { get; set; } = new List<MWorkshopInvoice>();

    public virtual ICollection<MWorkshopMovementStrike> MWorkshopMovementStrikes { get; set; } = new List<MWorkshopMovementStrike>();
}
