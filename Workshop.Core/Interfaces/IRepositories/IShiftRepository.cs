using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IShiftRepository
    {
        Task<List<ShiftListItemDTO>> GetAllAsync(ShiftFilterDTO filter);
        Task<List<ShiftListItemDTO>> GetAllShiftsDDLAsync();
        Task<ShiftDTO> GetByIdAsync(int id);
        Task<int> InsertAsync(CreateShiftDTO shift);
        Task<int> UpdateAsync(UpdateShiftDTO shift);
        Task<int> DeleteAsync(int id);
    }
}