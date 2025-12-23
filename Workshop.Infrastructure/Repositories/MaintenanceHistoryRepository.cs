using Dapper;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;


namespace Workshop.Infrastructure.Repositories
{
	public class MaintenanceHistoryRepository : IMaintenanceHistoryRepository
	{
		private readonly DapperContext _context;
		public MaintenanceHistoryRepository(DapperContext context)
		{
			_context = context;
		}
		public async Task<IEnumerable<ServiceHistoryDTO>> GetMaintenanceHistoryByVehicleIdAsync(
		   int vehicleId,
		   DateTime? fromDate = null,
		   DateTime? toDate = null)
		{
			const string spName = "M_GetMaintenanceHistoryByVehicleId";

			var parameters = new DynamicParameters();
			parameters.Add("@VehicleId", vehicleId, DbType.Int32);
			parameters.Add("@FromDate", fromDate, DbType.Date);
			parameters.Add("@ToDate", toDate, DbType.Date);

			using var connection = _context.CreateConnection();
			var result = await connection.QueryAsync<ServiceHistoryDTO>(
				spName,
				parameters,
				commandType: CommandType.StoredProcedure
			);

			return result;
		}
	}
}
