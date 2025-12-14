using System;
using System.Collections.Generic;

namespace Workshop.Infrastructure;

public partial class DWorkShop
{
    public int Id { get; set; }

    public string? PrimaryName { get; set; }

    public string? SecondaryName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? CityId { get; set; }

    public string? PrimaryAddress { get; set; }

    public string? SecondaryAddress { get; set; }

    public string? GoogleUrl { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public int CompanyId { get; set; }

    public int BranchId { get; set; }

    public int ParentId { get; set; }

    public int? UserId { get; set; }

    public bool IsActive { get; set; }

    public bool? IsExternalWorkshop { get; set; }

    public int? VatClassificationId { get; set; }

    public int? AccountId { get; set; }

    public int? SupplierId { get; set; }
}
