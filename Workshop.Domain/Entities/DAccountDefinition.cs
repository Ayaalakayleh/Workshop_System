using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DAccountDefinition
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public int CompanyId { get; set; }

    public int? JournalId { get; set; }

    public int? CostAccountId { get; set; }

    public bool IsJobCardWithCost { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
