using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemsService _itemsService;

        public ItemsController(IItemsService itemsService)
        {
            _itemsService = itemsService;
        }

        [HttpPost("Create")]
        public async Task<ActionResult<int>> itemsCreate([FromBody] ItemsDTO dto)
        {
            try
            {
                var id = await _itemsService.WIPInsertItemsAsync(dto);
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

       
    }
}