using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using System.Collections.Generic;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountDefinitionController : ControllerBase
    {
        private readonly IAccountDefinitionService _service;
        public AccountDefinitionController(IAccountDefinitionService service)
        {
            _service = service;
        }

        [HttpGet("Get/{CompanyId}")]
        public async Task<ActionResult<AccountDefinitionDTO>> Get(int CompanyId)
        {
            var result = await _service.GetAsync(CompanyId);
            if (result == null) return new AccountDefinitionDTO();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] AccountDefinitionDTO dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { Id = id });
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] AccountDefinitionDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new { Updated = updated });
        }

       
    }
}
