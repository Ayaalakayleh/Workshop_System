using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class LookupDetail
{
    public int Id { get; set; }

    public int HeaderId { get; set; }

    public string Primaryname { get; set; } = null!;

    public string SeconderyName { get; set; } = null!;

    public string CreatedBy { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int StatusValue { get; set; }

    public int? RoleId { get; set; }

    public virtual LookupHeader Header { get; set; } = null!;
}
