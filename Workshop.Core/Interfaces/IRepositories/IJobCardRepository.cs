using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IJobCardRepository
    {
        Task<JobCardDTO> GetJobCardByMasterIdAsync(Guid id);

    }
}
