using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
	public interface IMaintenanceHistoryRepository
	{
		Task<IEnumerable<ServiceHistoryDTO>> GetMaintenanceHistoryByVehicleIdAsync(int vehicleId, DateTime? fromDate = null, DateTime? toDate = null);
	}
}
