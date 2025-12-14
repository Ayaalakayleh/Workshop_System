using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class ItemDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string ItemNumber { get; set; }
        public string Name { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string ManufacturerNumber { get; set; }
        public decimal Price { get; set; }
        public decimal SalePrice { get; set; }
        public decimal PurchasePrice { get; set; }
        public string SubCategoryPrimaryName { get; set; }
        public string SubCategorySecondaryName { get; set; }
        public string CategoryPrimaryName { get; set; }
        public string CategorySecondaryName { get; set; }
        public string GroupPrimaryName { get; set; }
        public string GroupSecondaryName { get; set; }
        public string ImagePath { get; set; }
        public int FK_GroupId { get; set; }
        public int FK_CategoryId { get; set; }
        public int FK_SubCategoryId { get; set; }
        public int FK_UnitId { get; set; }
        public string UnitName { get; set; }
        public string UnitPrimaryName { get; set; }
        public string UnitSecondaryName { get; set; }
        public string CategoryName { get; set; }
        public int FK_TaxClassificationId { get; set; }
        public bool? IsSerialized { get; set; }
        public bool? IsUniqueSerial { get; set; }
        public bool? IsBatchControl { get; set; }
        public bool? IsExpiryDate { get; set; }
        public bool? IsWarrantlyEligible { get; set; }
        public bool? IsInspectionReq { get; set; }
        public bool? IsActive { get; set; }
        public string BaseUnitPrimaryName { get; set; }
        public string BaseUnitSecondaryName { get; set; }
        public int WarehouseId { get; set; }
        public string WarehousePrimaryName { get; set; }
        public string WarehouseSecondaryName { get; set; }
        public int? LocatorId { get; set; }
        public string LocatorCode { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal AvgCost { get; set; }
        public int TotalRows { get; set; }
        public string SubCategoryName { get; set; }

    }

    public class GroupDTO
    {
        public int Id { get; set; }
        public string primaryName { get; set; }
        public string secondaryName { get; set; }
    }

    public class CategoryDTO
    {
        public int Id { get; set; }
        public string primaryName { get; set; }
        public string secondaryName { get; set; }
        public int fK_GroupId { get; set; }
    }

    public class SubCategoryDTO
    {
        public int Id { get; set; }
        public string primaryName { get; set; }
        public string secondaryName { get; set; }
        public int fK_CategoryId { get; set; }
        public int fK_TaxClassificationId { get; set; }
    }


    public class UnitDTO
    {
        public int Id { get; set; }
        public string primaryName { get; set; }
        public string secondaryName { get; set; }
    }

    public class CreateItemDTO
    {
        public int Id { get; set; }     
        public int ItemId { get; set; }     
        public int WIPId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string CategoryName { get; set; }
        public string UnitName { get; set; }
        public string SubCategoryName { get; set; }
        public int fk_UnitId { get; set; }
        public int? WarehouseId { get; set; }
        public int? LocatorId { get; set; }
        public string UnitPrimaryName { get; set; }
        public string UnitSecondaryName { get; set; }
        public int fK_CategoryId { get; set; }
        public string CategoryPrimaryName { get; set; }
        public string CategorySecondaryName { get; set; }
        public int fK_SubCategoryId { get; set; }
        public int? PartsIssueId { get; set; }
        public int? AccountType { get; set; }
        public string SubCategoryPrimaryName { get; set; }
        public string SubCategorySecondaryName { get; set; }
        public string? Status { get; set; }
        public decimal RequestQuantity { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UsedQuantity { get; set; }
        public decimal? Price { get; set; }     
        public decimal? CostPrice { get; set; }     
        public decimal? SalePrice { get; set; }     
        public decimal? Discount { get; set; }
        public string? StatusText { get; set; }
        public string StatusCode { get; set; }
        public string? StatusPrimaryName { get; set; }
        public string? StatusSecondaryName { get; set; }
    }

    public class BaseItemDTO
    {
        public int Id { get; set; }
        public int WIPId { get; set; }
        public int? RequestId { get; set; }
        public int ItemId { get; set; }
        public int fk_UnitId { get; set; } 
        public int? WarehouseId { get; set; } 
        public int? LocatorId { get; set; } 
        public decimal RequestQuantity { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UsedQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public int? ModifyBy { get; set; }
        public int? AccountType { get; set; }
    }
    public class ReturnDetailsDTO
    {
        public int WIPId { get; set; }
        public int? RequestId { get; set; }
        public int ItemId { get; set; }
        public int fk_UnitId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }

    }

    public class WarehouseDTO
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int? FK_ParentWarehouseId { get; set; }
        public int FK_AccountId { get; set; }
        public int FK_WarehouseTypeId { get; set; }
        public int FK_BranchId { get; set; }
        public string? WorkshopBranchIds { get; set; }
        public bool IsLocatorEnabled { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetWarehouseDTO : WarehouseDTO
    {
        public List<int> WorkshopBranchIds { get; set; } = new();
     
    }

    public class FilterLocatorDTO
    {
        public long ItemId { get; set; }
        public int? Fk_UnitId { get; set; }
        public int? Fk_WarehouseId { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        /// <summary>
        /// CSV of statuses that affect stock, e.g. "2" or "2,5"
        /// </summary>
        public string StatusCsv { get; set; } = "2";
    }

 

    public class AvailableLocatorQtyDTO
    {
        public int? LocatorId { get; set; }
        public string? LocatorCode { get; set; }

        public decimal OnHandBaseQty { get; set; }
        public decimal OnHandQtyInUnit { get; set; }
        public int? RequestedUnitId { get; set; }
        public string? RequestedUnitPrimaryName { get; set; }
        public string? RequestedUnitSecondaryName { get; set; }
        public decimal? FactorUsed { get; set; }
    }

}
