using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IShiftRepository _shiftRepository;

        public ShiftService(IShiftRepository shiftRepository)
        {
            _shiftRepository = shiftRepository;
        }

        public async Task<List<ShiftListItemDTO>> GetAllAsync(ShiftFilterDTO filter)
        {
            ValidateCompanyId(filter.CompanyId);
            return await _shiftRepository.GetAllAsync(filter);
        }

        public async Task<List<ShiftListItemDTO>> GetAllShiftsDDLAsync()
        {
            return await _shiftRepository.GetAllShiftsDDLAsync();
        }

        public async Task<ShiftDTO> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid shift ID");

            var shift = await _shiftRepository.GetByIdAsync(id);
            if (shift == null) throw new KeyNotFoundException("Shift not found");

            return shift;
        }

        public async Task<int> CreateAsync(CreateShiftDTO shift)
        {
            ValidateBaseShift(shift);
            if (shift.CreatedBy <= 0) throw new ArgumentException("Invalid created by user ID");

            return await _shiftRepository.InsertAsync(shift);
        }

        public async Task<int> UpdateAsync(UpdateShiftDTO shift)
        {
            ValidateBaseShift(shift);
            if (shift.Id <= 0) throw new ArgumentException("Invalid shift ID");
            if (shift.UpdatedBy <= 0) throw new ArgumentException("Invalid updated by user ID");

            var existing = await _shiftRepository.GetByIdAsync(shift.Id);
            if (existing == null) throw new KeyNotFoundException("Shift not found");

            return await _shiftRepository.UpdateAsync(shift);
        }

        public async Task<int> DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid shift ID");

            var existing = await _shiftRepository.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException("Shift not found");

            return await _shiftRepository.DeleteAsync(id);
        }

        private void ValidateCompanyId(int companyId)
        {
            if (companyId <= 0) throw new ArgumentException("Invalid company ID");
        }

        private void ValidateBaseShift(BaseShiftDTO shift)
        {
            if (string.IsNullOrEmpty(shift.Code)) throw new ArgumentException("Code is required");
            if (string.IsNullOrEmpty(shift.PrimaryName)) throw new ArgumentException("Primary name is required");
            if (string.IsNullOrEmpty(shift.SecondaryName)) throw new ArgumentException("Secondary name is required");
            if (string.IsNullOrEmpty(shift.Short)) throw new ArgumentException("Short name is required");
            ValidateCompanyId(shift.CompanyId);
        }
    }
}