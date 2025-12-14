using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IMaintenanceCardRepository
    {

        Task InsertDMaintenanceCard(MaintenanceCardDt card);
        Task DeleteDMaintenanceCard(int movementId);
        Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMovementIdAsync(int movementId);
        Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMasterIdAsync(Guid masterId);
    }
}
