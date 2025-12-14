using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriceMatrixController : Controller
    {
        /*
        public IActionResult Index()
        {
            return View();
        }
        */
        private readonly IPriceMatrixService _service;
        public PriceMatrixController(IPriceMatrixService service)
        {
            _service = service;
        }

        [HttpPost("GetAll")]
        public async Task<IActionResult> GetAll([FromBody] PriceMatrixFilter dto)
        {

            var result = await _service.GetAllAsync(dto);

            return Ok(result);
        }

        [HttpPost("GetPaged")]
        public async Task<IActionResult> GetPaged([FromBody] PriceMatrixFilter dto)
        {
            var result = await _service.GetAllPagedAsync(dto);
            return Ok(result);
        }

        [HttpPost("Get")]
        public async Task<IActionResult> GetById([FromBody] GetPriceMatrixDTO dto)
        {
            var result = await _service.GetAsync(dto);
            return Ok(result);
        }
        [HttpPost("GetById")]
        public async Task<IActionResult> GetById([FromBody] int Id)
        {
            var result = await _service.GetAsync(new GetPriceMatrixDTO { Id = Id });
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] CreatePriceMatrixDTO dto)
        {
            var result = await _service.AddAsync(dto);
            return Ok(result);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] UpdatePriceMatrixDTO dto)
        {
            var result = await _service.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete([FromBody] int Id)
        {
            var result = await _service.DeleteAsync(Id);
            return Ok(result);
        }

    }
}
