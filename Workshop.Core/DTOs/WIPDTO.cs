using System;
using System.Text.Json.Serialization;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Domain.Enum;

namespace Workshop.Core.DTOs
{
    public abstract class WIPBaseDTO
    {
        public int VehicleId { get; set; }
        public int MovementId { get; set; }
        public int? PartsIssueId { get; set; }
        public string? PlateNumber { get; set; }
        public int? WorkOrderId { get; set; }
        public decimal? JobCardNo { get; set; }
        public int? Status { get; set; }
        public DateTime? WipDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Note { get; set; }
        public decimal? TotalParts { get; set; }
        public decimal? TotalTechnicians { get; set; }
        public decimal? Total { get; set; }
        public int? WorkshopId { get; set; } = 0;

        public bool IsExternal { get; set; } = false;
        public IEnumerable<WIPDTO>? List { get; set; }
        public IEnumerable<M_WIPServiceHistoryDTO>? History { get; set; }
        public IEnumerable<VehicleDefinitions>? VehicleDetails { get; set; }
        public VehicleTabDTO? VehicleTab { get; set; }
        public WIPOptionsDTO? Options { get; set; }
        public string? Services { get; set; }
        public string? Items { get; set; }
        public int? FK_WarehouseId { get; set; }
        public IEnumerable<BaseItemDTO>? ItemsList { get; set; }
        public IEnumerable<CreateWIPServiceDTO>? ServicesList { get; set; }
        public IEnumerable<WIPSChedule>? SCheduleList { get; set; }
        public AccountDTO? AccountDetails { get; set; }
        public WIPInvoiceDTO? InvoiceDetails { get; set; }
        public IEnumerable<WIPInvoiceDTO>? InvoiceDetailsList { get; set; }
        //public int? InternalCostTransaction { get; set; }
        //public int? ExternalCostCostTransaction { get; set; }
       
    }

    public class WIPDTO : WIPBaseDTO
    {
        public int Id { get; set; }
        public int? CreatedBy { get; set; }
        public int? ModifyBy { get; set; }
        public int? TotalPages { get; set; }
        public string? LookupCode { get; set; }
        public string? LookupPrimaryName { get; set; }
        public string? LookupSecondaryName { get; set; }
    }

    public class AccountDTO
    {
        public int WIPId { get; set; }
        public AccountTypeEnum AccountType { get; set; }
        public int? SalesType { get; set; }
        public int? CustomerId { get; set; }
        public int? CurrencyId { get; set; }
        public int? TermsId { get; set; }
        public int? Vat { get; set; }
        public int? TaxClassificationId { get; set; }  ///
        public AccountTypeEnum? PartialAccountType { get; set; }
        public int? PartialSalesType { get; set; }
        public int? PartialCustomerId { get; set; }
        public int? PartialCurrencyId { get; set; }
        public int? PartialTermsId { get; set; }
        public decimal? PartialVat { get; set; }
    }

    public class CreateWIPDTO : WIPBaseDTO
    {
        public int CreatedBy { get; set; }
    }

    public class UpdateWIPDTO : WIPBaseDTO
    {
        public int Id { get; set; }
        public int ModifyBy { get; set; }
        public int? ClosedBy { get; set; }
    }

    public class DeleteWIPDTO
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        //public int ModifyBy { get; set; }
    }

    public class FilterWIPDTO
    {
        public int? VehicleId { get; set; }
        public int? WorkOrderId { get; set; }
        public int? WorkshopId { get; set; }
        public int? WIPNo { get; set; }
        public int? Status { get; set; }
        public int? CustomerId { get; set; }
        public int? PageNumber { get; set; }
    }



    public class RTSWithTimeDTO 
    {
        public int CompanyId { get; set; }
        public int Make { get; set; }
        public int Model { get; set; }
        public int Year { get; set; }
        public int? Class { get; set; }
    }

    public class CreateWIPServiceDTO
    {
        public int Id { get; set; }
        public int? WIPId { get; set; }
        public string Code { get; set; }
        public string? StatusCode { get; set; }
        public string Description { get; set; }
        public string LongDescription { get; set; }
        public decimal StandardHours { get; set; }
        //public decimal Allowed { get; set; }
        public decimal Rate { get; set; }
        public decimal BaseRate { get; set; }
        public decimal Total { get; set; }
        public decimal? Discount { get; set; } = 0;
        public decimal TimeTaken { get; set; }
        public int Status { get; set; }
        public int? AccountType { get; set; }
        public string? StatusText { get; set; }
        public string? StatusPrimaryName { get; set; }
        public string? StatusSecondaryName { get; set; }
        public int? KeyId { get; set; }
        public int? tableId { get; set; }// The actual Id in WIP_Service table
        public bool IsExternal { get; set; }
        public bool IsFixed { get; set; } = false;


    }
    public sealed class TechnicianScheduleDTO
    {
        public int TechId { get; set; }
        //public string Code { get; set; } = string.Empty;

        public string TechName { get; set; } = string.Empty;
        public string TechSecondaryName { get; set; } = string.Empty;

        public string WorkingHours { get; set; } = "[]"; 
        public string Reservations { get; set; } = "[]";
        public List<WorkingHourDTO> WorkingHoursList { get; set; } = new();



        //public TimeSpan StartWorkingTime { get; set; }  
        //public TimeSpan EndWorkingTime { get; set; }     

        //public string WorkingHours { get; set; } = "00:00";
        //public string Assigned { get; set; } = "00:00";
        //public string Available { get; set; } = "00:00";
    }



    public class WIPGetItems
    {
        public int Id { get; set; }
        public List<BaseItemDTO>? Items { get; set; }

    }

    public class ReturnItems
    {
        public int WIPId { get; set; }
        public int RequestId { get; set; }
        public List<ReturnDetailsDTO>? Items { get; set; }

    }

    public class WIPIDs { 
        public int Id { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string? RequestDescription { get; set; }

    }

    public class GeneralRequest
    {
        public int Id { get; set; }
        public int WIPId { get; set; }
        public int CreatedBy { get; set; }
        public string? RequestDescription { get; set; }

    }

    public class WIPBookedServiceDTO
    {
        public int Id { get; set; }
        public int TechnicianId { get; set; }
        public int RTSId { get; set; }
        public DateTime Date { get; set; }

    }

    public class WIPSChedule
    {
        public int Id { get; set; }
        public int WIPId { get; set; }
        public int RTSId { get; set; }
        public int? KeyId { get; set; }
        public int TechnicianId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public decimal Duration { get; set; }
        public TimeSpan EndTime { get; set; }
        public int CompanyId { get; set; }
    }

    public class UpdateService
    {
        public int WIPId { get; set; }
        public int RTSId { get; set; }
        public int? KeyId { get; set; }
        public int Status { get; set; }

    }

    public class UpdatePartStatus
    {
        public int WIPId { get; set; }
        public int StatusId { get; set; }

    }

    public class VehicleTabDTO
    {
        public int WIPId { get; set; }
        public int VehicleId { get; set; }
        public string? VehServiceDesc { get; set; }
        public string? VehConcerns { get; set; }
        public string? VehAdvisorNotes { get; set; }
        public int? DepartmentId { get; set; }
        public int? CarPark { get; set; }
        public string? PlateNumber { get; set; }
        public string? ManufacturerPrimaryName { get; set; }
        public string? ManufacturerSecondaryName { get; set; }
        public string? VehicleModelPrimaryName { get; set; }
        public string? VehicleModelSecondaryName { get; set; }
        public string? VehicleClassPrimaryName { get; set; }
        public string? VehicleClassSecondaryName { get; set; }
        public int? ManufacturingYear { get; set; }
        public int? Color { get; set; }
        public string? ChassisNo { get; set; }
        public decimal? OdometerPrevious { get; set; }
        public decimal? OdometerCurrentIN { get; set; }
        public decimal? OdometerCurrentOUT { get; set; }
        public int? ManufacturerId { get; set; }
        public int? ModelId { get; set; }
        public int? ClassId { get; set; }
        public string? ColorName { get; set; }
    }

    public class OperatorDetailsDTO
    {
        public int Creating { get; set; }
        public int BookedIn { get; set; }
        public int Invoicing { get; set; }
        public int BookedOut { get; set; }
        public int Owning { get; set; }
    }

    public class WIPOptionsDTO
    {
        public int Id { get; set; } 
        public int WIPId { get; set; } 
        public bool PartialInvoicing { get; set; }
        public bool ReturnParts { get; set; }
        public bool RepeatRepair { get; set; }
        public bool UpdateDemand { get; set; }
    }

    public class M_WIPServiceHistoryDTO
    {
        public int WIPId { get; set; }
        public DateTime WIPDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal OdometerCurrentOUT { get; set; }
        public string? VehServiceDesc { get; set; }
        public int SalesType { get; set; }
        public string? SalesTypePrimaryName { get; set; }
        public string? SalesTypeSecondaryName { get; set; }
        public decimal? Total { get; set; }

        public IEnumerable<WIPServiceHistoryDetails_Labour>? HistoryLabours;
        public IEnumerable<WIPServiceHistoryDetails_Parts>? HistoryParts;
    }

    public class WIPServiceHistoryDetails_Labour
    {
        public int FK_WIPId { get; set; }
        public int? FK_RTSCode { get; set; }
        public string? RTSCodeText { get; set; }
        public string? CodeText { get; set; }
        public string? RTSPrimaryDescription { get; set; }
        public string? RTSSecondaryDescription { get; set; }
        public Decimal? Time { get; set; }
        public Decimal? Rate { get; set; }
        public Decimal? Discount { get; set; }
        public Decimal? Cost { get; set; }
        public int FK_TechnicianID { get; set; }
        public string? TechnicianPrimaryName { get; set; }
        public string? TechnicianSecondaryName { get; set; }
        public decimal? Total { get; set; }

    }

    public class WIPServiceHistoryDetails_Parts
    {
        public int FK_WIPId { get; set; }
        public int? Product { get; set; }
        //public string? ProductText { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
       // public decimal? Discount { get; set; }
        public decimal? Cost { get; set; }
        public decimal? Total { get; set; }

    }
    public class PartsRequest_RemainingQtyGroupDTO
    {
        public int WIPId { get; set; }
        public List<PartsRequest_RemainingQtyItemDTO> Items { get; set; } = new();
    }

    public class PartsRequest_RemainingQtyItemDTO
    {
        public int ItemId { get; set; }
        public int fk_UnitId { get; set; }
        public decimal RemainingQuantity { get; set; }
    }
    public class PartsRequest_RemainingQtyDTO: PartsRequest_RemainingQtyItemDTO
    {
        public int WIPId { get; set; }

    }
    public class TechnicianAvailabilty
    {

    }


    public class CloseWIPDTO
    {
        public int WIPId { get; set; }
        public int ClosedBy { get; set; }

    }
    public sealed class WorkingHourDTO
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = "";
        public decimal DurationMinutes { get; set; }
        public string EndTime { get; set; } = "";
    }
    public sealed class ReservationHoursDTO
    {
        public DateTime Date { get; set; }
        public string StartTime { get; set; } = "";
        public decimal DurationHours { get; set; }
        public string EndTime { get; set; } = "";
    }

    public class DeleteServiceDTO
    {
        public int Id { get; set; }
        public int WIPId { get; set; }
    }

    public class CheckWIPCountDTO
    {
        public int OpenWIPCount { get; set; }
        public int Previous { get; set; }
    }

    public class UpdateIssueIdDTO
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public int WIPId { get; set; }
    }

    public class UpdateWIPStatusDTO
    {
        public int WIPId { get; set; }
        public int StatusId { get; set; }
    }
    public class PrintInternalDTO
    {
        public IEnumerable<WipInvoiceDetailDTO>? WipInvoiceDetail { get; set; }
        public WIPInvoiceDTO? InvoiceDetails { get; set; }

    }
}
