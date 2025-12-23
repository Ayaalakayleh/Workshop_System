using Workshop.Core.DTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

public interface IExternalWorkshopInvoiceService
{
    Task<List<WorkShopDefinitionDTO>> GetWorkshopDetailsAsync(WorkShopFilterDTO filter);
    Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsAsync(ExternalWorkshopInvoiceDetailsFilterDTO filter);
	Task<IEnumerable<WorkshopInvoice>> M_WorkshopInvoice_GetWorkshop(DateTime? fromDate, DateTime? toDate, int? customerId, int? vehicleId, int? projectId, int? companyId);

}
