using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class MExcelMapping
{
    public int Id { get; set; }

    public int? WorkshopId { get; set; }

    public string FilePath { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public int? StartedColumn { get; set; }

    public int? StartedRow { get; set; }

    public int CompanyId { get; set; }

    public int? BranchId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<DExcelMapping> DExcelMappings { get; set; } = new List<DExcelMapping>();
}
