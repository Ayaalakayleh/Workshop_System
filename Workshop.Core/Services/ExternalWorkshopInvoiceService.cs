using Microsoft.AspNetCore.Mvc;
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

    public async Task<List<ExternalWorkshopInvoiceDetailsDTO>> GetInvoiceDetailsByWIPId(int? WIPId)
    {
        return await _repository.GetInvoiceDetailsByWIPId(WIPId);
    }
	public async Task<IEnumerable<WorkshopInvoice>> M_WorkshopInvoice_GetWorkshop(DateTime? fromDate, DateTime? toDate, int? customerId, int? vehicleId, int? projectId, int? companyId)
	{
		try
		{
			return await _repository.M_WorkshopInvoice_GetWorkshop(fromDate, toDate, customerId, vehicleId, projectId, companyId);
		}
		catch (Exception ex)
		{
			throw new Exception("Error in M_WorkshopInvoice_GetWorkshop", ex);
		}
	}
}
