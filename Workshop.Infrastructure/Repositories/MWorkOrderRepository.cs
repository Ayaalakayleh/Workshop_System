using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.ComponentModel.Design;
using System.Data;
using System.Security.Claims;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.City;
using Workshop.Core.DTOs.ExternalWorkshopExp;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;
using static System.Net.Mime.MediaTypeNames;

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
				parameters.Add("@ModifyBy ", workOrder.ModifyBy);
				parameters.Add("@ModifyAt ", workOrder.ModifyAt);
				parameters.Add("@hijriDamagetDate", workOrder.HijriDamagetDate);
				parameters.Add("@BranchId", workOrder.BranchId);
				parameters.Add("@IsFix", workOrder.IsFix);
				parameters.Add("@IsExternal", workOrder.IsExternal);
				parameters.Add("@WorkOrderStatus", workOrder.WorkOrderStatus);
				parameters.Add("@FK_AgreementId", workOrder.FkAgreementId);
				parameters.Add("@InvoicingStatus", workOrder.InvoicingStatus);
				parameters.Add("@CompanyId", workOrder.CompanyId);
				parameters.Add("@Wfstatus", workOrder.Wfstatus);
				parameters.Add("@AccidentTime", workOrder.AccidentTime);
				parameters.Add("@GregorianDamageDate", workOrder.GregorianDamageDate);
				parameters.Add("@FK_VehicleMovementId", workOrder.FkVehicleMovementId);
				parameters.Add("@TotalCost", workOrder.TotalCost);
				parameters.Add("@VehicleType", workOrder.VehicleType);
				parameters.Add("@RelatedId", workOrder.RelatedId);
				parameters.Add("@ReportNo", workOrder.ReportNo);
				parameters.Add("@FileName", workOrder.FileName);
				parameters.Add("@IsInsuraceDamege", workOrder.IsInsuraceDamege);
				parameters.Add("@InsuranceCompanyId", workOrder.InsuranceCompanyId);
				parameters.Add("@ThereIsASecondParty", workOrder.ThereIsAsecondParty);
				parameters.Add("@SecondPartyFaultInPercent", workOrder.SecondPartyFaultInPercent);
				parameters.Add("@damagesServices", workOrder.DMaintenanceCards != null ? JsonConvert.SerializeObject(workOrder.DMaintenanceCards.Where(a => a.ServiceId > 0)) : null);
				parameters.Add("@InsuranceWorkshop", workOrder.InsuranceWorkshop);
				parameters.Add("@InsuranceWorkshopConcernedPerson", workOrder.InsuranceWorkshopConcernedPerson);
				parameters.Add("@ProjectId", workOrder.ProjectId);
				parameters.Add("@CustomerId", workOrder.CustomerId);
				parameters.Add("@CityId", workOrder.CityId);
				parameters.Add("@HasEngineDamage", workOrder.HasEngineDamage);
				parameters.Add("@HasChassisDamage", workOrder.HasChassisDamage);
				parameters.Add("@CanRepaired", workOrder.CanRepaired);
				parameters.Add("@HasTransmissionDamage", workOrder.HasTransmissionDamage);
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
				parameters.Add("@Wfstatus", workOrder.Wfstatus);
				parameters.Add("@WorkOrderStatus", workOrder.WorkOrderStatus);
				parameters.Add("@FK_AgreementId", workOrder.FkAgreementId);
				parameters.Add("@InvoicingStatus", workOrder.InvoicingStatus);
				parameters.Add("@CompanyId", workOrder.CompanyId);
				parameters.Add("@Id", workOrder.Id);
				parameters.Add("@GregorianDamageDate", workOrder.GregorianDamageDate);
				parameters.Add("@AccidentTime", workOrder.AccidentTime);
				parameters.Add("@FK_VehicleMovementId", workOrder.FkVehicleMovementId);
				parameters.Add("@VehicleType", workOrder.VehicleType);
				parameters.Add("@RelatedId", workOrder.RelatedId);
				parameters.Add("@FileName", workOrder.FileName);
				parameters.Add("@ModifyBy", workOrder.ModifyBy);
				parameters.Add("@ModifyAt", workOrder.ModifyAt);
				parameters.Add("@hijriDamagetDate", workOrder.HijriDamagetDate);
				parameters.Add("@IsFix", workOrder.IsFix);
				parameters.Add("@ReportNo", workOrder.ReportNo);
				parameters.Add("@TotalCost", workOrder.TotalCost);
				parameters.Add("@IsInsuraceDamege", workOrder.IsInsuraceDamege);
				parameters.Add("@InsuranceCompanyId", workOrder.InsuranceCompanyId);
				parameters.Add("@ThereIsASecondParty", workOrder.ThereIsAsecondParty);
				parameters.Add("@SecondPartyFaultInPercent", workOrder.SecondPartyFaultInPercent);
				parameters.Add("@ClaimType", workOrder.ClaimType);
				parameters.Add("@InsuranceWorkshop", workOrder.InsuranceWorkshop);
				parameters.Add("@InsuranceWorkshopConcernedPerson", workOrder.InsuranceWorkshopConcernedPerson);
				parameters.Add("@ProjectId", workOrder.ProjectId);
				parameters.Add("@CustomerId", workOrder.CustomerId);
				parameters.Add("@CityId", workOrder.CityId);
				parameters.Add("@WorkshopId", workOrder.WorkshopId);
				parameters.Add("@HasInsurance", workOrder.HasInsurance);
				parameters.Add("@HasEngineDamage", workOrder.HasEngineDamage);
				parameters.Add("@HasChassisDamage", workOrder.HasChassisDamage);
				parameters.Add("@HasTransmissionDamage", workOrder.HasTransmissionDamage);
				parameters.Add("@CanRepaired", workOrder.CanRepaired);
				parameters.Add("@MasterId", workOrder.MasterId);
				parameters.Add("@IsFinished", workOrder.IsFinished);
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
				WorkOrderId = insuranceClaimHistory.WorkOrderId,
				CreatedBy = insuranceClaimHistory.CreatedBy,
				Status = insuranceClaimHistory.Status,
				PathId = insuranceClaimHistory.PathId,
				TaqPrice = insuranceClaimHistory.TaqdeeratPrice,
				WSPrice = insuranceClaimHistory.WSPrice,
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

		public async Task<int> M_UniqueAccidentNo(long DamageId, string ReportNo)
		{
			try
			{
				var parameters = new
				{
					ReportNo = ReportNo,
					DamageId = DamageId
				};

				return await _database.ExecuteUpdateProcedure<int>(
					"[WorkOrder].[M_UniqueAccidentNo]",
					parameters);
			}
			catch (Exception e)
			{
				return 0;
			}
		}

		public async Task<List<MWorkOrderDetailDTO>> M_WorkOrderDetails_GetByWorkOrderID(int DamageId)
		{
			using var connection = _context.CreateConnection();

			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@DamageId", DamageId);

				var result = await connection.QueryAsync<MWorkOrderDetailDTO>(
					"[WorkOrder].[M_WorkOrderDetails_GetByWorkOrderID]",
					parameters
				);
				return result.ToList()
					;
			}
			catch (Exception e)
			{

				var r = e;
			}
			return null;
		}



		public async Task WorkOrderReport_Insert(List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument)
		{

			var parameters = new { @WorkOrderDocument = _database.ToDataTable<MWorkOrdersDetailsDocumentDTO>(WorkOrderDocument) };

			await _database.ExecuteNonReturnProcedure(
				"[WorkOrder].[D_WorkOrderReport_Insert]",
				parameters
				);
		}
		public async Task WorkOrderDetalsDoc_Insert(List<MWorkOrdersDetailsDocumentDTO> WorkOrderDocument)
		{
			try
			{

				var parameters = new { @WorkOrderDocument = _database.ToDataTable<MWorkOrdersDetailsDocumentDTO>(WorkOrderDocument) };

				await _database.ExecuteNonReturnProcedure(
					"[WorkOrder].[M_WorkOrdersDetailsDocument_Insert]",
					parameters
					);
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<MWorkOrderDetail> WorkOrderDetails_Insert(MWorkOrderDetail workOrder)
		{
			try
			{

				using var connection = _context.CreateConnection();
				var parameters = new DynamicParameters();
				parameters.Add("@PartName", workOrder.PartName);
				parameters.Add("@WorkOrderType", workOrder.WorkOrderType);
				parameters.Add("@WorkOrderId", workOrder.WorkOrderId);
				parameters.Add("@Note", workOrder.Note);
				parameters.Add("@isFix", workOrder.IsFix);
				var result = await connection.QueryAsync<int>(
			"WorkOrder.M_WorkOrderDetails_Insert",
			parameters
			);


				if (result == null || result.Count() < 1 || result == null)
					return null;

				workOrder.Id = result.First();

				return workOrder;
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

				using var connection = _context.CreateConnection();
				var parameters = new DynamicParameters();
				parameters.Add("@Id", workOrder.Id);
				parameters.Add("@PartName", workOrder.PartName);
				parameters.Add("@WorkOrderType", workOrder.WorkOrderType);
				parameters.Add("@WorkOrderId", workOrder.WorkOrderId);
				parameters.Add("@Note", workOrder.Note);
				parameters.Add("@isFix", workOrder.IsFix);
				var result = await connection.QueryAsync<int>(
			"[WorkOrder].[M_WorkOrderDetails_Update]",
			parameters
			);


				if (result == null || result.Count() < 1 || result == null)
					return null;

				workOrder.Id = result.First();

				return workOrder;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<List<MWorkOrdersDetailsDocument>> WorkOrdersDetailsDocument_Get(int WorkOrderDetailsId)
		{
			using var connection = _context.CreateConnection();

			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@WorkOrderDetailsId", WorkOrderDetailsId);

				var result = await connection.QueryAsync<MWorkOrdersDetailsDocument>(
					"[WorkOrder].[M_WorkOrdersDetailsDocument_Get]",
					parameters
				);
				return result.ToList();
			}
			catch (Exception e)
			{

				var r = e;
			}
			return null;
		}
		public async Task<List<MWorkOrdersDetailsDocument>> WorkOrderReports_Get(int WorkOrderId)
		{
			using var connection = _context.CreateConnection();

			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@WorkOrderId", WorkOrderId);

				var result = await connection.QueryAsync<MWorkOrdersDetailsDocument>(
					"[WorkOrder].[D_WorkOrderReport_GetByWorkOrder]",
					parameters
				);
				return result?.ToList();
			}
			catch (Exception e)
			{

				var r = e;
			}
			return null;
		}
		public async Task DeleteReportByWorkOrderId(int WorkOrderId)
		{
			using var connection = _context.CreateConnection();

			try
			{
				var parameters = new DynamicParameters();
				parameters.Add("@WorkOrderId", WorkOrderId);

				var result = await connection.QueryAsync<MWorkOrderDTO>(
					"[WorkOrder].[D_WorkOrderReport_DeleteByWorkOrderId]",
					parameters
				);
			}
			catch (Exception e)
			{

				var r = e;
			}
		}

		public async Task<int> DeleteMWorkOrderDetailsAsync(int id)
		{
			try
			{
				var parameters = new { Id = id };

				return await _database.ExecuteDeleteProcedure<int>(
					"[WorkOrder].[M_WorkOrderDetails_Delete]",
					parameters);
			}
			catch (Exception ex)
			{
				throw;
			}
		}
		public async Task<int> DeleteMWorkOrderDetailsDocAsync(int id)
		{
			try
			{
				var parameters = new { Id = id };

				return await _database.ExecuteDeleteProcedure<int>(
					"[WorkOrder].[M_WorkOrdersDetailsDocument_DeleteByWorkOrderId]",
					parameters);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<int> CheckAccidentNo(string accidentNo)
		{
			try
			{
				var parameters = new { AccidentNo = accidentNo };

				return await _database.ExecuteUpdateProcedure<int>(
					"[WorkOrder].[CheckAccidentNo]",
					parameters
				);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<List<WorkshopWorkOrderStatusReportDTO>> GetWorkshopWorkOrdersStatus(int? vehicleId, DateTime? fromDate, DateTime? toDate, int? externalVehicleId)
		{
			using var connection = _context.CreateConnection();


			var result = await connection.QueryAsync<WorkshopWorkOrderStatusReportDTO>(
				"GetWorkshopWorkOrdersStatus",
				new
				{
					vehicleId,
					fromDate,
					toDate,
					ExternalVehicleId = externalVehicleId
				},
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();

		}

		public async Task<bool> WorkOrder_FinishWorkflow(int id, int status)
		{
			using var connection = _context.CreateConnection();

			await connection.ExecuteAsync(
				"[WorkOrder].[M_WorkOrder_FinishWorkflow]",
				new { Id = id, Status = status },
				commandType: CommandType.StoredProcedure
			);

			return true;
		}
		public async Task<bool> UpdateClaimStatus(InsuranceClaimHistory model)
		{
			using var connection = _context.CreateConnection();

			await connection.ExecuteAsync(
				"dbo.M_ClaimHistory_Insert",
				new
				{
					WorkOrderId = model.WorkOrderId,
					CreatedBy = model.CreatedBy,
					CreatedAt = model.CreatedAt,
					Status = model.Status,
					Note = model.Note,
					RefrenceNo = model.RefrenceNo,
					TotalInsurance = model.TotalInsurance,
					RejectReason = model.RejectReason,
					Reason = model.Reason,
					FullInsuranceStatus = model.FullInsuranceStatus,
					ClaimFilePath = model.ClaimFilePath,
					CliamNumber = model.ClaimNumber,
					ClaimStatusFilePath = model.ClaimStatusFilePath,
					ClaimAmountReceivedDate = model.ClaimAmountReceivedDate
				},
				commandType: CommandType.StoredProcedure
			);

			return true;
		}
		public async Task<List<InsuranceClaimHistory>> GetClaimHistory(int workOrderId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<InsuranceClaimHistory>(
				"[M_ClaimHistory_GetAllByWorkOrderId]",
				new { WorkOrderId = workOrderId },
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<ClaimFileDTO> GetClaimFile(int workOrderId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryFirstOrDefaultAsync<ClaimFileDTO>(
				"M_WorkOrder_GetClaimFileByWorkOrderId",
				new { WorkOrderId = workOrderId },
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
		public async Task<List<InsuranceClaimHistory>> GetVehicleWithOpenClaim(int companyId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<InsuranceClaimHistory>(
				"M_ClaimHistory_GetVehicleWithOpenclaim",
				new { CompanyId = companyId },
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<InsuranceClaimStatistaic> GetInsuranceClaimStatistaic(int companyId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryFirstOrDefaultAsync<InsuranceClaimStatistaic>(
				"M_Claim_GetClaimStats",
				new { CompanyId = companyId },
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
		public async Task<List<WorkOrderClaimEarnings>> GetEarningsFromAccidentClaim(int companyId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<WorkOrderClaimEarnings>(
				"GetEarningsFromAccidentClaim",
				new { CompanyId = companyId },
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<List<AmountFromClientsForAccidents>> M_Claim_GetCustomerwithDeductibleAmount(int companyId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<AmountFromClientsForAccidents>(
				"M_Claim_GetCustomerwithDeductibleAmount",
				new { CompanyId = companyId },
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<bool> UpdateClaimAmountReceivedDate(MWorkOrderDTO model)
		{
			using var connection = _context.CreateConnection();

			await connection.ExecuteAsync(
				"M_UpdateClaimAmountReceivedDate",
				new
				{
					Id = model.Id,
					ModifyBy = model.ModifyBy,
					ClaimAmountReceivedDate = model.ClaimAmountReceivedDate,
					TotalInsurance = model.TotalInsurance,
					ClaimAmountFilePath = model.ClaimAmoountFilePath
				},
				commandType: CommandType.StoredProcedure
			);

			return true;
		}

		public async Task<int> M_WorkOrderStatus_Back(int workOrderId, int status)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.ExecuteScalarAsync<int>(
				"[WorkOrder].[M_WorkOrderStatus_Back]",
				new
				{
					Status = status,
					WorkOrderid = workOrderId
				},
				commandType: CommandType.StoredProcedure
			);

			return result;
		}

		public async Task<int> CheckAccidentCountPerCustomer(int customerId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.ExecuteScalarAsync<int>(
				"[WorkOrder].[CheckAccidentCountPerCustomer]",
				new { CustomerId = customerId },
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
		public async Task<int> M_SendToLegalAfter3Weeks()
		{
			using var connection = _context.CreateConnection();

			var result = await connection.ExecuteScalarAsync<int>(
				"[WorkOrder].[M_SendToLegalAfter3Weeks]",
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
		public async Task<List<MTaqdeeratDocumentDTO>> M_TaqdeeratDocs_GetById(int workOrderId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<MTaqdeeratDocumentDTO>(
				"[WorkOrder].[M_TaqdeeratDocs_GetById]",
				new { WorkOrderId = workOrderId },
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<int> M_AddTaqdeeratDoc(MTaqdeeratDocumentDTO model)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.ExecuteScalarAsync<int>(
				"[WorkOrder].[M_AddTaqdeeratDoc]",
				new
				{
					WorkOrderId = model.WorkOrderId,
					FilePath = model.FilePath,
					TaqFileName = model.TaqFileName,
					CreatedBy = model.CreatedBy
				},
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
		public async Task<int> M_TaqDocs_Delete(int workOrderId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.ExecuteScalarAsync<int>(
				"[WorkOrder].[M_TaqDocs_Delete]",
				new { WorkOrderId = workOrderId },
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
		public async Task<List<WorkOrderStatusDTO>> GetAllWorkflowStatus()
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<WorkOrderStatusDTO>(
				"[WorkOrder].[GetAllWorkflowStatus]",
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<int> M_SaveStatusRoleId(List<StatusRoleViewModel> liStatusRole)
		{
			using var connection = _context.CreateConnection();

			var table = new DataTable();
			table.Columns.Add("StatusId", typeof(int));
			table.Columns.Add("RoleId", typeof(int));

			foreach (var item in liStatusRole)
			{
				table.Rows.Add(item.StatusId, item.RoleId);
			}

			var result = await connection.ExecuteScalarAsync<int>(
				"[WorkOrder].[M_SaveStatusRoleId]",
				new
				{
					StatusRoles = table.AsTableValuedParameter("StatusRoleType")
				},
				commandType: CommandType.StoredProcedure
			);

			return result;
		}


		public async Task<bool> WorkOrders_VehicleWorkOrderStatus(int vehicleId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.ExecuteScalarAsync<int>(
				"[WorkOrder].[WorkOrders_VehicleWorkOrderStatus]",
				new
				{
					VehicleId = vehicleId,

				},
				commandType: CommandType.StoredProcedure
			);

			return result > 0 ? false : true;
		}
		public async Task<List<VehicleWorkOrderSummery>> GetDamagesSummeryByVehicleId(WorkOrderFilterDTO damageFilter)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<VehicleWorkOrderSummery>(
				"[Damage].[GetDamagesSummeryByVehicleId]",
				new
				{
					VehicleId = damageFilter.VehicleID,
					FromDate = damageFilter.FromDate,
					ToDate = damageFilter.ToDate,
					StatusId = damageFilter.WorkOrderStatus
				},
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<List<WorkOrderInsuranceDetails>> GetInsuranceDetails(int companyId, DateTime? fromDate, DateTime? toDate)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<WorkOrderInsuranceDetails>(
				"[dbo].[D_InsuranceDetails_Get]",
				new
				{
					CompanyId = companyId,
					FromDate = fromDate,
					ToDate = toDate
				},
				commandType: CommandType.StoredProcedure
			);

			return result.ToList();
		}
		public async Task<IEnumerable<VehicleWorkOrdersSummery>> GetWorkOrdersSummeryByVehicleIdAsync(int vehicleId, int companyId)
		{
			var parameters = new DynamicParameters();
			parameters.Add("@VehicleId", vehicleId);
			parameters.Add("@CompanyId", companyId);

			using var connection = _context.CreateConnection();

			var result = await connection.QueryAsync<VehicleWorkOrdersSummery>(
				"WorkOrder_GetWorkOrdersSummeryByVehicleId",
				parameters,
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
		public async Task<string?> GetLastMaintenanceMovementStrikeAsync(int vehicleId)
		{
			using var connection = _context.CreateConnection();

			var result = await connection.ExecuteScalarAsync<string?>(
				"GetLastMaintenanceMovementStrike",
				new { vehicleId },
				commandType: CommandType.StoredProcedure
			);

			return result;
		}


	}

}