using Dapper;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;


namespace Workshop.Infrastructure.Repositories
{
	public class ClaimReportRepository : IClaimReportRepository
	{
		private readonly DapperContext _context;
		public ClaimReportRepository(DapperContext context)
		{
			_context = context;
		}
		public async Task<IEnumerable<ClaimReportDTO>> GetWorkshopClaimReportDataAsync()
		{
			try
			{
				using var connection = _context.CreateConnection();

				var result = await connection.QueryAsync<ClaimReportDTO>(
					"GetWorkshopClaimsReportData",
					commandType: CommandType.StoredProcedure
				);
				return result;

			}
			catch(Exception ex)
			{
				throw ex;
			}
		}
	}
}
