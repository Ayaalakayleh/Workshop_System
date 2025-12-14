using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClockingController : ControllerBase
    {
        private readonly IClockingService _service;
        public ClockingController(IClockingService service)
        {
            _service = service;
        }

        [HttpGet("Get")]
        public async Task<IActionResult> GetClocks()
        {
            var result = await _service.GetClocks();
            return Ok(result);
        }
        [HttpPost("Insert")]
        public async Task<IActionResult> InsertClock([FromBody] ClockingDTO clockingDTO)
        {
            var result = await _service.InsertClock(clockingDTO);
            return Ok(result);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> UpdateClock([FromBody] ClockingDTO clockingDTO)
        {
            var result = await _service.UpdateClock(clockingDTO);
            return Ok(result);
        }

        [HttpGet("Delete")]
        public async Task<IActionResult> DeleteClock([FromBody] int id)
        {
            var result = await _service.DeleteClock(id);
            return Ok(result);
        }

        [HttpPost("InsertClockBreak")]
        public async Task<IActionResult> InsertClockBreak([FromBody] ClockingBreakDTO clockingDTO)
        {
            var result = await _service.InsertClockBreak(clockingDTO);
            return Ok(result);
        }

        [HttpPost("UpdateClockBreak")]
        public async Task<IActionResult> UpdateClockBreak([FromBody] ClockingBreakDTO clockingDTO)
        {
            var result = await _service.UpdateClockBreak(clockingDTO);
            return Ok(result);
        }

        [HttpPost("GetClockById")]
        public async Task<IActionResult> GetClock([FromBody] int Id)
        {
            var result = await _service.GetClock(Id);
            return Ok(result);
        }

        [HttpPost("GetLastBreakByClockID")]
        public async Task<IActionResult> GetLastBreakByClockID([FromBody] int Id)
        {
            var result = await _service.GetLastBreakByClockID(Id);
            return Ok(result);
        }

        [HttpGet("GetAllClocksBreaksDDL")]
        public async Task<IActionResult> GetAllClocksBreaksDDL()
        {
            var result = await _service.GetAllClocksBreaksDDL();
            return Ok(result);
        }

        [HttpPost("GetAllPaged")]
        public async Task<IActionResult> GetAllClocksPaged([FromBody] ClockingFilterDTO filterDTO)
        {
            var result = await _service.GetClocksPaged(filterDTO);
            return Ok(result);
        }
        [HttpPost("GetAllBreaksPaged")]
        public async Task<IActionResult> GetAllClocksBreaksPaged([FromBody] ClockingFilterDTO filterDTO)
        {
            var result = await _service.GetClocksBreaksPaged(filterDTO);
            return Ok(result);
        }
        [HttpPost("GetBreaksByClockID")]
        public async Task<IActionResult> GetBreaksByClockID([FromBody] int ClockID)
        {
            var result = await _service.GetBreaksByClockID(ClockID);
            return Ok(result);
        }
        [HttpGet("GetClocksHistory")]
        public async Task<IActionResult> GetClocksHistory()
        {
            var result = await _service.GetClocksHistory();
            return Ok(result);
        }

        [HttpGet("GetClockingFilter")]
        public async Task<IActionResult> GetClockingFilter()
        {
            var result = await _service.GetClockingFilter();
            return Ok(result);
        }


    }
}
