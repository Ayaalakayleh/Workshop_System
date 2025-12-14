using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DServiceReminder
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public int ManufacturerId { get; set; }

    public int VehicleModelId { get; set; }

    public int ManufacturingYear { get; set; }

    public long ItemId { get; set; }

    public int Repates { get; set; }

    public int? TimeInterval { get; set; }

    public int? TimeIntervalUnit { get; set; }

    public int? TimeDue { get; set; }

    public int? TimeDueUnit { get; set; }

    public int? PrimaryMeterInterval { get; set; }

    public int? PrimaryMeterDue { get; set; }

    public bool IsManually { get; set; }

    public DateOnly? ManualDate { get; set; }

    public int? ManualPrimaryMeter { get; set; }

    public bool HasNotification { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? ModifyAt { get; set; }

    public int? ModifyBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateOnly? LastCompleted { get; set; }

    public int? ReminderStatus { get; set; }

    public DateOnly? StartDate { get; set; }

    public decimal? StartMeter { get; set; }

    public string? NotificationsGroup { get; set; }

    public int? VehicleGroupId { get; set; }

    public virtual ICollection<DServiceRemindersSchedule> DServiceRemindersSchedules { get; set; } = new List<DServiceRemindersSchedule>();
}
