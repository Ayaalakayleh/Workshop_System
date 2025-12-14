using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllowedTimesController : ControllerBase
    {
        private readonly IAllowedTimeService _allowedTimeService;

        public AllowedTimesController(IAllowedTimeService allowedTimeService)
        {
            _allowedTimeService = allowedTimeService;
        }

        [HttpGet("AllowedTimeGetAll")]
        public async Task<ActionResult<List<AllowedTimeListItemDTO>>> AllowedTimeGetAll([FromQuery] AllowedTimeFilterDTO filter)
        {
            try
            {
                var result = await _allowedTimeService.GetAllAsync(filter);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpGet("AllowedTimeGetById/{id}")]
        public async Task<ActionResult<AllowedTimeDTO>> AllowedTimeGetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { IsSuccess = false, Message = "Invalid ID" });

                var allowedTime = await _allowedTimeService.GetByIdAsync(id);

                if (allowedTime == null)
                    return new AllowedTimeDTO();

                return Ok(allowedTime);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return new AllowedTimeDTO();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPost("AllowedTimeCreate")]
        public async Task<ActionResult<int>> AllowedTimeCreate([FromBody] CreateAllowedTimeDTO createDto)
        {
            try
            {

                var id = await _allowedTimeService.CreateAsync(createDto);
                return Ok(new { Id = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpPut("AllowedTimeUpdate")]
        public async Task<ActionResult> AllowedTimeUpdate([FromBody] UpdateAllowedTimeDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { IsSuccess = false, Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors) });

                var result = await _allowedTimeService.UpdateAsync(updateDto);

                if (result <= 0)
                    return null;

                return Ok(new { IsSuccess = true, Message = "Allowed time updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { IsSuccess = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
            }
        }

        [HttpDelete("AllowedTimeDelete/{id}")]
        public async Task<ActionResult> AllowedTimeDelete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { IsSuccess = false, Message = "Invalid ID" });

                var result = await _allowedTimeService.DeleteAsync(id);

                if (result <= 0)
                    return null;

                return Ok(new { IsSuccess = true, Message = "Allowed time deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { IsSuccess = false, Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { IsSuccess = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
            }
        }
    }
}