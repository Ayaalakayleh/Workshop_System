using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IAllowedTimeService
    {
        Task<List<AllowedTimeListItemDTO>> GetAllAsync(AllowedTimeFilterDTO filter);
        Task<AllowedTimeDTO> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateAllowedTimeDTO allowedTime);
        Task<int> UpdateAsync(UpdateAllowedTimeDTO allowedTime);
        Task<int> DeleteAsync(int id);
    }
}