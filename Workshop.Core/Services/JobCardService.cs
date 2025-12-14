using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class JobCardService: IJobCardService
    {
        private readonly IJobCardRepository _repository;
        
        public JobCardService(IJobCardRepository repository) {
            _repository = repository;
        }

        public async Task<JobCardDTO> GetJobCardByMasterIdAsync(Guid id)
        {
            return await _repository.GetJobCardByMasterIdAsync(id);
        }
    }
}
