using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkshopsController : ControllerBase
    {
        private readonly IWorkShopService _workShopService;

        public WorkshopsController(IWorkShopService workShopService)
        {
            _workShopService = workShopService;
        }

        [HttpGet("GetAllWorkshopsPage")]
        public async Task<ActionResult<IEnumerable<WorkshopListDTO>>> GetAllWorkshopsPage([FromQuery] WorkShopFilterDTO workShopFilterDTO)
        {
            try
            {
                var workshops = await _workShopService.GetAllWorkshopsPageAsync(workShopFilterDTO);
                return Ok(workshops);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<WorkShopDefinitionDTO>>> GetAll([FromQuery] int companyId,
            [FromQuery] int? branchId,
            [FromQuery] int? cityId = null,
            [FromQuery] string lang = "en"
            )
        {
            try
            {

                var workshops = await _workShopService.WorkshopGetAllAsync(companyId, branchId, cityId, lang);
                return Ok(workshops);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<WorkShopDefinitionDTO>> GetById(int id)
        {
            try
            {
                var workshop = await _workShopService.GetWorkshopByIdAsync(id);

                if (workshop == null)
                    return new WorkShopDefinitionDTO();

                return Ok(workshop);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("Add")]
        public async Task<ActionResult<int>> Add([FromBody] CreateWorkShopDTO createDto)
        {
            try
            {
                var id = await _workShopService.CreateWorkshopAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update([FromBody] UpdateWorkShopDTO updateDto)
        {
            try
            {
                await _workShopService.UpdateWorkshopAsync(updateDto);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, [FromBody] int updatedBy)
        {
            try
            {
                await _workShopService.DeleteWorkshopAsync(new DeleteWorkShopDTO
                {
                    Id = id,
                    UpdatedBy = updatedBy
                });
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetAllSimpleParentsWorkshop")]
        public async Task<ActionResult<IEnumerable<ParentWorkshopSimpleDTO>>> GetAllSipmleParentsWorkshop([FromQuery] int companyId, [FromQuery]string language = "en")
        {
            try
            {
                var workshops = await _workShopService.GetAllSipmleParentsWorkshop(companyId, language);
                return Ok(workshops);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}