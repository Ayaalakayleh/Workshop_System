using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MWorkshopMovementStrike
{
    public int Id { get; set; }

    public int? MovementId { get; set; }

    public string? Strikes { get; set; }

    public virtual DWorkshopMovement? Movement { get; set; }
}
