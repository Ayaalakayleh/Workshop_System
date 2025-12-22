using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
	public interface IClaimReportRepository
	{
		Task<IEnumerable<ClaimReportDTO>> GetWorkshopClaimReportDataAsync();

	}
}
