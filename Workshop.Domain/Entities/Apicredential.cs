using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class Apicredential
{
    public int Id { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }
}
