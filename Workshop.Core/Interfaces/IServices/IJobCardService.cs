using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IJobCardService
    {
        Task<JobCardDTO> GetJobCardByMasterIdAsync(Guid id);

    }
}
