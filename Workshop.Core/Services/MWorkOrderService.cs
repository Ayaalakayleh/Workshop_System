using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;
using Workshop.Infrastructure;

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

		public Task<int> UniqueAccidentNo(long DamageId, string ReportNo)
		{
			return _mWorkOrderRepository.M_UniqueAccidentNo(DamageId, ReportNo);
		}
		public Task<List<MWorkOrderDetailDTO>> M_WorkOrderDetails_GetByWorkOrderID(int WorkOrderId)
		{
			try
			{
				var result = _mWorkOrderRepository.M_WorkOrderDetails_GetByWorkOrderID(WorkOrderId);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}



		public async Task WorkOrderReport_Insert(List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument)
		{
			await _mWorkOrderRepository.WorkOrderReport_Insert(WorkOrderDocument);
		}
		public async Task WorkOrderDetalsDoc_Insert(List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument)
		{
			await _mWorkOrderRepository.WorkOrderDetalsDoc_Insert(WorkOrderDocument);
		}
		public async Task<MWorkOrderDetail> WorkOrderDetails_Insert(MWorkOrderDetail workOrder)
		{
			try
			{
				var result = await _mWorkOrderRepository.WorkOrderDetails_Insert(workOrder);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<MWorkOrderDetail> M_WorkOrderDetails_Update(MWorkOrderDetail workOrder)
		{
			try
			{
				var result = await _mWorkOrderRepository.M_WorkOrderDetails_Update(workOrder);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<List<MWorkOrdersDetailsDocument>> WorkOrdersDetailsDocument_Get(int WorkOrderDetailsId)
		{
			try
			{
				var result = await _mWorkOrderRepository.WorkOrdersDetailsDocument_Get(WorkOrderDetailsId);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<List<MWorkOrdersDetailsDocument>> WorkOrderReports_Get(int WorkOrderId)
		{
			try
			{
				var result = await _mWorkOrderRepository.WorkOrderReports_Get(WorkOrderId);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task DeleteReportByWorkOrderId(int WorkOrderId)
		{
			await _mWorkOrderRepository.DeleteReportByWorkOrderId(WorkOrderId);
		}
		public async Task<bool> DeleteMWorkOrderDetailsAsync(int id)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("Invalid workOrder ID");

				var result = await _mWorkOrderRepository.DeleteMWorkOrderDetailsAsync(id);
				return result > 0;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<bool> DeleteMWorkOrderDetailsDocAsync(int id)
		{
			try
			{
				if (id <= 0)
					throw new ArgumentException("Invalid workOrder ID");

				var result = await _mWorkOrderRepository.DeleteMWorkOrderDetailsDocAsync(id);
				return result > 0;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<int> CheckAccidentNo(string AccidentNo)
		{
			try
			{


				var result = await _mWorkOrderRepository.CheckAccidentNo(AccidentNo);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<List<WorkshopWorkOrderStatusReportDTO>> GetWorkshopWorkOrdersStatus(int? vehicleId, DateTime? fromDate, DateTime? toDate, int? externalVehicleId)
		{
			try
			{
				var result = await _mWorkOrderRepository.GetWorkshopWorkOrdersStatus(vehicleId, fromDate, toDate, externalVehicleId);
				return result;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<bool> WorkOrder_FinishWorkflow(int id, int status)
		{
			try
			{
				return await _mWorkOrderRepository.WorkOrder_FinishWorkflow(id, status);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in WorkOrder_FinishWorkflow", ex);
			}
		}

		public async Task<bool> UpdateClaimStatus(InsuranceClaimHistory model)
		{
			try
			{
				return await _mWorkOrderRepository.UpdateClaimStatus(model);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in UpdateClaimStatus", ex);
			}
		}

		public async Task<List<InsuranceClaimHistory>> GetClaimHistory(int workOrderId)
		{
			try
			{
				return await _mWorkOrderRepository.GetClaimHistory(workOrderId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetClaimHistory", ex);
			}
		}

		public async Task<ClaimFileDTO> GetClaimFile(int workOrderId)
		{
			try
			{
				return await _mWorkOrderRepository.GetClaimFile(workOrderId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetClaimFile", ex);
			}
		}

		public async Task<List<InsuranceClaimHistory>> GetVehicleWithOpenClaim(int companyId)
		{
			try
			{
				return await _mWorkOrderRepository.GetVehicleWithOpenClaim(companyId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetVehicleWithOpenClaim", ex);
			}
		}

		public async Task<InsuranceClaimStatistaic> GetInsuranceClaimStatistaic(int companyId)
		{
			try
			{
				return await _mWorkOrderRepository.GetInsuranceClaimStatistaic(companyId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetInsuranceClaimStatistaic", ex);
			}
		}

		public async Task<List<WorkOrderClaimEarnings>> GetEarningsFromAccidentClaim(int companyId)
		{
			try
			{
				return await _mWorkOrderRepository.GetEarningsFromAccidentClaim(companyId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetEarningsFromAccidentClaim", ex);
			}
		}
		public async Task<List<AmountFromClientsForAccidents>> M_Claim_GetCustomerwithDeductibleAmount(int companyId)
		{
			try
			{
				return await _mWorkOrderRepository.M_Claim_GetCustomerwithDeductibleAmount(companyId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in M_Claim_GetCustomerwithDeductibleAmount", ex);
			}
		}
		public async Task<bool> UpdateClaimAmountReceivedDate(MWorkOrderDTO model)
		{
			try
			{
				return await _mWorkOrderRepository.UpdateClaimAmountReceivedDate(model);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in UpdateClaimAmountReceivedDate", ex);
			}
		}
		public async Task<int> M_WorkOrderStatus_Back(int workOrderId, int status)
		{
			try
			{
				return await _mWorkOrderRepository.M_WorkOrderStatus_Back(workOrderId, status);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in M_WorkOrderStatus_Back", ex);
			}
		}

		public async Task<int> CheckAccidentCountPerCustomer(int customerId)
		{
			try
			{
				return await _mWorkOrderRepository.CheckAccidentCountPerCustomer(customerId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in CheckAccidentCountPerCustomer", ex);
			}
		}
		public async Task<int> M_SendToLegalAfter3Weeks()
		{
			try
			{
				return await _mWorkOrderRepository.M_SendToLegalAfter3Weeks();
			}
			catch (Exception ex)
			{
				throw new Exception("Error in M_SendToLegalAfter3Weeks", ex);
			}
		}
		public async Task<List<MTaqdeeratDocumentDTO>> M_TaqdeeratDocs_GetById(int workOrderId)
		{
			try
			{
				return await _mWorkOrderRepository.M_TaqdeeratDocs_GetById(workOrderId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in M_TaqdeeratDocs_GetById", ex);
			}
		}
		public async Task<int> M_AddTaqdeeratDoc(MTaqdeeratDocumentDTO model)
		{
			try
			{
				return await _mWorkOrderRepository.M_AddTaqdeeratDoc(model);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in M_AddTaqdeeratDoc", ex);
			}
		}
		public async Task<int> M_TaqDocs_Delete(int workOrderId)
		{
			try
			{
				return await _mWorkOrderRepository.M_TaqDocs_Delete(workOrderId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in M_TaqDocs_Delete", ex);
			}
		}
		public async Task<bool> WorkOrders_VehicleWorkOrderStatus(int workOrderId)
		{
			try
			{
				return await _mWorkOrderRepository.WorkOrders_VehicleWorkOrderStatus(workOrderId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in WorkOrders_VehicleWorkOrderStatus", ex);
			}
		}
		public async Task<List<WorkOrderStatusDTO>> GetAllWorkflowStatus()
		{
			try
			{
				return await _mWorkOrderRepository.GetAllWorkflowStatus();
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetAllWorkflowStatus", ex);
			}
		}
		public async Task<List<WorkOrderInsuranceDetails>> GetInsuranceDetails(int companyId, DateTime? fromDate, DateTime? toDate)
		{
			try
			{
				return await _mWorkOrderRepository.GetInsuranceDetails(companyId, fromDate, toDate);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetAllWorkflowStatus", ex);
			}
		}
		public async Task<int> M_SaveStatusRoleId(List<StatusRoleViewModel> liStatusRole)
		{
			try
			{
				return await _mWorkOrderRepository.M_SaveStatusRoleId(liStatusRole);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in M_SaveStatusRoleId", ex);
			}
		}



		public async Task<IEnumerable<VehicleWorkOrdersSummery>> GetWorkOrdersSummeryByVehicleIdAsync(int vehicleId, int companyId)
		{
			try
			{
				return await _mWorkOrderRepository.GetWorkOrdersSummeryByVehicleIdAsync(vehicleId, companyId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetWorkOrdersSummeryByVehicleIdAsync", ex);
			}
		}
		public async Task<string> GetLastMaintenanceMovementStrikeAsync(int vehicleId)
		{
			try
			{
				return await _mWorkOrderRepository.GetLastMaintenanceMovementStrikeAsync(vehicleId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in GetWorkOrdersSummeryByVehicleIdAsync", ex);
			}
		}
	}
}