using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class LKPDefLookupHeaders
{
    public int Id { get; set; }
    public string? PrimaryName { get; set; }
    public string? SecondaryName { get; set; }
    public bool IsEditable { get; set; }
    public int CompanyId { get; set; }

    public virtual ICollection<LKPDefLookupDetails> LookupDetails { get; set; } = new List<LKPDefLookupDetails>();
}
