using System;
using System.ComponentModel.DataAnnotations;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

namespace Workshop.Core.DTOs
{
    #region ExternalWorkshopInvoice DTOs

    public abstract class BaseExternalWorkshopInvoiceDTO
    {
        [Range(1, 12)]
        public int Month { get; set; } = 1;

        [Required]
        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Required]
        public DateTime GregorianFromDate { get; set; }

        [Required]
        public DateTime GregorianToDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ExternalWorkshopId { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal VatRate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int InvoiceTypeId { get; set; }
    }

    public class ExternalWorkshopInvoiceDTO : BaseExternalWorkshopInvoiceDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }

        //public List<WorkShopDefinitionDTO> WorkshopDefinitions { get; set; }
        public List<ExternalWorkshopInvoiceDetailsDTO> ExternalWorkshopInvoiceDetails { get; set; }
        //public List<TypeSalesPurchases> InvoiceTypes { get; set; }
    }

    public class CreateExternalWorkshopInvoiceDTO : BaseExternalWorkshopInvoiceDTO
    {
        [Required]
        public int CreatedBy { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateExternalWorkshopInvoiceDetailsDTO> InvoiceDetails { get; set; }
    }

    public class UpdateExternalWorkshopInvoiceDTO : BaseExternalWorkshopInvoiceDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int UpdatedBy { get; set; }

        public List<UpdateExternalWorkshopInvoiceDetailsDTO> InvoiceDetails { get; set; }
    }

    public class ExternalWorkshopInvoiceFilterDTO
    {
        public int? BranchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? WorkshopId { get; set; }
        public string? Language { get; set; }
    }

    public class ExternalWorkshopInvoiceListItemDTO : ExternalWorkshopInvoiceDTO
    {
        public int TotalCount { get; set; }
        public string BranchName { get; set; }
        public string ExternalWorkshopName { get; set; }
        public string InvoiceTypeName { get; set; }
    }

    #endregion

    #region ExternalWorkshopInvoiceDetails DTOs

    public abstract class BaseExternalWorkshopInvoiceDetailsDTO
    {
        [Required]
        [StringLength(50)]
        public string InvoiceNo { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal TotalInvoice { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ExternalWorkshopId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int WorkOrderId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ConsumptionValueOfSpareParts { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Vat { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DeductibleAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal PartsCost { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal LaborCost { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int WorkOrderNo { get; set; }

        [Required]
        [StringLength(200)]
        public string WorkOrderTitle { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int BranchId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int VehicleId { get; set; }
    }

    public class ExternalWorkshopInvoiceDetailsDTO : BaseExternalWorkshopInvoiceDetailsDTO
    {
        public int Id { get; set; }
        public int MovementId { get; set; }
        public Guid MasterId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string? BranchName { get; set; }
        public int? WIP { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class CreateExternalWorkshopInvoiceDetailsDTO : BaseExternalWorkshopInvoiceDetailsDTO
    {
        public int CreatedBy { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    public class UpdateExternalWorkshopInvoiceDetailsDTO : BaseExternalWorkshopInvoiceDetailsDTO
    {
        public int Id { get; set; }
        public int? UpdatedBy { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    public class ExternalWorkshopInvoiceDetailsFilterDTO
    {
        public int? ExternalWorkshopId { get; set; }
        public int? WorkshopId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ExternalWorkshopInvoiceDetailsListItemDTO : ExternalWorkshopInvoiceDetailsDTO
    {
        public int TotalCount { get; set; }
        public string ExternalWorkshopName { get; set; }
        public string BranchName { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string WorkOrderDescription { get; set; }
    }

    #endregion

}