
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Infrastructure;

namespace Workshop.Core.Interfaces.IRepositories
{
	public interface IMWorkOrderRepository
	{
		Task<MWorkOrderDTO> GetMWorkOrderByIdAysnc(int Id);
		Task<List<MWorkOrderDTO>> GetMWorkOrdersAsync(WorkOrderFilterDTO filter);
		Task<int> DeleteMWorkOrderAsync(int id);
		Task<MWorkOrderDTO> InsertMWorkOrderAsync(MWorkOrderDTO workOrder);
		Task<MWorkOrderDTO> UpdateMWorkOrderAsync(MWorkOrderDTO workOrder);
		Task UpdateWorkOrderKMAsync(int workOrderId, decimal receivedKM);
		Task<int> UpdateWorkOrderStatusAsync(int workOrderId, int statusId, decimal totalCost = 0);
		Task<int> UpdateMAccidentStatusAsync(InsuranceClaimHistory insuranceClaimHistory);
		Task<MWorkOrderDTO> GetMWorkOrderByMasterId(Guid id);
		Task UpdateWorkOrderInvoicingStatus(int workOrderId);
		Task FixWorkOrder(int workOrderId, bool isFix);
		Task<int> M_UniqueAccidentNo(long DamageId, string ReportNo);

		Task<List<MWorkOrderDetailDTO>> M_WorkOrderDetails_GetByWorkOrderID(int WorkOrderId);
		Task WorkOrderReport_Insert(List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument);
		Task WorkOrderDetalsDoc_Insert(List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument);
		Task<MWorkOrderDetail> WorkOrderDetails_Insert(MWorkOrderDetail workOrder);
		Task<MWorkOrderDetail> M_WorkOrderDetails_Update(MWorkOrderDetail workOrder);
		Task<List<MWorkOrdersDetailsDocument>> WorkOrdersDetailsDocument_Get(int WorkOrderDetailsId);
		Task<List<MWorkOrdersDetailsDocument>> WorkOrderReports_Get(int WorkOrderId);

		Task DeleteReportByWorkOrderId(int WorkOrderId);
		Task<int> DeleteMWorkOrderDetailsAsync(int id);
		Task<int> DeleteMWorkOrderDetailsDocAsync(int id);
		Task<int> CheckAccidentNo(string AccidentNo);
		Task<List<WorkshopWorkOrderStatusReportDTO>> GetWorkshopWorkOrdersStatus(int? vehicleId, DateTime? fromDate, DateTime? toDate, int? externalVehicleId);
		Task<bool> WorkOrder_FinishWorkflow(int id, int status);
		Task<bool> UpdateClaimStatus(InsuranceClaimHistory model);
		Task<List<InsuranceClaimHistory>> GetClaimHistory(int workOrderId);
		Task<ClaimFileDTO> GetClaimFile(int workOrderId);
		Task<List<InsuranceClaimHistory>> GetVehicleWithOpenClaim(int companyId);
		Task<InsuranceClaimStatistaic> GetInsuranceClaimStatistaic(int companyId);
		Task<List<WorkOrderClaimEarnings>> GetEarningsFromAccidentClaim(int companyId);
		Task<List<AmountFromClientsForAccidents>> M_Claim_GetCustomerwithDeductibleAmount(int companyId);
		Task<bool> UpdateClaimAmountReceivedDate(MWorkOrderDTO model);
		Task<int> M_WorkOrderStatus_Back(int workOrderId, int status);

		Task<int> CheckAccidentCountPerCustomer(int customerId);
		Task<int> M_SendToLegalAfter3Weeks();
		Task<List<MTaqdeeratDocumentDTO>> M_TaqdeeratDocs_GetById(int workOrderId);
		Task<int> M_AddTaqdeeratDoc(MTaqdeeratDocumentDTO model);
		Task<int> M_TaqDocs_Delete(int workOrderId);
		Task<List<WorkOrderStatusDTO>> GetAllWorkflowStatus();
		Task<int> M_SaveStatusRoleId(List<StatusRoleViewModel> liStatusRole);
		Task<bool> WorkOrders_VehicleWorkOrderStatus(int Vehicleid);
		Task<List<WorkOrderInsuranceDetails>> GetInsuranceDetails(int companyId, DateTime? fromDate, DateTime? toDate);
		Task<IEnumerable<VehicleWorkOrdersSummery>> GetWorkOrdersSummeryByVehicleIdAsync(int vehicleId, int companyId);
		Task<string?> GetLastMaintenanceMovementStrikeAsync(int vehicleId);
	}
}