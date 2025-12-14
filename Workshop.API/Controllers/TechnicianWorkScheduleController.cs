using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using System.Collections.Generic;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianWorkScheduleController : ControllerBase
    {
        private readonly ITechnicianWorkScheduleService _service;
        public TechnicianWorkScheduleController(ITechnicianWorkScheduleService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<TechnicianWorkScheduleDTO>>> GetAll([FromQuery] int workshopId, string Name = "", DateTime? Date=null, string language="en", int PageNumber = 0)
        {
            var result = await _service.GetAllAsync(workshopId, Name, Date, language, PageNumber);
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<TechnicianWorkScheduleDTO>> GetById(int id, string language)
        {
            var result = await _service.GetByIdAsync(id, language);
            if (result == null) return new TechnicianWorkScheduleDTO();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] CreateTechnicianWorkScheduleDTO dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { id });
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update([FromBody] UpdateTechnicianWorkScheduleDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new { Updated = updated });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> Delete([FromBody] DeleteTechnicianWorkScheduleDTO dto)
        {
            var deleted = await _service.DeleteAsync(dto);
            return Ok(new { Id = deleted });
        }

        [HttpGet("GetTechnicians")]
        public async Task<ActionResult<IEnumerable<int>>> GetTechnicians()
        {
            var result = await _service.GetTechniciansFromWorkSchedulesAsync();
            return Ok(result);
        }
    }
}
