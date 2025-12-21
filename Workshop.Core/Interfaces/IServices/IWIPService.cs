using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Services;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IWIPService
    {
        Task<IEnumerable<WIPDTO>> GetAllAsync(FilterWIPDTO oFilter);
        Task<WIPDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(CreateWIPDTO dto);
        Task<int> UpdateAsync(UpdateWIPDTO dto);
        Task<int> DeleteAsync(DeleteWIPDTO dto);
        Task<int> DeleteItem(DeleteWIPDTO dto);
        Task<IEnumerable<RTSCodeDTO>> GetAllServicesWithTimeAsync(RTSWithTimeDTO dto);
        Task<IEnumerable<CreateItemDTO?>> WIP_GetItemsById(int id, string lang);
        Task<IEnumerable<CreateWIPServiceDTO?>> WIP_GetServicesById(int id, string lang="en");
        Task<IEnumerable<WIPGetItems>> WIP_Get(int Id = 0);
        Task<IEnumerable<WIPSChedule>> WIP_SChedule_GetAll();
        Task<IEnumerable<ReturnItems>> GetReturnParts(int WIPId=0);
        Task<IEnumerable<GeneralRequest>> WIP_IDs(int Id = 0);
        Task<int> WIP_GeneralRequest_Insert(GeneralRequest dto);
        Task<int> AddItemsAsync(List<WIPGetItems> items);
        Task<int> InsertWIPAccountAsync(AccountDTO dto);
        Task<IEnumerable<MenuDTO>> GetMenuServicesAsync();
        Task<AccountDTO?> WIP_GetAccountById(int id);
        Task<VehicleTabDTO?> WIP_GetVehicleDetailsById(int id);
        Task<int> WIPSChedule_Insert(WIPSChedule dto);
        Task<WIPSChedule?> WIP_SChedule_Get(int RTSId, int WIPId, int KeyId);
        Task<int> UpdateServiceStatus(UpdateService dto);
        Task<IEnumerable<WIPDTO>> GetAllDDLAsync();

        Task<int> InsertWIPVehicleDetailsAsync(VehicleTabDTO dto);
        Task<int> UpdateWIPOptions(WIPOptionsDTO dto);
        Task<WIPOptionsDTO?> WIP_GetOptionsById(int id);
        Task<int> UpdateReturnStatusById(int WIPId);
        Task<IEnumerable<M_WIPServiceHistoryDTO?>> M_WIPServiceHistoryAsync(int VehicleId);
        Task<IEnumerable<PartsRequest_RemainingQtyGroupDTO?>> WIP_PartsRequest_RemainingQty(int WIPId);
        Task<decimal?> WIP_GetLabourRate(LabourRateFilterDTO filter);
        Task<IEnumerable<WIPServiceHistoryDetails_Parts?>> WIP_ServiceHistoryDetails_GetPartsByWIPId(int id);
        Task<IEnumerable<WIPServiceHistoryDetails_Labour?>> WIP_ServiceHistoryDetails_GetLaboursByWIPId(int id);
        Task<IEnumerable<WIPServiceHistoryDetails_Parts?>> WIP_ServiceHistoryDetails_GetParts();
        Task<IEnumerable<WIPServiceHistoryDetails_Labour?>> WIP_ServiceHistoryDetails_GetLabours();
        Task<int> DeleteService(DeleteServiceDTO dto);
        Task<int?> WIP_Close(CloseWIPDTO dto);
        Task<int?> WIP_Validation(int WIPId);
        Task TransferMaintenanceMovement(int movementId, int workshopId, Guid masterId, string reason);
        Task<int> UpdateWIPServicesIsExternalAsync(string ids);
        Task<int> UpdateWIPServicesIsFixedAsync(string ids);
        Task<List<VehicleMovement>> GetAllVehicleTransferMovementAsync(int? vehicleId, int? page, int workshopId);
        Task<IEnumerable<CreateWIPServiceDTO>> GetWIPServicesByMovementIdAsync(int movementId);
        Task<CheckWIPCountDTO> GetByVehicleIdAsync(int vehicleId);
        Task<int> UpdatePartStatus(UpdatePartStatus dto);
        Task<int> UpdateIssueIdAsync(UpdateIssueIdDTO dto);
        Task<IEnumerable<int>> WIP_GetOpenWIPs(int? Id, int BranchId);
        Task<int> WIP_DeleteItems(int WIPId, int Id);
        Task<int> UpdatePartStatusForSingleItem(UpdateSinglePartStatusDTO dto);
        Task<IEnumerable<WIPDTO>> GetWIPByMovementIds(string movementIds);
        Task<int> WIP_UpdatePartWarehouseForSingleItem(UpdateSinglePartWarehouseDTO dto);
        Task<int> WIP_Invoice_Insert(CreateWIPInvoiceDTO dto);
        Task<IEnumerable<WIPInvoiceDTO?>> WIP_Invoice_GetById(int? id, int? TransactionMasterId);
        Task<IEnumerable<CreateWIPServiceDTO>> GetAllInternalLabourLineAsync(int WIPId);
        Task<IEnumerable<CreateItemDTO>> GetAllInternalPartsLineAsync(int WIPId);
        Task<int> UpdateWIPStatus(UpdateWIPStatusDTO dto);
        Task<IEnumerable<WipInvoiceDetailDTO>> WIP_InvoiceDetails_GetByHeaderId(int headerId);
    }
}
