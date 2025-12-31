using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DTechnicianController : ControllerBase
    {
        private readonly IDTechnicianService _service;
        public DTechnicianController(IDTechnicianService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] int workshopId, string Name = "", string Email = "", int PageNumber = 0)
        {
            var result = await _service.GetAllAsync(workshopId, Name, Email, PageNumber);
            return Ok(result);
        }
        [HttpGet("GetAllPIN")]
        public async Task<IActionResult> GetAllPIN([FromQuery] int workshopId, string Name = "", string Email = "", int PageNumber = 0)
        {
            var result = await _service.GetAllPINAsync(workshopId, Name, Email, PageNumber);
            return Ok(result);
        }

        [HttpGet("GetDDL")]
        public async Task<IActionResult> GetTechniciansDDL([FromQuery] int Id)
        {
            var result = await _service.GetTechniciansDDL(Id);
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            //if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] CreateDTechnicianDto dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { Id = id });
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateDTechnicianDto dto)
        {
            var updated = await _service.UpdateAsync(dto);
            //if (updated == 0) return NotFound();
            return Ok();
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteDTechnicianDto dto)
        {
            var deleted = await _service.DeleteAsync(dto);
            //if (deleted == 0) return NotFound();
            return Ok();
        }

        [HttpGet("GetAvailableTechniciansAsync")]
        public async Task<IActionResult> GetAvailableTechniciansAsync([FromQuery] DateTime date, decimal duration, int BranchId, bool trimPastIntervals = false)
        {
            var result = await _service.GetAvailableTechniciansAsync(date, duration, BranchId, trimPastIntervals);
            return Ok(result);
        }
    }
}
