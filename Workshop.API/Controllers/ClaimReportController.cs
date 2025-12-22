using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ClaimReportController : ControllerBase
	{
		private readonly IClaimReportService _service;
		public ClaimReportController(IClaimReportService service)
		{
			_service = service;
		}

		[HttpGet("GetClaimsReport")]
		public async Task<IActionResult> GetClaimsReport()
		{
			try
			{
				var data = await _service.GetWorkshopClaimReportDataAsync();
				return Ok(data);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					message = ex.Message,
					inner = ex.InnerException?.Message,
					stack = ex.StackTrace
				});
			}
		}

	}
}
