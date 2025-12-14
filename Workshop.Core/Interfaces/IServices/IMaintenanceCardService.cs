using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IMaintenanceCardService
    {

        Task InsertDMaintenanceCardAsync(MaintenanceCardDTO card);
        Task UpdateDMaintenanceCardAsync(MaintenanceCardDTO maintenanceCard);
        Task DeleteDMaintenanceCardAsync(int movementId);
        Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMovementIdAsync(int movementId);
        Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMasterIdAsync(Guid masterId);
    }
}
