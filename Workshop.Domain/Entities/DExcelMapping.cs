using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DExcelMapping
{
    public int Id { get; set; }

    public int HeaderId { get; set; }

    public string? ColumnName { get; set; }

    public string? AdditionCullmanName { get; set; }

    public string? MappingColumnDb { get; set; }

    public int? MappingColumnIndex { get; set; }

    public bool? IsVatIncluded { get; set; }

    public string? AdditionData { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual MExcelMapping Header { get; set; } = null!;
}
