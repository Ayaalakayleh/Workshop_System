using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RecallController : ControllerBase
	{
		private readonly IRecallService _service;
		public RecallController(IRecallService service)
		{
			_service = service;
		}


		[HttpPost("GetAll")]
		public async Task<ActionResult<IEnumerable<RecallDTO>>> GetAll([FromBody] FilterRecallDTO filterRecallDTO)
		{
			var result = await _service.GetAllAsync(filterRecallDTO);
			return Ok(result);
		}
		[HttpGet("GetAllDDL")]
		public async Task<ActionResult<IEnumerable<RecallDTO>>> GetAllDDL()
		{
			var result = await _service.GetAllDDLAsync();
			return Ok(result);
		}

		[HttpGet("GetById/{id}")]
		public async Task<ActionResult<RecallDTO>> GetById(int id)
		{
			var result = await _service.GetByIdAsync(id);
			if (result == null) return new RecallDTO();
			return Ok(result);
		}

		[HttpGet("GetActiveRecallsByChassis/{chassisNo}")]
		public async Task<ActionResult<ActiveRecallsByChassisResponseDto>> GetActiveRecallsByChassis(string chassisNo)
		{
			if (string.IsNullOrWhiteSpace(chassisNo))
			{
				return BadRequest("Chassis number is required.");
			}

			var result = await _service.GetActiveRecallsByChassisAsync(chassisNo);
			return Ok(result);
		}

		[HttpPost("Add")]
		public async Task<ActionResult> Add([FromBody] CreateRecallDTO dto)
		{
			var id = await _service.AddAsync(dto);
			return Ok(new { id = id });
		}

		[HttpPut("Update")]
		public async Task<ActionResult> Update([FromBody] UpdateRecallDTO dto)
		{
			var updated = await _service.UpdateAsync(dto);
			return Ok(new { Updated = updated });
		}

		[HttpPost("Delete")]
		public async Task<ActionResult> Delete([FromBody] DeleteRecallDTO dto)
		{
			var delete = await _service.DeleteAsync(dto);
			return Ok(new { deleted = delete });
		}
		[HttpPut("UpdateRecallVehicleStatus")]
		public async Task<IActionResult> UpdateRecallVehicleStatusAsync(string chassisNo, int statusId)
		{
			var result = await _service.UpdateRecallVehicleStatus(chassisNo, statusId);
			return Ok(result);

		}
        [HttpPost("GetActiveRecallsByChassisBulk")]
        public async Task<IActionResult> GetActiveRecallsByChassisBulk(List<string> chassisList)
        {
            var result = await _service.GetActiveRecallsByChassisBulkAsync(chassisList);
            return Ok(result);

        }
    }
}
