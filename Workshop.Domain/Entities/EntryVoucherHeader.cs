using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class EntryVoucherHeader
{
    public int Id { get; set; }

    public DateTime? Date { get; set; }

    public int? CompanyId { get; set; }

    public int? BranchId { get; set; }

    public bool? IsApproveStoreKeeper { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public int? ModifyBy { get; set; }

    public DateTime? ModifyAt { get; set; }

    public int? TransferId { get; set; }

    public int? PurchaseInvoiceId { get; set; }

    public string? VendorInvoiceNo { get; set; }
}
