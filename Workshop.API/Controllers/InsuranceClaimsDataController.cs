using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InsuranceClaimsDataController : Controller
    {
        private readonly IInsuranceClaimsDataService _service;
        public InsuranceClaimsDataController(IInsuranceClaimsDataService service)
        {
            _service = service;
        }

        [HttpPost("InsuranceClaimsDataDataAsync")]
        public async Task<IActionResult> InsuranceClaimsDataDataAsync(InsuranceClaimsDataFilterDTO filterDTO)
        {
            try
            {
                var data = await _service.InsuranceClaimsDataDataAsync(filterDTO);
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
