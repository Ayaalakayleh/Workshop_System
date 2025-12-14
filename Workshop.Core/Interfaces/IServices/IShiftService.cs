using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IShiftService
    {
        Task<List<ShiftListItemDTO>> GetAllAsync(ShiftFilterDTO filter);
        Task<List<ShiftListItemDTO>> GetAllShiftsDDLAsync();
        Task<ShiftDTO> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateShiftDTO shift);
        Task<int> UpdateAsync(UpdateShiftDTO shift);
        Task<int> DeleteAsync(int id);
    }
}