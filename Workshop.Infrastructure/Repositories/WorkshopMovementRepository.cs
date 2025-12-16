using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class WorkshopMovementRepository : IWorkshopMovementRepository
    {
        private readonly Database _database;
        private readonly DapperContext _context;
        public WorkshopMovementRepository(Database database, DapperContext context)
        {
            _database = database;
            _context = context;
        }

        #region MovementIn

        public async Task<int> WorkshopInvoiceInsertAsync(MovementInvoice invoice)
        {
            try
            {
                var parameters = new
                {
                    MovementId = invoice.MovementId,
                    InvoiceNo = invoice.InvoiceNo,
                    TotalAmount = invoice.TotalInvoice,
                    ExternalWorkshopId = invoice.ExternalWorkshopId,
                    MasterId = invoice.MasterId,
                    Vat = invoice.Vat,
                    ConsumptionValueOfSpareParts = invoice.ConsumptionValueOfSpareParts,
                    DeductibleAmount = invoice.DeductibleAmount,
                    WorkOrderId = invoice.WorkOrderId,
                    PartsCost = invoice.PartsCost,
                    LaborCost = invoice.LaborCost,
                    Invoice_Date = invoice.Invoice_Date
                };

                return await _database.ExecuteAddStoredProcedure<int>(
                    "M_WorkshopInvoice_Insert",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error inserting workshop invoice");
                throw;
            }
        }

        public async Task<int> DExternalWorkshopInvoiceInsertAsync(MovementInvoice invoice)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@MovementId", invoice.MovementId);
            parameters.Add("@WorkOrderId", invoice.WorkOrderId);
            parameters.Add("@FilePath", invoice.FilePath);
            parameters.Add("@FileName", invoice.FileName);

            var result = await connection.ExecuteAsync(
                             "D_ExternalWorkshopInvoice_Insert",
                             parameters,
                             commandType: CommandType.StoredProcedure
                           );

            return result;

        }

        public async Task<VehicleMovementStatusDTO> CheckVehicleMovementStatusAsync(int vehicleId)
        {
            var parameters = new
            {
                VehicleId = vehicleId
            };

            var result = await _database.ExecuteGetByIdProcedure<VehicleMovementStatusDTO>(
                "D_CheckVehicleMovementStatus",
                parameters);

            return result;
        }

        public async Task<VehicleMovement> InsertVehicleMovementAsync(VehicleMovement movement)
        {
            try
            {
                var parameters = new
                {
                    VehicleID = movement.VehicleID ?? (object)DBNull.Value,
                    ReceivedMeter = movement.ReceivedMeter ?? (object)DBNull.Value,
                    ResivedDriverId = movement.ResivedDriverId ?? (object)DBNull.Value,
                    ReceivedBranchId = movement.ReceivedBranchId ?? (object)DBNull.Value,
                    ReceivedTime = movement.ReceivedTime ?? (object)DBNull.Value,
                    Note = movement.Note ?? (object)DBNull.Value,
                    CreatedBy = movement.CreatedBy ?? (object)DBNull.Value,
                    MovementOut = movement.MovementOut ?? (object)DBNull.Value,
                    MovementIN = movement.MovementIN ?? (object)DBNull.Value,
                    GregorianMovementDate = movement.GregorianMovementDate ?? (object)DBNull.Value,
                    HijriMovementDate = movement.hijriMovementDate ?? (object)DBNull.Value,
                    IsHijriMovement = movement.ishijriMovement ?? (object)DBNull.Value,
                    CompanyId = movement.CompanyId ?? (object)DBNull.Value,
                    WorkshopId = movement.WorkshopId ?? (object)DBNull.Value,
                    MovementInId = movement.MovementInId ?? (object)DBNull.Value,
                    MasterId = movement.MasterId ?? (object)DBNull.Value,
                    FuelLevelId = movement.FuelLevelId ?? (object)DBNull.Value,
                    GregorianMovementEndDate = movement.GregorianMovementEndDate ?? (object)DBNull.Value,
                    IsRegularMaintenance = movement.IsRegularMaintenance ?? (object)DBNull.Value,
                    EmployeeSignature = movement.EmployeeSignature ?? (object)DBNull.Value,
                    DriverSignature = movement.DriverSignature ?? (object)DBNull.Value,
                    MoveOutWorkshopId = movement.MoveOutWorkshopId ?? (object)DBNull.Value,
                    MoveInWorkshopId = movement.MoveInWorkshopId ?? (object)DBNull.Value,
                    Status = movement.Status ?? (object)DBNull.Value,
                    ExitDriverId = movement.ExitDriverId ?? (object)DBNull.Value,
                    ExitMeter = movement.ExitMeter ?? (object)DBNull.Value,
                    MovementOutId = movement.MovementOutId ?? (object)DBNull.Value,
                    LastVehicleStatus = movement.LastVehicleStatus ?? (object)DBNull.Value,
                    IsExternal = movement.IsExternal ?? (object)DBNull.Value,
                    WorkOrderId = movement.WorkOrderId ?? (object)DBNull.Value,
                    isPart = movement.isPart
                };

                var result = await _database.ExecuteAddStoredProcedure<VehicleMovement>(
                    "D_WorkshopMovement_Insert",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task InsertMWorkshopMovementStrikesAsync(int movementId, string strikes)
        {
            var parameters = new
            {
                MovementId = movementId,
                Strikes = strikes
            };

            await _database.ExecuteNonReturnProcedure(
                "M_WorkshopMovementStrikes_Insert",
                parameters);
        }

        public async Task InsertMovementDocumentAsync(VehicleMovementDocument movmentDoc)
        {
            var parameters = new
            {
                MovementId = movmentDoc.MovementId,
                CreatedBy = movmentDoc.CreatedBy,
                FileName = movmentDoc.FileName,
                FilePath = movmentDoc.FilePath
            };

            await _database.ExecuteNonReturnProcedure(
                "M_InsertMovementDocument",
                parameters);
        }
        #endregion

        #region Movements
        public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovement(WorkshopMovementFilter filter)
        {

            var parameters = new
            {
                VehicleID = filter.VehicleID ?? (object)DBNull.Value,
                PageNumber = filter.page ?? 1,
                branchId = filter.WorkshopId,
                GregorianDate = filter.GregorianDate ?? (object)DBNull.Value,
            };

            var result = await _database.ExecuteGetAllStoredProcedure<VehicleMovement>("D_WorkshopVehicleMovement_Filter", parameters);
            return result?.ToList();
        }

        public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovementDDL(WorkshopMovementFilter filter)
        {
            var parameters = new
            {

                branchId = filter.WorkshopId

            };

            var result = await _database.ExecuteGetAllStoredProcedure<VehicleMovement>("[D_WorkshopVehicleMovementDDL]", parameters);
            return result?.ToList();
        }

        public async Task<VehicleMovement> GetVehicleMovementByIdAsync(int movementId)
        {
            var parameters = new
            {
                VmovementId = movementId
            };
            var result = await _database.ExecuteGetByIdProcedure<VehicleMovement>(
                "D_WorkshopMovement_Find",
                parameters);
            return result;
        }

        public async Task<List<VehicleMovementDocument>> GetMovementDocumentsAsync(int movementId)
        {
            var parameters = new
            {
                MovementId = movementId
            };

            var result = await _database.ExecuteGetAllStoredProcedure<VehicleMovementDocument>("M_GetMovementDocument", parameters);
            return result?.ToList();
        }

        public async Task<List<MovementInvoice>> GetWorkshopInvoiceByMovementIdAsync(int movementId)
        {
            var parameters = new
            {
                MovementId = movementId
            };

            var result = await _database.ExecuteGetAllStoredProcedure<MovementInvoice>("D_WorkshopInvoice_GetByMovementId", parameters);
            return result?.ToList();
        }

        public async Task<VehicleMovement> GetLastVehicleMovementByVehicleIdAsync(int vehicleId)
        {
            try
            {
                var parameters = new
                {
                    VehicleId = vehicleId
                };

                var result = await _database.ExecuteGetByIdProcedure<VehicleMovement>("D_WorkshopMovement_GetLastMovementByVehicleId", parameters);
                return result;
            }
            catch (Exception e)
            {
                throw new(e.Message);
            }
        }

        public async Task<string> GetVehicleMovementStrikeAsync(int movementId)
        {
            var parameters = new
            {
                MovementId = movementId
            };

            var result = await _database.ExecuteGetByIdProcedure<string>("D_WorkshopVehicleMovement_GetStrike", parameters);
            return result;
        }

        #endregion

        #region MovementOut

        public async Task UpdateVehicleMovementStatusAync(int workshopId, Guid masterId)
        {
            var parameters = new
            {
                WorkshopId = workshopId,
                MasterId = masterId
            };

            await _database.ExecuteNonReturnProcedure("D_VehicleMovement_UpdateStatus", parameters);
        }
        #endregion
    }
}