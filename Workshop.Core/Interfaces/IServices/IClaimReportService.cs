
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
	public interface IClaimReportService
	{
		Task<IEnumerable<ClaimReportDTO>> GetWorkshopClaimReportDataAsync();

	}
}
