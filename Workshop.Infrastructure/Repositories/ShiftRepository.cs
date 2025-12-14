using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class ShiftRepository : IShiftRepository
    {
        private readonly Database _database;

        public ShiftRepository(Database database)
        {
            _database = database;
        }

        public async Task<List<ShiftListItemDTO>> GetAllAsync(ShiftFilterDTO filter)
        {
            var parameters = new
            {
                CompanyId = filter.CompanyId,
                Name = string.IsNullOrEmpty(filter.Name) ? (object)DBNull.Value : filter.Name,
                Code = string.IsNullOrEmpty(filter.Code) ? (object)DBNull.Value : filter.Code,
                PageNumber = filter.Page ?? 1,
                PageSize = filter.PageSize ?? 25
            };

            var result = await _database.ExecuteGetAllStoredProcedure<ShiftListItemDTO>(
                "Shift_GetAll",
                parameters);

            return result?.ToList();
        }

        public async Task<List<ShiftListItemDTO>> GetAllShiftsDDLAsync()
        {
            var result = await _database.ExecuteGetAllStoredProcedure<ShiftListItemDTO>("Shift_GetAll_DDL", null);
            return result?.ToList();
        }

        public async Task<ShiftDTO> GetByIdAsync(int id)
        {
            var parameters = new { Id = id };

            var result = await _database.ExecuteGetByIdProcedure<ShiftDTO>(
                "Shift_GetById",
                parameters);

            return result;
        }

        public async Task<int> InsertAsync(CreateShiftDTO shift)
        {
            var parameters = new
            {
                Code = shift.Code,
                PrimaryName = shift.PrimaryName,
                SecondaryName = shift.SecondaryName,
                Short = shift.Short,
                Color = shift.Color ?? (object)DBNull.Value,
                SpanMidnight = shift.SpanMidnight,
                Sun = shift.Sun,
                Mon = shift.Mon,
                Tue = shift.Tue,
                Wed = shift.Wed,
                Thu = shift.Thu,
                Fri = shift.Fri,
                Sat = shift.Sat,
                DOW = shift.DOW ?? (object)DBNull.Value,
                CreatedBy = shift.CreatedBy,
                CompanyId = shift.CompanyId,
                WorkingFromTime = shift.WorkingFromTime,
                WorkingToTime = shift.WorkingToTime,
                BrakingFromTime = shift.BreakFromTime,
                BreakingToTime = shift.BreakToTime
            };

            var result = await _database.ExecuteAddStoredProcedure<int>(
                "Shift_Insert",
                parameters);

            return result;
        }

        public async Task<int> UpdateAsync(UpdateShiftDTO shift)
        {
            var parameters = new
            {
                Id = shift.Id,
                Code = shift.Code,
                PrimaryName = shift.PrimaryName,
                SecondaryName = shift.SecondaryName,
                Short = shift.Short,
                Color = shift.Color ?? (object)DBNull.Value,
                SpanMidnight = shift.SpanMidnight,
                Sun = shift.Sun,
                Mon = shift.Mon,
                Tue = shift.Tue,
                Wed = shift.Wed,
                Thu = shift.Thu,
                Fri = shift.Fri,
                Sat = shift.Sat,
                DOW = shift.DOW ?? (object)DBNull.Value,
                UpdatedBy = shift.UpdatedBy,
                CompanyId = shift.CompanyId,
                WorkingFromTime = shift.WorkingFromTime,
                WorkingToTime = shift.WorkingToTime,
                BrakingFromTime = shift.BreakFromTime,
                BreakingToTime = shift.BreakToTime
            };

            return await _database.ExecuteUpdateProcedure<int>(
                "Shift_Update",
                parameters);
        }

        public async Task<int> DeleteAsync(int id)
        {
            var parameters = new { Id = id };

            var result = await _database.ExecuteDeleteProcedure<int>(
                "Shift_Delete",
                parameters);

            return result;
        }

    }
}
