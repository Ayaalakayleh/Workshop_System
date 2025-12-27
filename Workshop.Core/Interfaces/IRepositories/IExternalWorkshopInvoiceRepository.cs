using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

public interface IExternalWorkshopInvoiceRepository
{
    Task<List<WorkShopDefinitionDTO>> GetWorkshopDetailsAsync(WorkShopFilterDTO filter);
    Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsAsync(ExternalWorkshopInvoiceDetailsFilterDTO filter);
    Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsByWIPId(int? WIPId);
	Task<IEnumerable<WorkshopInvoice>> M_WorkshopInvoice_GetWorkshop(DateTime? fromDate, DateTime? toDate, int? customerId, int? vehicleId, int? projectId, int? companyId);
}