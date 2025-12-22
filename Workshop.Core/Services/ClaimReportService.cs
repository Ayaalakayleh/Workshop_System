using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
	public class ClaimReportService : IClaimReportService
	{
		private readonly IClaimReportRepository _repository;

		public ClaimReportService(IClaimReportRepository repository)
		{
			_repository = repository;
		}

		public async Task<IEnumerable<ClaimReportDTO>> GetWorkshopClaimReportDataAsync()
		{
			return await _repository.GetWorkshopClaimReportDataAsync();
		}
	}

}
