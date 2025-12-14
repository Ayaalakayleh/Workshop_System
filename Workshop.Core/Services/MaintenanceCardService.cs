using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class MaintenanceCardService : IMaintenanceCardService
    {
        private readonly IMaintenanceCardRepository _repository;

        public MaintenanceCardService(IMaintenanceCardRepository repository)
        {
            _repository = repository;
        }

        public async Task InsertDMaintenanceCardAsync(MaintenanceCardDTO maintenanceCard)
        {
            MaintenanceCardDt dtCard = new MaintenanceCardDt();
            await _repository.DeleteDMaintenanceCard((int)maintenanceCard.MovementId);

            dtCard.MovementId = maintenanceCard.MovementId.Value;
            dtCard.ServiceId = maintenanceCard.ServiceId;
            dtCard.WorkOrderId = maintenanceCard.WorkOrderId;
            dtCard.Description = maintenanceCard.Description;
            dtCard.status = maintenanceCard.status;

            await _repository.InsertDMaintenanceCard(dtCard);
        }

        public async Task DeleteDMaintenanceCardAsync(int movementId)
        {
            await _repository.DeleteDMaintenanceCard(movementId);
        }

        public async Task UpdateDMaintenanceCardAsync(MaintenanceCardDTO maintenanceCard)
        {
            await _repository.DeleteDMaintenanceCard(maintenanceCard.MovementId.Value);
            await InsertDMaintenanceCardAsync(maintenanceCard);
        }

        public async Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMovementIdAsync(int movementId)
        {
            return await _repository.GetDMaintenanceCardsByMovementIdAsync(movementId);
        }

        public async Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMasterIdAsync(Guid masterId)
        {

            return await _repository.GetDMaintenanceCardsByMasterIdAsync(masterId);
        }
    }
}
