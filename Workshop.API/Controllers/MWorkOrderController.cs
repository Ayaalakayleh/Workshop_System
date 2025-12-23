using Microsoft.AspNetCore.Mvc;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Interfaces.IServices;
using Workshop.Infrastructure;

namespace Workshop.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MWorkOrderController : ControllerBase
	{
		private readonly IMWorkOrderService _mWorkOrderService;

		public MWorkOrderController(IMWorkOrderService mWorkOrderService)
		{
			_mWorkOrderService = mWorkOrderService;
		}

		[HttpGet("GetMWorkOrderByID/{id}")]
		public async Task<IActionResult> GetMWorkOrderByID(int id)
		{

			var result = await _mWorkOrderService.GetMWorkOrderByIdAysnc(id);

			//if (result == null)
			//    return NotFound(new { IsSuccess = false, Message = "WorkOrder not found" });
			return Ok(result);
		}


		[HttpPost("GetMWorkOrders")]
		public async Task<IActionResult> GetMWorkOrders([FromBody] WorkOrderFilterDTO workOrderFilterDTO)
		{

			try
			{
				var workOrders = await _mWorkOrderService.GetMWorkOrdersAsync(workOrderFilterDTO);
				return Ok(workOrders);
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
			}
		}



		[HttpPost("InsertMWorkOrder")]
		public async Task<ActionResult<MWorkOrderDTO>> InsertMWorkOrder([FromBody] MWorkOrderDTO workOrder)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _mWorkOrderService.InsertMWorkOrderAsync(workOrder);
			return Ok(result);
		}


		[HttpPut("UpdateMWorkOrder")]
		public async Task<ActionResult<MWorkOrderDTO>> UpdateMWorkOrder([FromBody] MWorkOrderDTO workOrder)
		{
			try
			{
				if (!ModelState.IsValid)
					return BadRequest(new { IsSuccess = false, Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors) });

				// Check if damage exists before updating
				//var existingDamage = await _damageService.GetDamageByIdAsync(damage.Id);
				//if (existingDamage == null)
				//    return NotFound(new { IsSuccess = false, Message = "Damage not found" });


				var result = await _mWorkOrderService.UpdateMWorkOrderAsync(workOrder);

				return Ok(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new { IsSuccess = false, Message = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteMWorkOrder(int id)
		{
			try
			{
				if (id <= 0)
					return BadRequest(new { IsSuccess = false, Message = "Invalid workOrder ID" });

				var success = await _mWorkOrderService.DeleteMWorkOrderAsync(id);

				//if (!success)
				//    return NotFound(new { IsSuccess = false, Message = "WorkOrder not found or could not be deleted" });

				return Ok(new
				{
					IsSuccess = true,
					Message = "WorkOrder deleted successfully"
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

		[HttpPut("UpdateWorkOrderKM")]
		public async Task<IActionResult> UpdateWorkOrderKM(int workOrderId, decimal receivedKM)
		{
			await _mWorkOrderService.UpdateWorkOrderKMAsync(workOrderId, receivedKM);
			return Ok();
		}

		[HttpPut("UpdateWorkOrderStatus")]
		public async Task<IActionResult> UpdateWorkOrderStatus(int workOrderId, int statusId, decimal totalCost = 0)
		{
			var result = await _mWorkOrderService.UpdateWorkOrderStatusAsync(workOrderId, statusId, totalCost);
			return Ok(result);
		}

		[HttpPut("UpdateMAccidentStatus")]
		public async Task<IActionResult> UpdateMAccidentStatus([FromBody] InsuranceClaimHistory insuranceClaimHistory)
		{
			var result = await _mWorkOrderService.UpdateMAccidentStatusAsync(insuranceClaimHistory);

			if (result == 1)
			{
				return Ok(new { IsSuccess = true, Message = "InsuranceClaimHistory was added successfully", Result = result });
			}
			else
			{
				return StatusCode(500, new { IsSuccess = false });
			}
		}

		[HttpGet("GetMWorkOrderByMasterId/{id}")]
		public async Task<IActionResult> GetMWorkOrderByMasterId(Guid id)
		{
			var result = await _mWorkOrderService.GetMWorkOrderByMasterId(id);

			return Ok(result);
		}

		[HttpPut("UpdateWorkOrderInvoicingStatus")]
		public async Task<IActionResult> UpdateWorkOrderInvoicingStatus(int workOrderId)
		{
			await _mWorkOrderService.UpdateWorkOrderInvoicingStatus(workOrderId);
			return Ok();
		}

		[HttpPut("FixWorkOrder")]
		public async Task<IActionResult> FixWorkOrder(int workOrderId, bool isFix)
		{
			await _mWorkOrderService.FixWorkOrder(workOrderId, isFix);
			return Ok();
		}
		[HttpPost("UniqueAccidentNo")]
		public async Task<IActionResult> M_UniqueAccidentNo(long DamageId, string ReportNo)
		{
			var result = await _mWorkOrderService.UniqueAccidentNo(DamageId, ReportNo);
			return Ok(result);
		}
		[HttpGet("WorkOrderDetails/{DamageId}")]
		public async Task<IActionResult> M_WorkOrderDetails_GetByWorkOrderID(int DamageId)
		{
			var result = await _mWorkOrderService.M_WorkOrderDetails_GetByWorkOrderID(DamageId);
			return Ok(result);
		}



		[HttpPost("WorkOrderReport_Insert")]
		public async Task<IActionResult> WorkOrderReport_Insert([FromBody] List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument)
		{
			await _mWorkOrderService.WorkOrderReport_Insert(WorkOrderDocument);
			return Ok();
		}
		[HttpPost("WorkOrderDetailsDoc_Insert")]
		public async Task<IActionResult> WorkOrderDetalsDoc_Insert([FromBody] List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument)
		{
			await _mWorkOrderService.WorkOrderDetalsDoc_Insert(WorkOrderDocument);
			return Ok();
		}
		[HttpPost("WorkOrderDetails_Insert")]
		public async Task<ActionResult<MWorkOrderDetail>> WorkOrderDetails_Insert([FromBody] MWorkOrderDetail WorkOrdert)
		{
			var result = await _mWorkOrderService.WorkOrderDetails_Insert(WorkOrdert);
			return Ok(result);
		}
		[HttpPost("WorkOrderDetails_Update")]
		public async Task<ActionResult<MWorkOrderDetail>> M_WorkOrderDetails_Update([FromBody] MWorkOrderDetail WorkOrdert)
		{
			var result = await _mWorkOrderService.M_WorkOrderDetails_Update(WorkOrdert);
			return Ok(result);
		}
		[HttpGet("WorkOrdersDetailsDocument/{WorkOrderDetailsId}")]
		public async Task<IActionResult> WorkOrdersDetailsDocument_Get(int WorkOrderDetailsId)
		{
			var result = await _mWorkOrderService.WorkOrdersDetailsDocument_Get(WorkOrderDetailsId);
			return Ok(result);
		}
		[HttpGet("WorkOrderReports_Get/{WorkOrderId}")]
		public async Task<ActionResult<MWorkOrdersDetailsDocument>> WorkOrderReports_Get(int WorkOrderId)
		{
			var result = await _mWorkOrderService.WorkOrderReports_Get(WorkOrderId);
			return Ok(result);
		}
		[HttpDelete("DeleteReportByWorkOrderId/{WorkOrderId}")]
		public async Task<IActionResult> DeleteReportByWorkOrderId(int WorkOrderId)
		{
			await _mWorkOrderService.DeleteReportByWorkOrderId(WorkOrderId);
			return Ok();
		}

		[HttpDelete("DeleteMWorkOrderDetails/{id}")]
		public async Task<IActionResult> DeleteMWorkOrderDetailsAsync(int id)
		{
			try
			{
				if (id <= 0)
					return BadRequest(new { IsSuccess = false, Message = "Invalid workOrder ID" });

				var success = await _mWorkOrderService.DeleteMWorkOrderDetailsAsync(id);

				return Ok(new
				{
					IsSuccess = true,
					Message = "WorkOrder Details deleted successfully"
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

		[HttpDelete("DeleteMWorkOrderDetailsDoc/{id}")]
		public async Task<IActionResult> DeleteMWorkOrderDetailsDocAsync(int id)
		{
			try
			{
				if (id <= 0)
					return BadRequest(new { IsSuccess = false, Message = "Invalid workOrder ID" });

				var success = await _mWorkOrderService.DeleteMWorkOrderDetailsDocAsync(id);

				return Ok(new
				{
					IsSuccess = true,
					Message = "WorkOrder Details Doc deleted successfully"
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
		[HttpGet("CheckAccidentNo/{accidentNo}")]
		public async Task<IActionResult> CheckAccidentNo(string accidentNo)
		{
			try
			{
				var result = await _mWorkOrderService.CheckAccidentNo(accidentNo);

				return Ok(result);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(new
				{
					IsSuccess = false,
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					IsSuccess = false,
					Message = "Internal Server Error",
					Details = ex.Message
				});
			}
		}

		[HttpGet("GetWorkshopWorkOrdersStatus")]
		public async Task<ActionResult<List<WorkshopWorkOrderStatusReportDTO>>> GetWorkshopWorkOrdersStatus(int? vehicleId, DateTime? fromDate, DateTime? toDate, int? externalVehicleId)
		{
			var data = await _mWorkOrderService.GetWorkshopWorkOrdersStatus(vehicleId, fromDate, toDate, externalVehicleId);
			return Ok(data);
		}

		[HttpPut("WorkOrder_FinishWorkflow")]
		public async Task<ActionResult> WorkOrder_FinishWorkflow(int id, int status)
		{
			await _mWorkOrderService.WorkOrder_FinishWorkflow(id, status);
			return Ok(true);
		}
		[HttpPost("UpdateClaimStatus")]
		public async Task<ActionResult> UpdateClaimStatus([FromBody] InsuranceClaimHistory model)
		{
			await _mWorkOrderService.UpdateClaimStatus(model);
			return Ok(true);
		}
		[HttpGet("WorkOrderClaimHistory/{workOrderId}")]
		public async Task<ActionResult<List<InsuranceClaimHistory>>> WorkOrderClaimHistory(int workOrderId)
		{
			var data = await _mWorkOrderService.GetClaimHistory(workOrderId);
			return Ok(data);
		}
		[HttpGet("WorkOrderClaimFile/{workOrderId}")]
		public async Task<ActionResult<ClaimFileDTO>> GetClaimFile(int workOrderId)
		{
			var data = await _mWorkOrderService.GetClaimFile(workOrderId);
			return Ok(data);
		}
		[HttpGet("GetVehicleWithOpenClaim")]
		public async Task<ActionResult<List<InsuranceClaimHistory>>> GetVehicleWithOpenClaim(int CompanyId)
		{
			var data = await _mWorkOrderService.GetVehicleWithOpenClaim(CompanyId);
			return Ok(data);
		}
		[HttpGet("GetInsuranceClaimStatistaic")]
		public async Task<ActionResult<InsuranceClaimStatistaic>> GetInsuranceClaimStatistaic(int companyId)
		{
			var data = await _mWorkOrderService.GetInsuranceClaimStatistaic(companyId);
			return Ok(data);
		}
		[HttpGet("GetEarningsFromAccidentClaim")]
		public async Task<ActionResult<List<WorkOrderClaimEarnings>>> GetEarningsFromAccidentClaim(int CompanyId, string language)
		{
			var data = await _mWorkOrderService.GetEarningsFromAccidentClaim(CompanyId);
			return Ok(data);
		}
		[HttpGet("M_Claim_GetCustomerwithDeductibleAmount")]
		public async Task<ActionResult<List<AmountFromClientsForAccidents>>> M_Claim_GetCustomerwithDeductibleAmount(int CompanyId, string language)
		{
			var data = await _mWorkOrderService.M_Claim_GetCustomerwithDeductibleAmount(CompanyId);
			return Ok(data);
		}
		[HttpPost("UpdateClaimAmountReceivedDate")]
		public async Task<ActionResult> UpdateClaimAmountReceivedDate([FromBody] MWorkOrderDTO model)
		{
			await _mWorkOrderService.UpdateClaimAmountReceivedDate(model);
			return Ok(true);
		}

		[HttpGet("CheckAccidentCountPerCustomer")]
		public async Task<ActionResult<int>> CheckAccidentCountPerCustomer(int CustomerId)
		{
			var data = await _mWorkOrderService.CheckAccidentCountPerCustomer(CustomerId);
			return Ok(data);
		}
		[HttpGet("M_WorkOrderStatus_Back")]
		public async Task<ActionResult<int>> M_WorkOrderStatus_Back(int WorkOrderId, int Status)
		{
			var result = await _mWorkOrderService.M_WorkOrderStatus_Back(WorkOrderId, Status);
			return Ok(result);
		}
		[HttpGet("M_SendToLegalAfter3Weeks")]
		public async Task<ActionResult<int>> M_SendToLegalAfter3Weeks()
		{
			var result = await _mWorkOrderService.M_SendToLegalAfter3Weeks();
			return Ok(result);
		}
		[HttpGet("M_TaqdeeratDocs_GetById")]
		public async Task<ActionResult<List<MTaqdeeratDocumentDTO>>> M_TaqdeeratDocs_GetById(int WorkOrderId)
		{
			var data = await _mWorkOrderService.M_TaqdeeratDocs_GetById(WorkOrderId);
			return Ok(data);
		}
		[HttpPost("M_AddTaqdeeratDoc")]
		public async Task<ActionResult<int>> M_AddTaqdeeratDoc([FromBody] MTaqdeeratDocumentDTO model)
		{
			var result = await _mWorkOrderService.M_AddTaqdeeratDoc(model);
			return Ok(result);
		}
		[HttpPost("M_TaqDocs_Delete")]
		public async Task<ActionResult<int>> M_TaqDocs_Delete(int WorkOrderId)
		{
			var result = await _mWorkOrderService.M_TaqDocs_Delete(WorkOrderId);
			return Ok(result);
		}
		[HttpGet("GetAllWorkflowStatus")]
		public async Task<ActionResult<List<WorkOrderStatusDTO>>> GetAllWorkflowStatus()
		{
			var data = await _mWorkOrderService.GetAllWorkflowStatus();
			return Ok(data);
		}

		[HttpPost("M_SaveStatusRoleId")]
		public async Task<ActionResult<int>> M_SaveStatusRoleId([FromBody] List<StatusRoleViewModel> liStatusRole)
		{
			var result = await _mWorkOrderService.M_SaveStatusRoleId(liStatusRole);
			return Ok(result);
		}


		[HttpGet("WorkOrders_VehicleWorkOrderStatus")]
		public async Task<ActionResult<bool>> WorkOrders_VehicleWorkOrderStatus(int VehicleId)
		{
			var result = await _mWorkOrderService.WorkOrders_VehicleWorkOrderStatus(VehicleId);
			return Ok(result);
		}
		[HttpGet("Get_InsuranceDetails")]
		public async Task<ActionResult<List<WorkOrderInsuranceDetails>>> Get_InsuranceDetails(int companyId, DateTime? fromDate, DateTime? toDate)
		{
			var data = await _mWorkOrderService.GetInsuranceDetails(companyId, fromDate, toDate);
			return Ok(data);
		}
		[HttpGet("GetWorkOrdersSummeryByVehicleId")]
		public async Task<IActionResult> GetWorkOrdersSummeryByVehicleId(int vehicleId, int companyId)
		{
			var result = await _mWorkOrderService.GetWorkOrdersSummeryByVehicleIdAsync(vehicleId, companyId);
			if (result == null || !result.Any())
				return NotFound("No work orders found for this vehicle.");

			return Ok(result);
		}
		[HttpGet("GetLastMaintenanceMovementStrike")]
		public async Task<IActionResult> GetLastMaintenanceMovementStrike(int vehicleId)
		{
			var strikes = await _mWorkOrderService.GetLastMaintenanceMovementStrikeAsync(vehicleId);
			return Ok(strikes);
		}

	}
}