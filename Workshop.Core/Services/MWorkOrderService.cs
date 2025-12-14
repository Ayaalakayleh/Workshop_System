using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class MWorkOrderService : IMWorkOrderService
    {
        private readonly IMWorkOrderRepository _mWorkOrderRepository;

        public MWorkOrderService(IMWorkOrderRepository mWorkOrderRepository)
        {
            _mWorkOrderRepository = mWorkOrderRepository;
        }

        public async Task<MWorkOrderDTO> GetMWorkOrderByIdAysnc(int Id)
        {
            return await _mWorkOrderRepository.GetMWorkOrderByIdAysnc(Id);
        }

        public async Task<List<MWorkOrderDTO>> GetMWorkOrdersAsync(WorkOrderFilterDTO filter)
        {
            try
            {
                var result = await _mWorkOrderRepository.GetMWorkOrdersAsync(filter);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<MWorkOrderDTO> InsertMWorkOrderAsync(MWorkOrderDTO workOrder)
        {
            try
            {
                var result = await _mWorkOrderRepository.InsertMWorkOrderAsync(workOrder);

                if (result == null || result.Id <= 0)
                    throw new InvalidOperationException("Failed to insert workOrder");

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<MWorkOrderDTO> UpdateMWorkOrderAsync(MWorkOrderDTO workOrder)
        {
            try
            {
                var result = await _mWorkOrderRepository.UpdateMWorkOrderAsync(workOrder);

                if (result == null || result.Id <= 0)
                    throw new InvalidOperationException("Failed to update workOrder");

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> DeleteMWorkOrderAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid workOrder ID");

                var result = await _mWorkOrderRepository.DeleteMWorkOrderAsync(id);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateWorkOrderKMAsync(int workOrderId, decimal receivedKM)
        {
            await _mWorkOrderRepository.UpdateWorkOrderKMAsync(workOrderId, receivedKM);
        }

        public async Task<int> UpdateWorkOrderStatusAsync(int workOrderId, int statusId, decimal totalCost = 0)
        {
            return await _mWorkOrderRepository.UpdateWorkOrderStatusAsync(workOrderId, statusId, totalCost);
        }

        public async Task<int> UpdateMAccidentStatusAsync(InsuranceClaimHistory insuranceClaimHistory)
        {
            return await _mWorkOrderRepository.UpdateMAccidentStatusAsync(insuranceClaimHistory);
        }

        public async Task<MWorkOrderDTO> GetMWorkOrderByMasterId(Guid id)
        {
            return await _mWorkOrderRepository.GetMWorkOrderByMasterId(id);
        }

        public async Task UpdateWorkOrderInvoicingStatus(int workOrderId)
        {
            await _mWorkOrderRepository.UpdateWorkOrderInvoicingStatus(workOrderId);
        }

        public async Task FixWorkOrder(int workOrderId, bool isFix)
        {
            await _mWorkOrderRepository.FixWorkOrder(workOrderId, isFix);
        }
    }
}