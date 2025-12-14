using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _service;
        public LookupController(ILookupService service)
        {
            _service = service;
        }

        [HttpGet("Details/GetAll")]
        public async Task<IActionResult> GetAll_Details()
        {
            var result = await _service.GetAllDetailsAsync();
            return Ok(result);
        }

        [HttpGet]
        [Route("Details/GetByHeaderId")]
        public async Task<IActionResult> GetByHeaderId_Details([FromQuery] int headerId, int CompanyId)
        {
            var result = await _service.GetDetailsByHeaderIdAsync(headerId, CompanyId);
            //if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("Details/GetById")]
        public async Task<IActionResult> GetById_Details([FromQuery] int Id, int HeaderId, int CompanyId)
        {
            var result = await _service.GetDetailsByIdAsync(Id, HeaderId, CompanyId);
            //if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("Details/Add")]
        public async Task<IActionResult> Add_Details([FromBody] CreateLookupDetailsDTO dto)
        {
            var id = await _service.AddDetailsAsync(dto);
            return Ok(new { Id = id });
        }

        [HttpPut("Details/Update")]
        public async Task<IActionResult> Update_Details([FromBody] UpdateLookupDetailsDTO dto)
        {
            var updated = await _service.UpdateDetailsAsync(dto);
            return Ok(new { Updated = updated });
        }

        [HttpDelete("Details/Delete")]
        public async Task<IActionResult> Delete_Details([FromBody] DeleteLookupDetailsDTO dto)
        {
            var deleted = await _service.DeleteDetailsAsync(dto);
            return Ok(new { Deleted = deleted });
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll_Header([FromQuery] string language, int CompanyId)
        {
            try
            {
                var result = await _service.GetAllHeaderAsync(language, CompanyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString()); 
            }
        }

        [HttpGet("Header/GetById")]
        public async Task<IActionResult> GetById_Header([FromQuery] int id, int CompanyId)
        {
            var result = await _service.GetHeaderByIdAsync(id, CompanyId);
            //if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
