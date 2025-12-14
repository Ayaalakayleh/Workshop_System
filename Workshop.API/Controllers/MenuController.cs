using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using System.Collections.Generic;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _service;
        public MenuController(IMenuService service)
        {
            _service = service;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<MenuDTO>>> GetAll([FromQuery] string GroupCode="", string Name="", int? PageNumber = 0)
        {
            var result = await _service.GetAllAsync(GroupCode, Name, PageNumber);
            return Ok(result);
        }
        
        [HttpGet("GetAllMenuDDL")]
        public async Task<ActionResult<IEnumerable<MenuDTO>>> GetAllMenuDDL()
        {
            var result = await _service.GetAllMenuDDL();
            return Ok(result);
        }


        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<MenuDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return new MenuDTO();
            return Ok(result);
        }

        [HttpGet("GetMenuItemsById/{id}")]
        public async Task<ActionResult<IEnumerable<MenuGroupDTO>>> GetMenuItemsByIdAsync(int id)
        {
            var result = await _service.GetMenuItemsByIdAsync(id);
            if (result == null) return new List<MenuGroupDTO>();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] CreateMenuDTO dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { id });
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update([FromBody] UpdateMenuDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new { Updated = updated });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> Delete([FromBody] DeleteMenuDTO dto)
        {
            var deleted = await _service.DeleteAsync(dto);
            return Ok(new { Id = deleted });
        }
    }
}
