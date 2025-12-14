using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs.ExternalWorkshopExp;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExternalWorkshopExpController : ControllerBase
    {
        private readonly IExternalWorkshopExpService _externalWorkshopExpService;

        public ExternalWorkshopExpController(IExternalWorkshopExpService externalWorkshopExpService)
        {
            _externalWorkshopExpService = externalWorkshopExpService;
        }

        [HttpPost("GetExternalWorkshopExp")]
        public async Task<ActionResult<IEnumerable<MExternalWorkshopExpDTO>>> GetExternalWorkshopExp([FromBody] ExternalWorkshopExpFilterDTO filter)
        {
            try
            {
                var result = await _externalWorkshopExpService.ExternalWorkshopExpGetAsync(filter);
                return Ok(result);
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

        [HttpPost("ExternalWorkshopExpInsert")]
        public async Task<ActionResult> ExternalWorkshopExpInsert([FromBody] CreateExternalWorkshopExpDTO createDto)
        {
            try
            {
                var success = await _externalWorkshopExpService.InsertExternalWorkshopExpAsync(createDto);
                return success ? Ok() : StatusCode(500, "Failed to insert external workshop expense");
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

        [HttpGet("ExternalWorkshopExpGetDetailsById/{headerId}")]
        public async Task<ActionResult<IEnumerable<DExternalWorkshopExpDTO>>> ExternalWorkshopExpGetDetailsById(int headerId)
        {
            try
            {
                var result = await _externalWorkshopExpService.ExternalWorkshopExpGetDetailsByIdAsync(headerId);
                return Ok(result);
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

        [HttpGet("ExternalWorkshopExpGetById/{id}")]
        public async Task<ActionResult<MExternalWorkshopExpDTO>> ExternalWorkshopExpGetById(int id)
        {
            try
            {
                var result = await _externalWorkshopExpService.ExternalWorkshopExpGetByIdAsync(id);

                if (result == null)
                    return new MExternalWorkshopExpDTO();

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpPut("ExternalWorkshopExpDetailsUpdate")]
        public async Task<ActionResult> ExternalWorkshopExpDetailsUpdate([FromBody] List<DExternalWorkshopExpDTO> prData)
        {
            try
            {
                var success = await _externalWorkshopExpService.ExternalWorkshopExpDetailsUpdateAsync(prData);
                return success ? NoContent() : StatusCode(500, "Failed to update external workshop expense details");
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

        [HttpPost("ExcelMappingGet")]
        public async Task<ActionResult<IEnumerable<MExcelMappingDTO>>> ExcelMappingGet([FromBody] ExcelMappingFilterDTO filter)
        {
            try
            {
                var result = await _externalWorkshopExpService.ExcelMappingGetAsync(filter);
                return Ok(result);
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

        [HttpPost("ExcelMappingInsert")]
        public async Task<ActionResult> ExcelMappingInsert([FromBody] CreateExcelMappingDTO createDto)
        {
            try
            {
                var success = await _externalWorkshopExpService.ExcelMappingInsertAsync(createDto);
                return success ? Ok() : StatusCode(500, "Failed to insert excel mapping");
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

        [HttpPut("ExcelMappingUpdate")]
        public async Task<ActionResult> ExcelMappingUpdate([FromBody] UpdateExcelMappingDTO updateDto)
        {
            try
            {
                var success = await _externalWorkshopExpService.ExcelMappingUpdateAsync(updateDto);
                return success ? NoContent() : StatusCode(500, "Failed to update excel mapping");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpGet("ExcelMappingGetColumns")]
        public async Task<ActionResult<IEnumerable<ExcelMappingColumnDTO>>> ExcelMappingGetColumns()
        {
            try
            {
                var result = await _externalWorkshopExpService.ExcelMappingGetColumnsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("ExcelMappingGetDetailsById")]
        public async Task<ActionResult<IEnumerable<DExcelMappingDTO>>> ExcelMappingGetDetailsById(
            [FromQuery] int? id,
            [FromQuery] int? workshopId)
        {
            try
            {
                var result = await _externalWorkshopExpService.ExcelMappingGetDetailsByIdAsync(id, workshopId);
                return Ok(result);
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

        [HttpPost("ExternalWorkshopExpReport")]
        public async Task<ActionResult<IEnumerable<ExternalWorkshopExpReportDTO>>> ExternalWorkshopExpReport([FromBody] ExternalWorkshopExpReportFilterDTO filter)
        {
            try
            {
                if (filter.CompanyId <= 0)
                    return BadRequest("CompanyId is required");

                var result = await _externalWorkshopExpService.ExternalWorkshopExpReportAsync(filter);
                return Ok(result);
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
    }
}