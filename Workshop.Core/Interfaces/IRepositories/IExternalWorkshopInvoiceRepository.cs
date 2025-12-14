using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.DTOs;

public interface IExternalWorkshopInvoiceRepository
{
    Task<List<WorkShopDefinitionDTO>> GetWorkshopDetailsAsync(WorkShopFilterDTO filter);
    Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsAsync(ExternalWorkshopInvoiceDetailsFilterDTO filter);
}