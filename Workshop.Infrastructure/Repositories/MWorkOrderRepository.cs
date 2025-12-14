using Dapper;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class MWorkOrderRepository : IMWorkOrderRepository
    {
        private readonly Database _database;
        private readonly DapperContext _context;
        /*  public MWorkOrderRepository(Database database)
          {
              _database = database;
          }*/
        public MWorkOrderRepository(Database database, DapperContext context)
        {
            _database = database;
            _context = context;
        }

        public async Task<MWorkOrderDTO> GetMWorkOrderByIdAysnc(int Id)
        {
            // var parameters = new { Id = Id };
            using var connection = _context.CreateConnection();
            /*var result = await _database.ExecuteGetByIdProcedure<MWorkOrderDTO>(
                "[WorkOrder].[M_WorkOrder_GetById]",
                parameters);
            */
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", Id);
                var result = await connection.QueryAsync<MWorkOrderDTO>(
                "[WorkOrder].[M_WorkOrders_GetById]",
                    parameters
                    );
                return result?.FirstOrDefault();
            }
            catch (Exception e)
            {

                var r = e;
            }
            return null;
        }

        public async Task<List<MWorkOrderDTO>> GetMWorkOrdersAsync(WorkOrderFilterDTO filter)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@vehicleId", filter.VehicleID);
                parameters.Add("@companyId", filter.CompanyId);
                parameters.Add("@branchId", filter.BranchId);
                parameters.Add("@IsInsuraceDamege", filter.IsInsuraceDamege);
                parameters.Add("@Type", filter.Type);
                parameters.Add("@WorkOrderId", filter.Id);
                parameters.Add("@RowsPerPage", 25);
                parameters.Add("@PageNumber", filter.page);
                parameters.Add("@FromDate", filter.FromDate);
                parameters.Add("@ToDate", filter.ToDate);
                parameters.Add("@WorkOrderTypeId", filter.WorkOrderTypeId);
                parameters.Add("@IsExternal", filter.IsExternal);
                parameters.Add("@ExternalVehicleID", filter.ExternalVehicleID);
                parameters.Add("@CreatedBy", filter.CreatedBy);
                parameters.Add("@WorkOrderStatus", filter.WorkOrderStatus);
                parameters.Add("@InvoicingStatus", filter.InvoicingStatus);
                parameters.Add("@WorkOrderNo", filter.WorkOrderNo);

                var result = await connection.QueryAsync<MWorkOrderDTO>(
                    "[WorkOrder].[D_WorkOrder_Get]",
                    parameters
                );

                foreach (var item in result)
                {
                    item.WorkOrderTitle = item.BranchId + item.WorkOrderNo.Value.ToString("D3");
                }
                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<int> DeleteMWorkOrderAsync(int id)
        {
            try
            {
                var parameters = new { Id = id };

                return await _database.ExecuteDeleteProcedure<int>(
                    "[WorkOrder].[M_WorkOrders_Delete]",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error deleting workOrder with ID {DamageId}", id);
                throw;
            }
        }

        public async Task<MWorkOrderDTO> InsertMWorkOrderAsync(MWorkOrderDTO workOrder)
        {
            try
            {

                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@WorkOrderType", workOrder.WorkOrderType);
                parameters.Add("@AccidentNo", workOrder.AccidentNo);
                parameters.Add("@WorkOrderTitle", workOrder.WorkOrderTitle);
                parameters.Add("@VehicleId", workOrder.VehicleId);
                parameters.Add("@ReportType", workOrder.ReportType);
                parameters.Add("@AccidentPlace", workOrder.AccidentPlace);
                parameters.Add("@DriverFaultInPercent", workOrder.DriverFaultInPercent);
                parameters.Add("@Description", workOrder.Description);
                parameters.Add("@ImagesFilePath", workOrder.ImagesFilePath);
                parameters.Add("@Note", workOrder.Note);
                parameters.Add("@CreatedBy", workOrder.CreatedBy);
                parameters.Add("@IsExternal", workOrder.IsExternal);
                parameters.Add("@WorkOrderStatus", workOrder.WorkOrderStatus);
                parameters.Add("@FK_AgreementId", workOrder.FkAgreementId);
                parameters.Add("@InvoicingStatus", workOrder.InvoicingStatus);
                parameters.Add("@CompanyId", workOrder.CompanyId);
                parameters.Add("@Wfstatus", workOrder.Wfstatus);
                parameters.Add("@AccidentTime", workOrder.AccidentTime);
                parameters.Add("@GregorianDamageDate", workOrder.AccidentDate);
                parameters.Add("@FK_VehicleMovementId", workOrder.FkVehicleMovementId);
                parameters.Add("@VehicleType", workOrder.VehicleType);
                parameters.Add("@RelatedId", workOrder.RelatedId);
                parameters.Add("@FileName", workOrder.FileName);
                parameters.Add("@BranchId", workOrder.BranchId);
                //parameters.Add("@WorkOrderNo", filter.WorkOrderNo);


                var result = await connection.QueryAsync<int>(
                    "WorkOrder.M_WorkOrders_Insert",
                    parameters
                    );


                if (result == null || result.Count() < 1 || result == null)
                    return null;

                workOrder.Id = result.First();

                return workOrder;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error updating workOrder with ID {DamageId}", workOrder.Id);
                throw;
            }
        }

        public async Task<MWorkOrderDTO> UpdateMWorkOrderAsync(MWorkOrderDTO workOrder)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@WorkOrderType", workOrder.WorkOrderType);
                parameters.Add("@AccidentNo", workOrder.AccidentNo);
                parameters.Add("@WorkOrderTitle", workOrder.WorkOrderTitle);
                parameters.Add("@VehicleId", workOrder.VehicleId);
                parameters.Add("@ReportType", workOrder.ReportType);
                parameters.Add("@AccidentPlace", workOrder.AccidentPlace);
                parameters.Add("@DriverFaultInPercent", workOrder.DriverFaultInPercent);
                parameters.Add("@Description", workOrder.Description);
                parameters.Add("@ImagesFilePath", workOrder.ImagesFilePath);
                parameters.Add("@Note", workOrder.Note);
                //parameters.Add("@CreatedBy", workOrder.CreatedBy);
                parameters.Add("@Wfstatus", workOrder.Wfstatus);
                //parameters.Add("@IsExternal", workOrder.IsExternal);
                parameters.Add("@WorkOrderStatus", workOrder.WorkOrderStatus);
                parameters.Add("@FK_AgreementId", workOrder.FkAgreementId);
                parameters.Add("@InvoicingStatus", workOrder.InvoicingStatus);
                parameters.Add("@CompanyId", workOrder.CompanyId);
                parameters.Add("@Id", workOrder.Id);
                parameters.Add("@GregorianDamageDate", workOrder.AccidentDate);
                parameters.Add("@AccidentTime", workOrder.AccidentTime);
                parameters.Add("@FK_VehicleMovementId", workOrder.FkVehicleMovementId);
                parameters.Add("@VehicleType", workOrder.VehicleType);
                parameters.Add("@RelatedId", workOrder.RelatedId);
                parameters.Add("@FileName", workOrder.FileName);
                var result = await connection.QueryAsync<MWorkOrderDTO>(
                    "WorkOrder.M_WorkOrders_Update",
                    parameters
                );

                if (result == null || result.Count() < 1 || result.ElementAt(0) == null)
                    return null;

                // First result set is the updated work order details
                var table1 = (MWorkOrderDTO?)result.ElementAt(0);

                if (table1 == null)
                    return null;

                var updatedWorkOrder = (MWorkOrderDTO)table1;

                return updatedWorkOrder;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error updating workOrder with ID {Id}", workOrder.Id);
                throw;
            }
        }

        public async Task UpdateWorkOrderKMAsync(int workOrderId, decimal receivedKM)
        {
            try
            {
                var parameters = new
                {
                    WorkOrderId = workOrderId,
                    ReceivedKM = receivedKM
                };

                await _database.ExecuteNonReturnProcedure(
                    "M_UpdateWorkOrderKM",
                    parameters);
            }
            catch (Exception e)
            {
                return;
            }
        }

        public async Task<int> UpdateWorkOrderStatusAsync(int workOrderId, int statusId, decimal totalCost = 0)
        {
            var parameters = new
            {
                WorkOrderId = workOrderId,
                StutasId = statusId,
                TotalCost = totalCost
            };

            var result = await _database.ExecuteUpdateProcedure<int>(
                "[WorkOrder].[M_WorkOrders_UpdateWorkOrderStatus]",
                parameters);

            return result;
        }

        public async Task<int> UpdateMAccidentStatusAsync(InsuranceClaimHistory insuranceClaimHistory)
        {
            var parameters = new
            {
                DamageId = insuranceClaimHistory.WorkOrderId,
                CreatedBy = insuranceClaimHistory.CreatedBy,
                Status = insuranceClaimHistory.Status,
                PathId = insuranceClaimHistory.PathId,
                TaqPrice = insuranceClaimHistory.TaqdeeratPrice,
                WSPrice = insuranceClaimHistory.WorkShopPrice,
                TaqreportFilePath = insuranceClaimHistory.TaqdeeratReportFilePath,
                TaqObjectionReason = insuranceClaimHistory.TaqdeeratObjectionReason,
                CompanyId = insuranceClaimHistory.CompanyId,
                BranchId = insuranceClaimHistory.BranchId,
                TaqdeeratFeesFilePath = insuranceClaimHistory.TaqdeeratFeesFilePath,
                AdditionalFeesFilePath = insuranceClaimHistory.AdditionalFeesFilePath,
                EstimationFees = insuranceClaimHistory.EstimationFees,
                EstimateAmount = insuranceClaimHistory.EstimateAmount,
                TowingFees = insuranceClaimHistory.TowingFees,
                IsClientResponsible = insuranceClaimHistory.IsClientReponsible,
                SecondPartyFaultFilePath = insuranceClaimHistory.SecondPartyFaultFilePath,
                CollectionProofFilePath = insuranceClaimHistory.CollectionProofFilePath,
                ExternalWSPrice = insuranceClaimHistory.ExternalWSPrice,
                ExternalWsId = insuranceClaimHistory.ExternalWsId,
                InsurancePricing = insuranceClaimHistory.InsurancePricing,
                FinanceConfirmationFilePath = insuranceClaimHistory.FinanceConfirmationFilePath,

            };

            return await _database.ExecuteUpdateProcedure<int>(
                "WorkOrder.M_AccidentStatus_Update",
                parameters
                );

        }

        public async Task<MWorkOrderDTO> GetMWorkOrderByMasterId(Guid id)
        {
            var parameters = new { MasterId = id };
            return await _database.ExecuteGetByIdProcedure<MWorkOrderDTO>(
                "WorkOrder.M_WorkOrder_FindByMasterId",
                parameters
                );
        }

        public async Task UpdateWorkOrderInvoicingStatus(int workOrderId)
        {

            var parameters = new { WorkOrderId = workOrderId };

            await _database.ExecuteNonReturnProcedure(
                "[WorkOrder].[M_WorkOrder_UpdateInvoicingStatus]",
                parameters
                );
        }

        //Update this mehtod to match our new architecture
        //public void FixDamage(int DamageId, bool isFix)
        //{
        //    SqlCommand cmd = new SqlCommand();
        //    cmd.CommandText = "[Damages].[FixDamage]";
        //    cmd.Parameters.AddWithValue("@DamageId", DamageId);
        //    cmd.Parameters.AddWithValue("@isFix", isFix);
        //    ExDataBase_nonQuery(cmd);

        //}

        public async Task FixWorkOrder(int workOrderId, bool isFix)
        {
            var parameters = new
            {
                WorkOrderId = workOrderId,
                isFix = isFix
            };

            await _database.ExecuteNonReturnProcedure(
                "[WorkOrder].[FixWorkOrder]",
                parameters);
        }
    }

}