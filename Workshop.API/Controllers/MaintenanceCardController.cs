using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    
    public class MaintenanceCardController : Controller
    {
        private readonly IMaintenanceCardService _service;

        public MaintenanceCardController(IMaintenanceCardService service)
        {
            _service = service;
        }

        [HttpPost("InsertDMaintenanceCard")]
        public async Task<IActionResult> InsertDMaintenanceCard([FromBody] MaintenanceCardDTO maintenanceCard)
        {
            await _service.InsertDMaintenanceCardAsync(maintenanceCard);
            return Ok();
        }

        [HttpDelete("DeleteDMaintenanceCard")]
        public async Task<IActionResult> DeleteDMaintenanceCard(int movementId)
        {
            await _service.DeleteDMaintenanceCardAsync(movementId);
            return Ok();
        }

        [HttpGet("GetDMaintenanceCardsByMovementId")]
        public async Task<IActionResult> GetDMaintenanceCardsByMovementId(int movementId)
        {
            var cards = await _service.GetDMaintenanceCardsByMovementIdAsync(movementId);
            return Ok(cards);
        }

        [HttpGet("GetDMaintenanceCardsByMasterId")]
        public async Task<IActionResult> GetDMaintenanceCardsByMasterId(Guid masterId)
        {
            var cards = await _service.GetDMaintenanceCardsByMasterIdAsync(masterId);
            return Ok(cards);
        }

        [HttpPost("UpdateDMaintenanceCard")]

        public async Task<IActionResult> UpdateDMaintenanceCard([FromBody] MaintenanceCardDTO maintenanceCard)
        {
            await _service.UpdateDMaintenanceCardAsync(maintenanceCard);
            return Ok();
        }

    }
}
