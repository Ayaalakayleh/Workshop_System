
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
	public interface IMaintenanceHistoryService
	{
		Task<IEnumerable<ServiceHistoryDTO>> GetMaintenanceHistoryByVehicleIdAsync(int vehicleId, DateTime? fromDate = null, DateTime? toDate = null);
	}
}
