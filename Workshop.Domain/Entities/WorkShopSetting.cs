using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class WorkShopSetting
{
    public int Id { get; set; }

    public int? ExpensesTypeId { get; set; }

    public int? PaymentMethodId { get; set; }

    public int? CompanyId { get; set; }

    public int? BranchId { get; set; }
}
