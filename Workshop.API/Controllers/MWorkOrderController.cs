using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.Interfaces.IServices;

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

        //[HttpPost("InsertMWorkOrder")]
        //public async Task<IActionResult> InsertMWorkOrder([FromBody] MWorkOrderDTO workOrder)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(new { IsSuccess = false, Message = "Invalid model state", Errors = ModelState.Values.SelectMany(v => v.Errors) });

        //        var result = await _mWorkOrderService.InsertMWorkOrderAsync(workOrder);

        //        return Ok(new
        //        {
        //            IsSuccess = true,
        //            Message = "WorkOrder inserted successfully",
        //            Data = result
        //        });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { IsSuccess = false, Message = ex.Message });
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { IsSuccess = false, Message = ex.Message });
        //    }
        //}

        [HttpPost("InsertMWorkOrder")]
        public async Task<ActionResult<MWorkOrderDTO>> InsertMWorkOrder([FromBody] MWorkOrderDTO workOrder)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _mWorkOrderService.InsertMWorkOrderAsync(workOrder);
            return Ok(result); 
        }


        [HttpPut("UpdateMWorkOrder")]
        public async Task<IActionResult> UpdateMWorkOrder([FromBody] MWorkOrderDTO workOrder)
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

                return Ok(new
                {
                    IsSuccess = true,
                    Message = "WorkOrder updated successfully",
                    Data = result
                });
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
            await _mWorkOrderService.UpdateWorkOrderStatusAsync(workOrderId, statusId, totalCost);
            return Ok();
        }

        [HttpPut("UpdateMAccidentStatus")]
        public async Task<IActionResult> UpdateMAccidentStatus([FromBody] InsuranceClaimHistory insuranceClaimHistory)
        {
            var result = await _mWorkOrderService.UpdateMAccidentStatusAsync(insuranceClaimHistory);

            if (result == 1)
            {
                return Ok(new { IsSuccess = true, Message = "InsuranceClaimHistory was added successfully" });
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
    }
}