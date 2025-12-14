using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using System.Collections.Generic;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LabourRateController : ControllerBase
    {
        private readonly ILabourRateService _service;
        public LabourRateController(ILabourRateService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<LabourRateDTO>>> GetAll([FromQuery] string? Name, string Language = "en", int? PageNumber = 0)
        {
            var result = await _service.GetAllAsync(Name, Language, PageNumber);
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<LabourRateDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return new LabourRateDTO();
            return Ok(result);
        }

        //[HttpGet("GetByGroupId/{id}")]
        //public async Task<ActionResult<LabourRateDTO>> GetByGroupId(int id)
        //{
        //    var result = await _service.GetByGroupIdAsync(id);
        //    if (result == null) return NotFound();
        //    return Ok(result);
        //}

        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] CreateLabourRateDTO dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { id });
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update([FromBody] UpdateLabourRateDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new { Updated = updated });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> Delete([FromBody] DeleteLabourRateDTO dto)
        {
            var deleted = await _service.DeleteAsync(dto);
            return Ok(new { Id = deleted });
        }
    }
}
