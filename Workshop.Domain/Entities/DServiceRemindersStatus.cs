using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DServiceRemindersStatus
{
    public int Id { get; set; }

    public string? PrimaryName { get; set; }

    public string? SecondaryName { get; set; }
}
