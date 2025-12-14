using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IAllowedTimeRepository
    {
        Task<List<AllowedTimeListItemDTO>> GetAllAsync(AllowedTimeFilterDTO filter);
        Task<AllowedTimeDTO> GetByIdAsync(int id);
        Task<int> InsertAsync(CreateAllowedTimeDTO allowedTime);
        Task<int> UpdateAsync(UpdateAllowedTimeDTO allowedTime);
        Task<int> DeleteAsync(int id);
    }
}