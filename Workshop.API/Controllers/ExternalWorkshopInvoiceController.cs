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
}
