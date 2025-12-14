using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DServiceRemindersSchedule
{
    public int Id { get; set; }

    public int ServiceReminderId { get; set; }

    public DateOnly? Date { get; set; }

    public decimal? PrimaryMeter { get; set; }

    public bool IsCompleted { get; set; }

    public int? VehicleId { get; set; }

    public int? ItemId { get; set; }

    public DateOnly? DueDate { get; set; }

    public decimal? DuePrimaryMeter { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public decimal? CompletedMeter { get; set; }

    public virtual DServiceReminder ServiceReminder { get; set; } = null!;
}
