using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class AllowedTimeService : IAllowedTimeService
    {
        private readonly IAllowedTimeRepository _allowedTimeRepository;

        public AllowedTimeService(IAllowedTimeRepository allowedTimeRepository)
        {
            _allowedTimeRepository = allowedTimeRepository;
        }

        public async Task<List<AllowedTimeListItemDTO>> GetAllAsync(AllowedTimeFilterDTO filter)
        {
            return await _allowedTimeRepository.GetAllAsync(filter);
        }

        public async Task<AllowedTimeDTO> GetByIdAsync(int id)
        {

            var allowedTime = await _allowedTimeRepository.GetByIdAsync(id);

            return allowedTime;
        }

        public async Task<int> CreateAsync(CreateAllowedTimeDTO allowedTime)
        {


            return await _allowedTimeRepository.InsertAsync(allowedTime);
        }

        public async Task<int> UpdateAsync(UpdateAllowedTimeDTO allowedTime)
        {

            return await _allowedTimeRepository.UpdateAsync(allowedTime);
        }

        public async Task<int> DeleteAsync(int id)
        {
        

            return await _allowedTimeRepository.DeleteAsync(id);
        }

    }
}