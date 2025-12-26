using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

[ApiController]
[Route("api/[controller]")]
public class ExternalWorkshopInvoiceController : ControllerBase
{
    private readonly IExternalWorkshopInvoiceService _service;

    public ExternalWorkshopInvoiceController(IExternalWorkshopInvoiceService service)
    {
        _service = service;
    }

    [HttpGet("GetWorkshopDetails")]
    public async Task<ActionResult<List<WorkShopDefinitionDTO>>> GetWorkshopDetails([FromQuery] WorkShopFilterDTO filter)
    {
        var result = await _service.GetWorkshopDetailsAsync(filter);
        return Ok(result);
    }

    [HttpGet("GetInvoiceDetails")]
    public async Task<ActionResult<List<ExternalWorkshopInvoiceDetailsDTO>>> GetInvoiceDetails([FromQuery] ExternalWorkshopInvoiceDetailsFilterDTO filter)
    {
        var result = await _service.GetInvoiceDetailsAsync(filter);
        return Ok(result);
    }

    [HttpGet("GetInvoiceDetailsByWIPId")]
    public async Task<ActionResult<List<ExternalWorkshopInvoiceDetailsDTO>>> GetInvoiceDetailsByWIPId(int? WIPId)
    {
        var result = await _service.GetInvoiceDetailsByWIPId(WIPId);
        return Ok(result);
    }

	[HttpGet("WorkshopInvoice_GetWorkshop")]
	public async Task<ActionResult<IEnumerable<WorkshopInvoice>>> M_WorkshopInvoice_GetWorkshop([FromQuery] int companyId, [FromQuery] string fromDate, [FromQuery] string toDate, [FromQuery] int? customerId, [FromQuery] int? projectId,
	[FromQuery] int? vehicleId)
	{
		try
		{
			var result = await _service.M_WorkshopInvoice_GetWorkshop(
				DateTime.Parse(fromDate),
				DateTime.Parse(toDate),
				customerId,
				vehicleId,
				projectId,
				companyId
			);
			return Ok(result);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(ex.Message);
		}
		catch (Exception ex)
		{
			return StatusCode(500, ex.Message);
		}
	}

}
