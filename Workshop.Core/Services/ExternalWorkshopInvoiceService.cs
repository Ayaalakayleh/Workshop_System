using Workshop.Core.DTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

public class ExternalWorkshopInvoiceService : IExternalWorkshopInvoiceService
{
    private readonly IExternalWorkshopInvoiceRepository _repository;

    public ExternalWorkshopInvoiceService(IExternalWorkshopInvoiceRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WorkShopDefinitionDTO>> GetWorkshopDetailsAsync(WorkShopFilterDTO filter)
    {
        return await _repository.GetWorkshopDetailsAsync(filter);
    }

    public async Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsAsync(ExternalWorkshopInvoiceDetailsFilterDTO filter)
    {
        return await _repository.GetInvoiceDetailsAsync(filter);
    }
}
