using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MExternalWorkshopExp
{
    public int Id { get; set; }

    public int? ExternalWorkshopId { get; set; }

    public DateTime? ExcelDate { get; set; }

    public int? Type { get; set; }

    public int CompanyId { get; set; }

    public int BranchId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int IsDeleted { get; set; }

    public virtual ICollection<DExternalWorkshopExp> DExternalWorkshopExps { get; set; } = new List<DExternalWorkshopExp>();
}
