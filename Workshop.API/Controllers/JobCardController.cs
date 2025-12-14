using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobCardController : ControllerBase
    {
        private readonly IJobCardService _service;
        public JobCardController(IJobCardService service)
        {
            _service = service;
        }

        [HttpGet("GetJobCardByMasterId")]
        public async Task<IActionResult> GetJobCardByMasterId(Guid id)
        {
            var result = await _service.GetJobCardByMasterIdAsync(id);
            return Ok(result);
        }
    }
}
