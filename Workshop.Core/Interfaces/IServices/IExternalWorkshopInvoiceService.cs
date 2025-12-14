using Workshop.Core.DTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

public interface IExternalWorkshopInvoiceService
{
    Task<List<WorkShopDefinitionDTO>> GetWorkshopDetailsAsync(WorkShopFilterDTO filter);
    Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsAsync(ExternalWorkshopInvoiceDetailsFilterDTO filter);
}
