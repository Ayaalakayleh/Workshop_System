using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class LkpEngineFuelType
{
    public int Id { get; set; }

    public string TypeNameEn { get; set; } = null!;

    public string TypeNameAr { get; set; } = null!;

    public bool IsActive { get; set; }
}
