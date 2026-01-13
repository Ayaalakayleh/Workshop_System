using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceWorkflowController : ControllerBase
    {
        private readonly IPriceWorkflowService _service;
        public PriceWorkflowController(IPriceWorkflowService service)
        {
            _service = service;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get(PriceWorkflowDTO dto)
        {
            var data = await _service.GetAsync(dto);
            return Ok(data);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<PriceWorkflowDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return new PriceWorkflowDTO();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] PriceWorkflowDTO dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { Id = id });
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] PriceWorkflowDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new { Updated = updated });
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete([FromBody] int Id)
        {
            var id = await _service.DeleteAsync(Id);
            return Ok(id);
        }

    }
}
