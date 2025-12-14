using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using System.Collections.Generic;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RTSCodeController : ControllerBase
    {
        private readonly IRTSCodeService _service;
        public RTSCodeController(IRTSCodeService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<RTSCodeDTO>>> GetAll([FromQuery] string Name = "", string Code = "", int PageNumber = 0)
        {
            var result = await _service.GetAllAsync(Name, Code, PageNumber);
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<RTSCodeDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return new RTSCodeDTO();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] CreateRTSCodeDTO dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { id });
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update([FromBody] UpdateRTSCodeDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new { Updated = updated });
        }

        [HttpGet("DDL")]
        public async Task<ActionResult<IEnumerable<RTSCodeDTO>>> GetAllDDL()
        {
            var result = await _service.GetAllDDLAsync();
            return Ok(result);
        }

        [HttpPost("Delete")]
        public async Task<ActionResult<int>> Delete([FromBody] DeleteRTSCodeDTO dto)
        {
            var result = await _service.DeleteAsync(dto);
            return Ok(result);
        }


    }
}
