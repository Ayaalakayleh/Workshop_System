using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class MaintenanceCardRepository : IMaintenanceCardRepository
    {
        private readonly Database _database;

        public MaintenanceCardRepository(Database database)
        {
            _database = database;
        }

        public async Task InsertDMaintenanceCard(MaintenanceCardDt card)
        {
            try{
                var parameters = new
                {
                    MovementId = card.MovementId ?? (object)DBNull.Value,
                    WorkOrderId = card.WorkOrderId ?? (object)DBNull.Value,
                    Description = card.Description ?? (object)DBNull.Value,
                    status = card.status ?? (object)DBNull.Value,
                    ServiceId = card.ServiceId ?? (object)DBNull.Value
                };

                await _database.ExecuteNonReturnProcedure("D_MaintemanceCard_Insert", parameters);
            } catch (Exception e)
            {
                return;
            }
        }

        public async Task DeleteDMaintenanceCard(int movementId)
        {
            var parameters = new
            {
                MovementId = movementId
            };

            await _database.ExecuteNonReturnProcedure("D_MaintemanceCard_Delete", parameters);
        }


        public async Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMovementIdAsync(int movementId)
        {
            var parameters = new
            {
                MovementId = movementId
            };
            var result = await _database.ExecuteGetAllStoredProcedure<MaintenanceCardDTO>("D_MaintemanceCard_GetByMovementId", parameters);
            return result?.ToList();
        }

        public async Task<List<MaintenanceCardDTO>> GetDMaintenanceCardsByMasterIdAsync(Guid masterId)
        {
            var parameters = new { MasterId = masterId };
            var result = await _database.ExecuteGetAllStoredProcedure<MaintenanceCardDTO>(
                "D_MaintemanceCard_GetByMasterId",
                parameters);

            return result?.ToList();
        }

    }
}
