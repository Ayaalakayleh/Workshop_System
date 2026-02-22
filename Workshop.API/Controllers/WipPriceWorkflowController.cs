using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class WipPriceWorkflowController : ControllerBase
    {
        private readonly IWipPriceWorkflowService _service;

        public WipPriceWorkflowController(IWipPriceWorkflowService service)
        {
            _service = service;
        }

        [HttpGet("PendingLines/{wipId}")]
        public async Task<IActionResult> GetPendingLines(int wipId)
        {
            var data = await _service.WIP_Items_GetPriceWorkflow(wipId);
            return Ok(data); 
        }

        [HttpPost("SetPending")]
        public async Task<IActionResult> SetPending([FromBody] ApplyWipPriceWorkflowResult req)
        {
            if (req == null) return BadRequest();
            var data = await _service.WIP_Items_SetPriceWorkflowPending(req);
            return Ok(data);
        }

        [HttpPost("Finish")]
        public async Task<IActionResult> Finish([FromBody] FinishWipPriceWorkflowRequest req)
        {
            if (req == null) return BadRequest();

            var ok = await _service.WIP_Items_FinishPriceWorkflow(req);
            return Ok(ok);
        }
    }
}
