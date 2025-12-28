using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WIPController : ControllerBase
    {
        private readonly IWIPService _service;
        public WIPController(IWIPService service)
        {
            _service = service;
        }

        [HttpPost("GetAll")]
        public async Task<ActionResult<IEnumerable<WIPDTO>>> GetAll(FilterWIPDTO oFilter)
        {
            try {
                var result = await _service.GetAllAsync(oFilter);
                return Ok(result);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            }

        [HttpPost("GetAllInternalLabourLineAsync")]
        public async Task<ActionResult<IEnumerable<CreateWIPServiceDTO>>> GetAllInternalLabourLineAsync([FromBody]int WIPId)
        {
            var result = await _service.GetAllInternalLabourLineAsync(WIPId);
            return Ok(result);
        }

        [HttpPost("GetAllInternalPartsLineAsync")]
        public async Task<ActionResult<IEnumerable<CreateItemDTO>>> GetAllInternalPartsLineAsync([FromBody] int WIPId)
        {
            var result = await _service.GetAllInternalPartsLineAsync(WIPId);
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<WIPDTO>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return new WIPDTO();
            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<ActionResult> Add([FromBody] CreateWIPDTO dto)
        {
            var id = await _service.AddAsync(dto);
            return Ok(new { id });
        }

        [HttpPost("InsertWIPAccount")]
        public async Task<ActionResult> InsertWIPAccountAsync([FromBody] AccountDTO dto)
        {
            var id = await _service.InsertWIPAccountAsync(dto);
            return Ok(new { id });
        }

        [HttpPost("InsertWIPVehicleDetails")]
        public async Task<ActionResult> InsertWIPVehicleDetailsAsync([FromBody] VehicleTabDTO dto)
        {
            var id = await _service.InsertWIPVehicleDetailsAsync(dto);
            return Ok(new { id });
        }

        [HttpPost("AddItems")]
        public async Task<ActionResult> AddItems([FromBody] List<WIPGetItems> items)
        {
            var id = await _service.AddItemsAsync(items);
            return Ok(new { id });
        }

        [HttpPost("GeneralRequest")]
        public async Task<ActionResult> WIP_GeneralRequest_Insert([FromBody] GeneralRequest dto)
        {
            var id = await _service.WIP_GeneralRequest_Insert(dto);
            return Ok(new { id });
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update([FromBody] UpdateWIPDTO dto)
        {
            var updated = await _service.UpdateAsync(dto);
            return Ok(new { Updated = updated });
        }

        [HttpPut("UpdateWIPStatus")]
        public async Task<ActionResult> UpdateWIPStatus([FromBody] UpdateWIPStatusDTO dto)
        {
            var updated = await _service.UpdateWIPStatus(dto);
            return Ok(new { Updated = updated });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> Delete([FromBody] DeleteWIPDTO dto)
        {
            var deleted = await _service.DeleteAsync(dto);
            return Ok(new { Id = deleted });
        }

        [HttpDelete("DeleteItem")]
        public async Task<ActionResult> DeleteItem([FromBody] DeleteWIPDTO dto)
        {
            var deleted = await _service.DeleteItem(dto);
            return Ok(new { Id = deleted });
        }

        [HttpDelete("WIP_DeleteItems")]
        public async Task<ActionResult> WIP_DeleteItems(int WIPId, int Id)
        {
            var deleted = await _service.WIP_DeleteItems(WIPId, Id);
            return Ok(new { Id = deleted });
        }


        [HttpPost("GetAllServices")]
        public async Task<ActionResult<IEnumerable<RTSCodeDTO>>> GetAllServicesWithTime([FromBody] RTSWithTimeDTO dto)
        {
            var result = await _service.GetAllServicesWithTimeAsync(dto);
            return Ok(result);
        }

        [HttpGet("GetAllMenus")]
        public async Task<IEnumerable<MenuDTO>> GetMenuServicesAsync()
        {
            var result = await _service.GetMenuServicesAsync();
            return result;
        }

        [HttpGet("GetWIPItems")]
        public async Task<ActionResult<IEnumerable<CreateItemDTO>>> WIP_GetItemsById([FromQuery] int id, string lang)
        {
            var result = await _service.WIP_GetItemsById(id, lang);
            if (result == null) return new List<CreateItemDTO>();
            return Ok(result);
        }

        [HttpGet("GetWIPServices")]
        public async Task<ActionResult<IEnumerable<CreateWIPServiceDTO>>> WIP_GetServicesById([FromQuery] int id, string lang = "en")
        {
            var result = await _service.WIP_GetServicesById(id, lang);
            if (result == null) return new List<CreateWIPServiceDTO>();
            return Ok(result);
        }

        [HttpGet("GetWIP")]
        public async Task<ActionResult<IEnumerable<WIPGetItems>>> WIP_Get([FromQuery] int Id = 0)
        {
            var result = await _service.WIP_Get(Id);
            if (result == null) return new List<WIPGetItems>();
            return Ok(result);
        }

        [HttpGet("GetReturnParts")]
        public async Task<ActionResult<IEnumerable<ReturnItems>>> GetReturnParts([FromQuery] int WIPId = 0)
        {
            var result = await _service.GetReturnParts(WIPId);
            if (result == null) return new List<ReturnItems>();
            return Ok(result);
        }

        [HttpGet("WIPIDs")]
        public async Task<ActionResult<IEnumerable<WIPIDs>>> WIP_IDs([FromQuery] int Id = 0)
        {
            var result = await _service.WIP_IDs(Id);
            if (result == null) return new List<WIPIDs>();
            return Ok(result);
        }

        [HttpGet("GetAccountInfo/{id}")]
        public async Task<ActionResult<AccountDTO?>> WIP_GetAccountById(int Id = 0)
        {
            var result = await _service.WIP_GetAccountById(Id);
            if (result == null) return new AccountDTO();
            return Ok(result);
        }

        [HttpGet("GetVehicleDetailsById/{id}")]
        public async Task<ActionResult<VehicleTabDTO?>> WIP_GetVehicleDetailsById(int Id = 0)
        {
            var result = await _service.WIP_GetVehicleDetailsById(Id);
            if (result == null) return new VehicleTabDTO();
            return Ok(result);
        }


        [HttpPost("WIPSCheduleInsert")]
        public async Task<ActionResult> WIPSChedule_Insert([FromBody] WIPSChedule dto)
        {
            var id = await _service.WIPSChedule_Insert(dto);
            return Ok(new { id });
        }

        [HttpGet("WIPSCheduleGet")]
        public async Task<ActionResult<WIPSChedule?>> WIP_SChedule_Get([FromQuery] int RTSId, int WIPId, int KeyId)
        {
            var result = await _service.WIP_SChedule_Get(RTSId, WIPId, KeyId);
            if (result == null) return new WIPSChedule();
            return Ok(result);
        }
        [HttpGet("WIPSCheduleGetAll")]
        public async Task<ActionResult<WIPSChedule?>> WIP_SChedule_Get()
        {
            var result = await _service.WIP_SChedule_GetAll();
            if (result == null) return new WIPSChedule();
            return Ok(result);
        }

        [HttpPut("UpdateServiceStatus")]
        public async Task<ActionResult> UpdateServiceStatus([FromBody] UpdateService dto)
        {
            try
            {
                var updated = await _service.UpdateServiceStatus(dto);
                return Ok(new { Updated = updated });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdatePartStatus")]
        public async Task<ActionResult> UpdatePartStatus([FromBody] UpdatePartStatus dto)
        {
            try
            {
                var updated = await _service.UpdatePartStatus(dto);
                return Ok(new { Updated = updated });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("WIPGetDDL")]
        public async Task<ActionResult> GetWIPDDL()
        {
            var result = await _service.GetAllDDLAsync();
            return Ok(result);
        }
        [HttpPut("UpdateWIPOptions")]
        public async Task<ActionResult> UpdateWIPOptions([FromBody] WIPOptionsDTO dto)
        {
            var updated = await _service.UpdateWIPOptions(dto);
            return Ok(new { Updated = updated });
        }

        [HttpPut("UpdateReturnStatusById")]
        public async Task<ActionResult> UpdateReturnStatusById([FromQuery] int WIPId)
        {
            var updated = await _service.UpdateReturnStatusById(WIPId);
            return Ok(new { Updated = updated });
        }

        [HttpGet("GetOptionsById/{id}")]
        public async Task<ActionResult<WIPOptionsDTO?>> WIP_GetOptionsById(int Id = 0)
        {
            var result = await _service.WIP_GetOptionsById(Id);
            if (result == null) return new WIPOptionsDTO();
            return Ok(result);
        }

        [HttpGet("GetWIPServiceHistory")]
        public async Task<ActionResult<IEnumerable<M_WIPServiceHistoryDTO?>>> M_WIPServiceHistoryAsync([FromQuery] int VehicleId)
        {
            var result = await _service.M_WIPServiceHistoryAsync(VehicleId);
            if (result == null) return new List<M_WIPServiceHistoryDTO>();
            return Ok(result);
        }

        [HttpGet("GetPartsRequestRemainingQty")]
        public async Task<ActionResult<IEnumerable<PartsRequest_RemainingQtyGroupDTO?>>> WIP_PartsRequest_RemainingQty([FromQuery] int WIPId = 0)
        {
            var result = await _service.WIP_PartsRequest_RemainingQty(WIPId);
            if (result == null) return new List<PartsRequest_RemainingQtyGroupDTO>();
            return Ok(result);
        }

        [HttpPost("GetLabourRate")]
        public async Task<ActionResult<decimal?>> WIP_GetLabourRate([FromBody] LabourRateFilterDTO filter)
        {
            var result = await _service.WIP_GetLabourRate(filter);
            if (result == null) return 0;
            return Ok(result);
        }


        [HttpGet("WIPServiceHistoryDetailsGetPartsByWIPId")]
        public async Task<ActionResult<IEnumerable<WIPServiceHistoryDetails_Parts?>>> WIP_ServiceHistoryDetails_GetPartsByWIPId([FromQuery] int Id)
        {
            var result = await _service.WIP_ServiceHistoryDetails_GetPartsByWIPId(Id);
            if (result == null) return new List<WIPServiceHistoryDetails_Parts>();
            return Ok(result);
        }
        [HttpGet("WIPServiceHistoryDetailsGetLaboursByWIPId")]
        public async Task<ActionResult<IEnumerable<WIPServiceHistoryDetails_Labour?>>> WIP_ServiceHistoryDetails_GetLaboursByWIPId([FromQuery] int Id)
        {
            var result = await _service.WIP_ServiceHistoryDetails_GetLaboursByWIPId(Id);
            if (result == null) return new List<WIPServiceHistoryDetails_Labour>();
            return Ok(result);
        }

        [HttpGet("WIPServiceHistoryDetailsGetParts")]
        public async Task<ActionResult<IEnumerable<WIPServiceHistoryDetails_Parts?>>> WIP_ServiceHistoryDetails_GetParts()
        {
            var result = await _service.WIP_ServiceHistoryDetails_GetParts();
            if (result == null) return new List<WIPServiceHistoryDetails_Parts>();
            return Ok(result);
        }
        [HttpGet("WIPServiceHistoryDetailsGetLabours")]
        public async Task<ActionResult<IEnumerable<WIPServiceHistoryDetails_Labour?>>> WIP_ServiceHistoryDetails_GetLabours()
        {
            var result = await _service.WIP_ServiceHistoryDetails_GetLabours();
            if (result == null) return new List<WIPServiceHistoryDetails_Labour>();
            return Ok(result);
        }

        [HttpGet("GetWIPServicesByMovementId")]
        public async Task<IActionResult> GetWIPServicesByMovementIdAsync(int movementId)
        {
            var result = await _service.GetWIPServicesByMovementIdAsync(movementId);
            return Ok(result);
        }

        [HttpPost("TransferMaintenanceMovement")]
        public async Task<ActionResult> TransferMaintenanceMovement(int movementId, int workshopId, Guid masterId, string reason)
        {
            try
            {
                await _service.TransferMaintenanceMovement(movementId, workshopId, masterId, reason);
                return Ok(new { Message = "Transfer completed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteService")]
        public async Task<ActionResult> DeleteService([FromBody] DeleteServiceDTO dto)
        {
            try
            {
                var id = await _service.DeleteService(dto);
                return Ok(new { id = id });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { code = ex.Message, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { code = "ERR_UNKNOWN", message = ex.Message });
            }
        }

        [HttpPut("WIPClose")]
        public async Task<ActionResult> WIP_Close([FromBody] CloseWIPDTO dto)
        {
            try
            {
                var updated = await _service.WIP_Close(dto);
                return Ok(new { Updated = updated });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { code = ex.Message, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { code = "ERR_UNKNOWN", message = ex.Message });
            }
        }

        [HttpPost("WIPValidation")]
        public async Task<ActionResult> WIP_Validation(int WIPId)
        {
            try
            {
                var id = await _service.WIP_Validation(WIPId);
                return Ok(new { Id = id });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { code = ex.Message, message = ex.Message });
            }
            
        }

        [HttpPost("UpdateWIPServicesIsExternal")]
        public async Task<IActionResult> UpdateWIPServicesIsExternal(string ids)
        {
            try
            {
                var result = await _service.UpdateWIPServicesIsExternalAsync(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("UpdateWIPServicesIsFixed")]
        public async Task<IActionResult> UpdateWIPServicesIsFixedAsync(string ids)
        {
            try
            {
                var result = await _service.UpdateWIPServicesIsFixedAsync(ids);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAllVehicleTransferMovement")]
        public async Task<IActionResult> GetAllVehicleTransferMovement(int? vehicleId, int? page, int workshopId)
        {
            try
            {
                var result = await _service.GetAllVehicleTransferMovementAsync(vehicleId, page, workshopId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //private static string MapCloseErrorMessage(string code) => code switch
        //{
        //    "WIP_NOT_FOUND" => "لا يوجد أمر عمل بهذا الرقم.",
        //    "ALREADY_CLOSED" => "تم إغلاق أمر العمل مسبقًا.",
        //    "SERVICE_NOT_COMPLETED" => "هناك خدمات غير مكتملة.",
        //    "SERVICE_TIME_MISSING" => "هناك خدمات مكتملة بدون وقت عمل.",
        //    "PARTS_RETURN_PENDING" => "يوجد مرتجع قطع قيد انتظار المخازن.",
        //    "PARTIAL_INVOICE_INCOMPLETE" => "الفوترة الجزئية غير مكتملة.",
        //    "USER_CONTEXT_MISSING" => "هوية المستخدم غير مثبتة للجلسة.",
        //    _ => code 
        //};


        [HttpGet("GetWIPByVehicleId")]
        public async Task<ActionResult> GetByVehicleIdAsync(int vehicleId)
        {
            try
            {
                var result = await _service.GetByVehicleIdAsync(vehicleId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("UpdateIssueId")]
        public async Task<ActionResult> UpdateIssueId([FromBody] UpdateIssueIdDTO dto)
        {
            try
            {
                var result = await _service.UpdateIssueIdAsync(dto);
                return Ok(new { id = result });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetOpenWIPs")]
       
        public async Task<ActionResult<IEnumerable<int>>> WIP_GetOpenWIPs(int? Id, int BranchId)
        {
            var result = await _service.WIP_GetOpenWIPs(Id, BranchId);
            return Ok(result);
        }
        [HttpPost("UpdatePartStatusForSingleItem")]
        public async Task<ActionResult> UpdatePartStatusForSingleItem(UpdateSinglePartStatusDTO dto)
        {
            var result = await _service.UpdatePartStatusForSingleItem(dto);
            return Ok(result);
        }
        [HttpGet("GetWIPByMovementIds")]
        public async Task<IActionResult> GetWIPByMovementIds(string movementIds)
        {
            var result = await _service.GetWIPByMovementIds(movementIds);
            return Ok(result);
        }


        [HttpPost("UpdatePartWarehouseForSingleItem")]
        public async Task<ActionResult> WIP_UpdatePartWarehouseForSingleItem(UpdateSinglePartWarehouseDTO dto)
        {
            var result = await _service.WIP_UpdatePartWarehouseForSingleItem(dto);
            return Ok(result);
        }


        [HttpPost("InsertWIPInvoice")]
        public async Task<ActionResult> WIP_Invoice_Insert([FromBody] CreateWIPInvoiceDTO dto)
        {
            var id = await _service.WIP_Invoice_Insert(dto);
            return Ok(new { id });
        }



        [HttpGet("WIPInvoiceGetById")]
        public async Task<ActionResult<IEnumerable<WIPInvoiceDTO?>>> WIP_Invoice_GetById([FromQuery] int Id, int? TransactionMasterId)
        {
            var result = await _service.WIP_Invoice_GetById(Id, TransactionMasterId);
            if (result == null) return new List<WIPInvoiceDTO>();
            return Ok(result);
        }
        [HttpGet("WipInvoiceByHeaderId")]
        public async Task<IActionResult> WIP_InvoiceDetails_GetByHeaderId(int headerId)
        {
            var result = await _service.WIP_InvoiceDetails_GetByHeaderId(headerId);
            return Ok(result);
        }

        [HttpPost("UpdateWIPServicesExternalAndFixStatus")]
        public async Task<IActionResult> UpdateWIPServicesExternalAndFixStatus(List<WipServiceFixDto> services)
        {
            var result = await _service.UpdateWIPServicesExternalAndFixStatusAsync(services);
            return Ok(result);
        }
    }
}
