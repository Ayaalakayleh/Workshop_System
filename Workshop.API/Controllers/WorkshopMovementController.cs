using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Core.Interfaces.IServices;
using Workshop.Core.Services;

namespace Workshop.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class WorkshopMovementController : ControllerBase
	{
		private readonly IWorkshopMovementService _workshopMovementService;

		public WorkshopMovementController(IWorkshopMovementService workshopMovementService)
		{
			_workshopMovementService = workshopMovementService;
		}

		#region MovementIn
		[HttpPost("WorkshopInvoiceInsert")]
		public async Task<ActionResult> WorkshopInvoiceInsert([FromBody] MovementInvoice invoice)
		{
			try
			{
				var success = await _workshopMovementService.WorkshopInvoiceInsertAsync(invoice);

				return Ok(new
				{
					IsSuccess = success,
					Message = success ? "Invoice inserted successfully" : "Failed to insert invoice"
				});
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

		[HttpPost("DExternalWorkshopInvoiceInsert")]
		public async Task<ActionResult<int>> DExternalWorkshopInvoiceInsert([FromBody] MovementInvoice invoice)
		{
			var result = await _workshopMovementService.DExternalWorkshopInvoiceInsertAsync(invoice);
			return Ok(result);
		}

		[HttpGet("CheckVehicleMovementStatus/{vehicleId}")]
		public async Task<IActionResult> CheckVehicleMovementStatus(int vehicleId)
		{
			var result = await _workshopMovementService.CheckVehicleMovementStatusAsync(vehicleId);
			return Ok(result);
		}

		[HttpPost("InsertVehicleMovement")]
		public async Task<IActionResult> InsertVehicleMovementAsync([FromBody] VehicleMovement movement)
		{
			var result = await _workshopMovementService.InsertVehicleMovementAsync(movement);
			return Ok(result);
		}

		[HttpPost("InsertMWorkshopMovementStrikes")]
		public async Task<IActionResult> InsertMWorkshopMovementStrikes([FromBody] VehicleMovementStrike dto)
		{
			await _workshopMovementService.InsertMWorkshopMovementStrikesAsync((int)dto.MovementId, dto.Strike);
			return Ok(new { IsSuccess = true, Message = "Strikes inserted successfully" });
		}

		[HttpPost("InsertMovementDocument")]
		public async Task<IActionResult> InsertMovementDocument([FromBody] VehicleMovementDocument movmentDoc)
		{
			await _workshopMovementService.InsertMovementDocumentAsync(movmentDoc);
			return Ok(new { IsSuccess = true, Message = "Document inserted successfully" });
		}


        [HttpGet("GetVehicleChecklistLookup")]
        public async Task<IActionResult> GetVehicleChecklistLookup()
        {
            var result = await _workshopMovementService.GetVehicleChecklistLookup();
            return Ok(result);
        }
        [HttpGet("GetTyreChecklistLookup")]
        public async Task<IActionResult> GetTyreChecklistLookup()
        {
            var result = await _workshopMovementService.GetTyresChecklistLookup();
            return Ok(result);
        }
        [HttpPost("GetVehicleChecklistByMovementId")]
        public async Task<IActionResult> GetVehicleChecklistByMovementId([FromBody] int? movementId)
        {
            var result = await _workshopMovementService.GetVehicleChecklistByMovementId(movementId);
            return Ok(result);
        }
        [HttpPost("GetTyresChecklistByMovementId")]
        public async Task<IActionResult> GetTyresChecklistByMovementId([FromBody] int? movementId)
        {
            var result = await _workshopMovementService.GetTyresChecklistByMovementId(movementId);
            return Ok(result);
        }

        [HttpPost("InsertVehicleChecklist")]
        public async Task<IActionResult> InsertVehicleChecklist([FromBody] VehicleChecklist vehicleChecklist)
        {
            var result = await _workshopMovementService.InsertVehicleChecklist(vehicleChecklist);
            return Ok(result);
        }
        [HttpPost("InsertTyreChecklist")]
        public async Task<IActionResult> InsertTyreChecklist([FromBody] TyreChecklist tyreChecklist)
        {
            var result = await _workshopMovementService.InsertTyreChecklist(tyreChecklist);
            return Ok(result);
        }
        
        [HttpPost("UpdateVehicleChecklist")]
        public async Task<IActionResult> UpdateVehicleChecklist([FromBody] VehicleChecklist vehicleChecklist)
        {
            var result = await _workshopMovementService.UpdateVehicleChecklist(vehicleChecklist);
            return Ok(result);
        }
        [HttpPost("UpdateTyreChecklist")]
        public async Task<IActionResult> UpdateTyreChecklist([FromBody] TyreChecklist tyreChecklist)
        {
            var result = await _workshopMovementService.UpdateTyreChecklist(tyreChecklist);
            return Ok(result);
        }
        
        #endregion

        #region Movements
        [HttpPost("GetAllDWorkshopVehicleMovement")]
		public async Task<IActionResult> GetAllDWorkshopVehicleMovement([FromBody] WorkshopMovementFilter filter)
		{
			var result = await _workshopMovementService.GetAllDWorkshopVehicleMovement(filter);
			return Ok(result);
		}
		[HttpPost("GetAllDWorkshopVehicleMovementDDL")]
		public async Task<IActionResult> GetAllDWorkshopVehicleMovementDDL([FromBody] WorkshopMovementFilter filter)
		{
			var result = await _workshopMovementService.GetAllDWorkshopVehicleMovementDDL(filter);
			return Ok(result);
		}

		[HttpGet("GetVehicleMovementById/{movementId}")]
		public async Task<IActionResult> GetVehicleMovementById(int movementId)
		{
			var result = await _workshopMovementService.GetVehicleMovementByIdAsync(movementId);
			return Ok(result);
		}

		[HttpGet("GetMovementDocuments/{movementId}")]
		public async Task<IActionResult> GetMovementDocuments(int movementId)
		{
			var result = await _workshopMovementService.GetMovementDocumentsAsync(movementId);
			return Ok(result);
		}

		[HttpGet("GetWorkshopInvoiceByMovementId/{movementId}")]
		public async Task<IActionResult> GetWorkshopInvoiceByMovementId(int movementId)
		{
			var result = await _workshopMovementService.GetWorkshopInvoiceByMovementIdAsync(movementId);
			return Ok(result);
		}

		[HttpGet("GetLastVehicleMovementByVehicleId/{vehicleId}")]
		public async Task<IActionResult> GetLastVehicleMovementByVehicleId(int vehicleId)
		{
			var result = await _workshopMovementService.GetLastVehicleMovementByVehicleIdAsync(vehicleId);
			return Ok(result);
		}

		[HttpGet("GetVehicleMovementStrike/{movementId}")]
		public async Task<IActionResult> GetVehicleMovementStrike(int movementId)
		{
			var result = await _workshopMovementService.GetVehicleMovementStrikeAsync(movementId);
			return Ok(result);
		}
		[HttpGet("WorkshopMovement_GetFirstAgreementMovementByVehicleId")]
		public async Task<ActionResult<VehicleMovementDTO>> WorkshopMovement_GetFirstAgreementMovementByVehicleId(int VehicleId)
		{
			var data = await _workshopMovementService.WorkshopMovement_GetFirstAgreementMovementByVehicleId(VehicleId);
			return Ok(data);
		}
		[HttpGet("MovementStrikes_Get")]
		public async Task<ActionResult<string>> MovementStrikes_Get(int MovementId)
		{
			var result = await _workshopMovementService.MovementStrikes_GetAsync(MovementId);
			return Ok(result);
		}
		[HttpGet("UpdateMovementReplacement")]
		public async Task<IActionResult> UpdateMovementReplacement(int MovementId)
		{
			await _workshopMovementService.UpdateMovementReplacementAsync(MovementId);
			return Ok();
		}
		[HttpGet("VehicleMovement_GetLastMovementOutByWorkOrderId")]
		public async Task<ActionResult<VehicleMovement>> VehicleMovement_GetLastMovementOutByWorkOrderId(int WorkOrderId)
		{
			var result = await _workshopMovementService.VehicleMovement_GetLastMovementOutByWorkOrderIdAsync(WorkOrderId);
			if (result == null)
			{
				result = new VehicleMovement();

            }

            return Ok(result);
		}
        [HttpGet("GetWorkshopInvoiceByWorkOrderId/{workOrderId}")]
        public async Task<IActionResult> GetWorkshopInvoiceByWorkOrderId(int workOrderId)
        {
            var result = await _workshopMovementService.GetWorkshopInvoiceByWorkOrderId(workOrderId);
            return Ok(result);
        }


        #endregion

        #region MovementOut

        [HttpPut("UpdateVehicleMovementStatus")]
		public async Task<IActionResult> UpdateVehicleMovementStatusAync(int workshopId, Guid masterId)
		{
			await _workshopMovementService.UpdateVehicleMovementStatusAync(workshopId, masterId);
			return Ok();
		}
		#endregion
	}
}