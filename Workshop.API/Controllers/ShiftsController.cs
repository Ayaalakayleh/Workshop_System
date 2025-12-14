using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpGet("ShiftGetAll")]
        public async Task<ActionResult<List<ShiftListItemDTO>>> ShiftGetAll([FromQuery] ShiftFilterDTO filter)
        {
            try
            {
                if (filter.CompanyId <= 0)
                    return BadRequest(new { IsSuccess = false, Message = "CompanyId is required" });

                var result = await _shiftService.GetAllAsync(filter);
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

        [HttpGet("GetAllShiftsDDL")]
        public async Task<ActionResult<List<ShiftListItemDTO>>> GetAllShiftsDDL()
        {
            try
            {
                var result = await _shiftService.GetAllShiftsDDLAsync();
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

        [HttpGet("ShiftGetById/{id}")]
        public async Task<ActionResult<ShiftDTO>> ShiftGetById(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { IsSuccess = false, Message = "Invalid ID" });

                var shift = await _shiftService.GetByIdAsync(id);

                if (shift == null)
                    return new ShiftDTO();

                return Ok(shift);
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

        [HttpPost("ShiftCreate")]
        public async Task<ActionResult<int>> ShiftCreate([FromBody] CreateShiftDTO createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { IsSuccess = false, Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors) });

                var id = await _shiftService.CreateAsync(createDto);
                return Ok(new { Id = id });
                //return CreatedAtAction(nameof(ShiftGetById), new { id }, new { IsSuccess = true, Id = id });
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

        [HttpPut("ShiftUpdate")]
        public async Task<ActionResult> ShiftUpdate([FromBody] UpdateShiftDTO updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { IsSuccess = false, Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors) });

                var result = await _shiftService.UpdateAsync(updateDto);

                //if (result <= 0)
                //    return NotFound(new { IsSuccess = false, Message = "Shift not found or could not be updated" });

                return Ok(new { IsSuccess = true, Message = "Shift updated successfully" });
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

        [HttpDelete("ShiftDelete/{id}")]
        public async Task<ActionResult> ShiftDelete(int id)
        {
            try
            {
                if (id <= 0)
                    return BadRequest(new { IsSuccess = false, Message = "Invalid ID" });

                var result = await _shiftService.DeleteAsync(id);

                if (result <= 0)
                    return NotFound(new { IsSuccess = false, Message = "Shift not found or could not be deleted" });

                return Ok(new { IsSuccess = true, Message = "Shift deleted successfully" });
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
                return StatusCode(500);
            }
        }
    }
}