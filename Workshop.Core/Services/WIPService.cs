using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;
using Workshop.Domain.Entities;

namespace Workshop.Core.Services
{
    public class WIPService : IWIPService
    {
        private readonly IWIPRepository _repository;
        public WIPService(IWIPRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<WIPDTO>> GetAllAsync(FilterWIPDTO oFilter)
        {
            return await _repository.GetAllAsync(oFilter);
        }

        public async Task<WIPDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> AddAsync(CreateWIPDTO dto)
        {
            return await _repository.AddAsync(dto);
        }

        public async Task<int> InsertWIPAccountAsync(AccountDTO dto)
        {
            return await _repository.InsertWIPAccountAsync(dto);
        }

        public async Task<int> AddItemsAsync(List<WIPGetItems> items)
        {
            return await _repository.AddItemsAsync(items);
        }

        public async Task<int> UpdateAsync(UpdateWIPDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<int> DeleteAsync(DeleteWIPDTO dto)
        {
            return await _repository.DeleteAsync(dto);
        }
        
        public async Task<int> DeleteItem(DeleteWIPDTO dto)
        {
            return await _repository.DeleteItem(dto);
        }

        public async Task<IEnumerable<RTSCodeDTO>> GetAllServicesWithTimeAsync(RTSWithTimeDTO dto)
        {
            return await _repository.GetAllServicesWithTimeAsync(dto);
        }
        public async Task<IEnumerable<MenuDTO>> GetMenuServicesAsync()
        {
            return await _repository.GetMenuServicesAsync();
        }
        public async Task<IEnumerable<CreateItemDTO?>> WIP_GetItemsById(int id, string lang)
        {
            var result =  await _repository.WIP_GetItemsById(id);
            foreach (var item in result)
            {
                //item.StatusText = lang == "en" ? (item.StatusCode + "-" + item.StatusPrimaryName) : (item.StatusCode + "-" + item.StatusSecondaryName);
                item.StatusText = lang == "en" ? item.StatusPrimaryName :  item.StatusSecondaryName;
            }
            return result;
        }
        public async Task<IEnumerable<CreateWIPServiceDTO?>> WIP_GetServicesById(int id, string lang)
        {
            var result =  await _repository.WIP_GetServicesById(id);
            foreach(var item in result)
            {
                item.StatusText = lang == "en" ? (item.StatusCode + "-" + item.StatusPrimaryName) : (item.StatusCode + "-" + item.StatusSecondaryName);
            }
            return result;
        }
        public async Task<IEnumerable<WIPSChedule>> WIP_SChedule_GetAll()
        {
            var result = await _repository.WIP_SChedule_GetAll();
            return result;
        }
        public async Task<IEnumerable<WIPGetItems>> WIP_Get(int Id=0)
        {
            return await _repository.WIP_Get(Id);
        }
        public async Task<IEnumerable<ReturnItems>> GetReturnParts(int WIPId = 0)
        {
            return await _repository.GetReturnParts(WIPId);
        }

        public async Task<IEnumerable<GeneralRequest>> WIP_IDs(int Id = 0)
        {
            return await _repository.WIP_IDs(Id);
        }
        public async Task<int> WIP_GeneralRequest_Insert(GeneralRequest dto)
        {
            return await _repository.WIP_GeneralRequest_Insert(dto);
        }
        public async Task<AccountDTO?> WIP_GetAccountById(int id)
        {
            return await _repository.WIP_GetAccountById(id);
        }
        public async Task<int> WIPSChedule_Insert(WIPSChedule dto)
        {
            return await _repository.WIPSChedule_Insert(dto);
        }
        public async Task<WIPSChedule?> WIP_SChedule_Get(int RTSId, int WIPId, int KeyId)
        {
            return await _repository.WIP_SChedule_Get(RTSId, WIPId, KeyId);
        }

        public async Task<int> UpdateServiceStatus(UpdateService dto)
        {
            return await _repository.UpdateServiceStatus(dto);
        }
        public async Task<int> InsertWIPVehicleDetailsAsync(VehicleTabDTO dto)
        {
            return await _repository.InsertWIPVehicleDetailsAsync(dto);
        }
        
        public async Task<VehicleTabDTO?> WIP_GetVehicleDetailsById(int id)
        {
            return await _repository.WIP_GetVehicleDetailsById(id);
        }

        public async Task<int> UpdateWIPOptions(WIPOptionsDTO dto)
        {
            return await _repository.UpdateWIPOptions(dto);
        }

        public async Task<int> UpdateReturnStatusById(int WIPId)
        {
            return await _repository.UpdateReturnStatusById(WIPId);
        }

        public async Task<WIPOptionsDTO?> WIP_GetOptionsById(int id)
        {
            return await _repository.WIP_GetOptionsById(id);
        }
        public async Task<IEnumerable<M_WIPServiceHistoryDTO?>> M_WIPServiceHistoryAsync(int VehicleId)
        {
            return await _repository.M_WIPServiceHistoryAsync(VehicleId);
        }
        public async Task<IEnumerable<PartsRequest_RemainingQtyGroupDTO?>> WIP_PartsRequest_RemainingQty(int WIPId)
        {
            return await _repository.WIP_PartsRequest_RemainingQty(WIPId);
        }

        public async Task<decimal?> WIP_GetLabourRate(LabourRateFilterDTO filter)
        {
              return await _repository.WIP_GetLabourRate(filter);
        }


        public async Task<IEnumerable<WIPServiceHistoryDetails_Parts?>> WIP_ServiceHistoryDetails_GetPartsByWIPId(int id)
        {
            return await _repository.WIP_ServiceHistoryDetails_GetPartsByWIPId(id);
        }
        public async Task<IEnumerable<WIPServiceHistoryDetails_Labour?>> WIP_ServiceHistoryDetails_GetLaboursByWIPId(int id)
        {
            return await _repository.WIP_ServiceHistoryDetails_GetLaboursByWIPId(id);
        }

        public async Task<IEnumerable<WIPServiceHistoryDetails_Parts?>> WIP_ServiceHistoryDetails_GetParts()
        {
            return await _repository.WIP_ServiceHistoryDetails_GetParts();
        }
        public async Task<IEnumerable<WIPServiceHistoryDetails_Labour?>> WIP_ServiceHistoryDetails_GetLabours()
        {
            return await _repository.WIP_ServiceHistoryDetails_GetLabours();
        }

        public async Task<IEnumerable<WIPDTO>> GetAllDDLAsync()
        {
            return await _repository.GetAllDDLAsync();
        }

        public async Task<int> DeleteService(DeleteServiceDTO dto)
        {
            return await _repository.DeleteService(dto);
        }

        public async Task<int?> WIP_Close(CloseWIPDTO dto)
        {
            return await _repository.WIP_Close(dto);
        }

        public async Task<int?> WIP_Validation(int WIPId)
        {
            return await _repository.WIP_Validation(WIPId);
        }

        public async Task<IEnumerable<CreateWIPServiceDTO>> GetWIPServicesByMovementIdAsync(int movementId)
        {
            return await _repository.GetWIPServicesByMovementIdAsync(movementId);
        }
        #region Transfer Movement
        public async Task TransferMaintenanceMovement(int movementId, int workshopId, Guid masterId, string reason)
        {
            await _repository.TransferMaintenanceMovement(movementId, workshopId, masterId, reason);
        }

        public async Task<int> UpdateWIPServicesIsExternalAsync(string ids)
        {
            return await _repository.UpdateWIPServicesIsExternalAsync(ids);
        }
        
        public async Task<int> UpdateWIPServicesIsFixedAsync(string ids)
        {
            return await _repository.UpdateWIPServicesIsFixedAsync(ids);
        }

        public async Task<List<VehicleMovement>> GetAllVehicleTransferMovementAsync(int? vehicleId, int? page, int workshopId)
        {
            return await _repository.GetAllVehicleTransferMovementAsync(vehicleId, page, workshopId);
        }
        #endregion


        public async Task<CheckWIPCountDTO> GetByVehicleIdAsync(int vehicleId)
        {
            return await _repository.GetByVehicleIdAsync(vehicleId);
        }

        public async Task<int> UpdatePartStatus(UpdatePartStatus dto)
        {
            return await _repository.UpdatePartStatus(dto);
        }

        public async Task<int> UpdateIssueIdAsync(UpdateIssueIdDTO dto)
        {
            return await _repository.UpdateIssueIdAsync(dto);
        }

        public async Task<IEnumerable<int>> WIP_GetOpenWIPs(int? Id, int BranchId)
        {
            return await _repository.WIP_GetOpenWIPs(Id, BranchId);
        }

        public async Task<int> WIP_DeleteItems(int WIPId, int Id)
        {
            return await _repository.WIP_DeleteItems(WIPId, Id);
        }
        public async Task<int> UpdatePartStatusForSingleItem(UpdateSinglePartStatusDTO dto)
        {
            return await _repository.UpdatePartStatusForSingleItem(dto);
        }
        public async Task<IEnumerable<WIPDTO>> GetWIPByMovementIds(string movementIds)
        {
            return await _repository.GetWIPByMovementIds(movementIds);
        }
        public async Task<int> WIP_UpdatePartWarehouseForSingleItem(UpdateSinglePartWarehouseDTO dto)
        {
            return await _repository.WIP_UpdatePartWarehouseForSingleItem(dto);
        }
        
        public async Task<int> WIP_Invoice_Insert(CreateWIPInvoiceDTO dto)
        {
            return await _repository.WIP_Invoice_Insert(dto);
        }
        
        public async Task<IEnumerable<WIPInvoiceDTO?>> WIP_Invoice_GetById(int? id, int? TransactionMasterId)
        {
            return await _repository.WIP_Invoice_GetById(id, TransactionMasterId);
        }

        public async Task<IEnumerable<CreateWIPServiceDTO>> GetAllInternalLabourLineAsync(int WIPId)
        {
            return await _repository.GetAllInternalLabourLineAsync(WIPId);
        }

        public async Task<IEnumerable<CreateItemDTO>> GetAllInternalPartsLineAsync(int WIPId)
        {
            return await _repository.GetAllInternalPartsLineAsync(WIPId);
        }

        public async Task<int> UpdateWIPStatus(UpdateWIPStatusDTO dto)
        {
            return await _repository.UpdateWIPStatus(dto);
        }
        public async Task<IEnumerable<WipInvoiceDetailDTO>> WIP_InvoiceDetails_GetByHeaderId(int headerId)
        {
            return await _repository.WIP_InvoiceDetails_GetByHeaderId(headerId);
        }
        public async Task<int> UpdateWIPServicesExternalAndFixStatusAsync(List<WipServiceFixDto> services)
        {
            return await _repository.UpdateWIPServicesExternalAndFixStatusAsync(services);
        }

    }
}
