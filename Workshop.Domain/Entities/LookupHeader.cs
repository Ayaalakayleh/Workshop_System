using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class LookupHeader
{
    public int Id { get; set; }

    public string? PrimaryName { get; set; }

    public string? SecondaryName { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<LookupDetail> LookupDetails { get; set; } = new List<LookupDetail>();
}
