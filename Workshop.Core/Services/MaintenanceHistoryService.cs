using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
	public class MaintenanceHistoryService : IMaintenanceHistoryService
	{
		private readonly IMaintenanceHistoryRepository _repository;

		public MaintenanceHistoryService(IMaintenanceHistoryRepository repository)
		{
			_repository = repository;
		}

		public async Task<IEnumerable<ServiceHistoryDTO>> GetMaintenanceHistoryByVehicleIdAsync(
			int vehicleId, DateTime? fromDate = null, DateTime? toDate = null)
		{
			return await _repository.GetMaintenanceHistoryByVehicleIdAsync(vehicleId, fromDate, toDate);
		}
	}

}
