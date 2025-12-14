
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IWorkshopMovementService
    {
        Task<bool> WorkshopInvoiceInsertAsync(MovementInvoice invoice);
        Task<int> DExternalWorkshopInvoiceInsertAsync(MovementInvoice invoice);
        Task<VehicleMovementStatusDTO> CheckVehicleMovementStatusAsync(int vehicleId);
        Task<VehicleMovement> InsertVehicleMovementAsync(VehicleMovement movement);
        Task InsertMWorkshopMovementStrikesAsync(int movementId, string strikes);
        Task InsertMovementDocumentAsync(VehicleMovementDocument movmentDoc);
        Task<List<VehicleMovement>> GetAllDWorkshopVehicleMovement(WorkshopMovementFilter filter);
        Task<VehicleMovement> GetVehicleMovementByIdAsync(int movementId);
        Task<List<VehicleMovementDocument>> GetMovementDocumentsAsync(int movementId);
        Task<List<MovementInvoice>> GetWorkshopInvoiceByMovementIdAsync(int movementId);
        Task<VehicleMovement> GetLastVehicleMovementByVehicleIdAsync(int vehicleId);
        Task<string> GetVehicleMovementStrikeAsync(int movementId);
        Task UpdateVehicleMovementStatusAync(int workshopId, Guid masterId);
    }
}