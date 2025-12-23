using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MaintenanceHistoryController : ControllerBase
	{
		private readonly IMaintenanceHistoryService _service;
		public MaintenanceHistoryController(IMaintenanceHistoryService service)
		{
			_service = service;
		}

		[HttpGet("GetMaintenanceHistoryByVehicleId")]
		public async Task<IActionResult> GetMaintenanceHistoryByVehicleId(
		   int vehicleId,
		   DateTime? fromDate = null,
		   DateTime? toDate = null)
		{
			var result = await _service.GetMaintenanceHistoryByVehicleIdAsync(
				vehicleId, fromDate, toDate
			);

			if (result == null || !result.Any())
				return NotFound("No maintenance history found for the given parameters.");

			return Ok(result);
		}

	}
}
