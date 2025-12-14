using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MMovementDocument
{
    public int Id { get; set; }

    public int MovementId { get; set; }

    public string? FilePath { get; set; }

    public string? FileName { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual DWorkshopMovement Movement { get; set; } = null!;
}
