using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class LKPDefLookupDetails
{
    public int Id { get; set; }
    public int HeaderId { get; set; }
    public string Code { get; set; }
    public string PrimaryName { get; set; } 
    public string SecondaryName { get; set; } 
    public int CompanyId { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsEditable { get; set; }
    public bool IsDeleted { get; set; }

    public virtual LKPDefLookupHeaders Header { get; set; } = null!;
}
