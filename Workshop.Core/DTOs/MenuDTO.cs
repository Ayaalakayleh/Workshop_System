using System;
using System.Collections.Generic;

namespace Workshop.Core.DTOs
{
    public abstract class MenuBaseDTO
    {
        public string GroupCode { get; set; }
        public string GroupName { get; set; }
        public string PrimaryDescription { get; set; }
        public string SecondaryDescription { get; set; }
        public string PricingMethod { get; set; }
        public decimal? Price { get; set; } = 0;
        public decimal? TotalTime { get; set; } = 0;
        public DateTime? EffectiveDate { get; set; }
        public bool IsActive { get; set; } = true;
        public IEnumerable<MenuGroupDTO>? MenuGroup { get; set; }
    }

    public class MenuDTO : MenuBaseDTO
    {
        public int Id { get; set; }
        public int? RTSId { get; set; }
        public int? PartId { get; set; }
        public int? GroupId { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? TotalPages { get; set; }
        public string? ServicesToSave { get; set; }
        
    }

    public class CreateMenuDTO : MenuBaseDTO
    {
        public int? CreatedBy { get; set; }
    }

    public class UpdateMenuDTO : MenuBaseDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class DeleteMenuDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class FilterMenuDTO
    {
        public string? Name { get; set; }
        public string? GroupCode { get; set; }
        public int? PageNumber { get; set; }
    }

    public class MenuGroupDTO
    {
        public int Id { get; set; }
        public int FK_MenuId { get; set; }
        public int ItemId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? StandardHours { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Total { get; set; }
        public bool IsPart { get; set; }
    }



}
