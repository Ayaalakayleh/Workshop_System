using System.Data;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class WorkshopMovementService : IWorkshopMovementService
    {
        private readonly IWorkshopMovementRepository _workshopMovementRepository;

        public WorkshopMovementService(IWorkshopMovementRepository workshopMovementRepository)
        {
            _workshopMovementRepository = workshopMovementRepository;
        }

        #region MovementIn
        public async Task<bool> WorkshopInvoiceInsertAsync(MovementInvoice invoice)
        {
            try
            {

                var result = await _workshopMovementRepository.WorkshopInvoiceInsertAsync(invoice);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> DExternalWorkshopInvoiceInsertAsync(MovementInvoice invoice)
        {
            return await _workshopMovementRepository.DExternalWorkshopInvoiceInsertAsync(invoice);
        }

        public async Task<VehicleMovementStatusDTO> CheckVehicleMovementStatusAsync(int vehicleId)
        {
            return await _workshopMovementRepository.CheckVehicleMovementStatusAsync(vehicleId);
        }

        public async Task<VehicleMovement> InsertVehicleMovementAsync(VehicleMovement movement)
        {
            return await _workshopMovementRepository.InsertVehicleMovementAsync(movement);
        }

        public async Task InsertMWorkshopMovementStrikesAsync(int movementId, string strikes)
        {
            await _workshopMovementRepository.InsertMWorkshopMovementStrikesAsync(movementId, strikes);
        }

        public async Task InsertMovementDocumentAsync(VehicleMovementDocument movmentDoc)
        {
            await _workshopMovementRepository.InsertMovementDocumentAsync(movmentDoc);
        }
        #endregion

        #region Movements
        public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovement(WorkshopMovementFilter filter)
        {
            return await _workshopMovementRepository.GetAllDWorkshopVehicleMovement(filter);
        }

        public async Task<VehicleMovement> GetVehicleMovementByIdAsync(int movementId)
        {
            return await _workshopMovementRepository.GetVehicleMovementByIdAsync(movementId);
        }

        public async Task<List<VehicleMovementDocument>> GetMovementDocumentsAsync(int movementId)
        {
            return await _workshopMovementRepository.GetMovementDocumentsAsync(movementId);
        }

        public async Task<List<MovementInvoice>> GetWorkshopInvoiceByMovementIdAsync(int movementId)
        {
            return await _workshopMovementRepository.GetWorkshopInvoiceByMovementIdAsync(movementId);
        }

        public async Task<VehicleMovement> GetLastVehicleMovementByVehicleIdAsync(int vehicleId)
        {
            return await _workshopMovementRepository.GetLastVehicleMovementByVehicleIdAsync(vehicleId);
        }

        public async Task<string> GetVehicleMovementStrikeAsync(int movementId)
        {
            return await _workshopMovementRepository.GetVehicleMovementStrikeAsync(movementId);
        }

        public async Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovementDDL(WorkshopMovementFilter filter)
        {
            return await _workshopMovementRepository.GetAllDWorkshopVehicleMovementDDL(filter);
        }

        #endregion

        #region MovementOut


        public async Task UpdateVehicleMovementStatusAync(int workshopId, Guid masterId)
        {
            await _workshopMovementRepository.UpdateVehicleMovementStatusAync(workshopId, masterId);
        }
        #endregion
    }
}