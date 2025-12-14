using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Insurance;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IMWorkOrderService
    {
        Task<MWorkOrderDTO> GetMWorkOrderByIdAysnc(int Id);
        Task<List<MWorkOrderDTO>> GetMWorkOrdersAsync(WorkOrderFilterDTO filter);
        Task<bool> DeleteMWorkOrderAsync(int id);
        Task<MWorkOrderDTO> InsertMWorkOrderAsync(MWorkOrderDTO workOrder);
        Task<MWorkOrderDTO> UpdateMWorkOrderAsync(MWorkOrderDTO workOrder);
        Task UpdateWorkOrderKMAsync(int workOrderId, decimal receivedKM);
        Task<int> UpdateWorkOrderStatusAsync(int workOrderId, int statusId, decimal totalCost = 0);
        Task<int> UpdateMAccidentStatusAsync(InsuranceClaimHistory insuranceClaimHistory);
        Task<MWorkOrderDTO> GetMWorkOrderByMasterId(Guid id);
        Task UpdateWorkOrderInvoicingStatus(int workOrderId);
        Task FixWorkOrder(int workOrderId, bool isFix);
    }
}