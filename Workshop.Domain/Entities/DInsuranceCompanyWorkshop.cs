using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DInsuranceCompanyWorkshop
{
    public int Id { get; set; }

    public int? InsuranceCompanyId { get; set; }

    public int? WorkshopId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }
}
