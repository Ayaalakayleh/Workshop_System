using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class AllowedTimeRepository : IAllowedTimeRepository
    {
        private readonly Database _database;

        public AllowedTimeRepository(Database database)
        {
            _database = database;
        }

        public async Task<List<AllowedTimeListItemDTO>> GetAllAsync(AllowedTimeFilterDTO filter)
        {
            var parameters = new
            {
                FK_Make = filter.Make.HasValue ? filter.Make.Value : (object)DBNull.Value,
                FK_Model = filter.Model.HasValue ? filter.Model.Value : (object)DBNull.Value,
                Year = filter.Year.HasValue ? filter.Year.Value : (object)DBNull.Value,
                Fk_RTSCode = filter.RTSCode.HasValue ? filter.RTSCode.Value : (object)DBNull.Value,
                PageNumber = filter.Page ?? 1,
                PageSize = filter.PageSize ?? 25
            };

            var result = await _database.ExecuteGetAllStoredProcedure<AllowedTimeListItemDTO>(
                "AllowedTime_GetAll",
                parameters);

            return result?.ToList();
        }

        public async Task<AllowedTimeDTO> GetByIdAsync(int id)
        {
            var parameters = new { Id = id };

            var result = await _database.ExecuteGetByIdProcedure<AllowedTimeDTO>(
                "AllowedTime_GetById",
                parameters);

            return result;
        }

        public async Task<int> InsertAsync(CreateAllowedTimeDTO allowedTime)
        {

            var parameters = new
            {
                FK_Make = allowedTime.Make,
                FK_Model = allowedTime.Model,
                Year = allowedTime.Year,
                Fk_RTSCode = allowedTime.RTSCode,
                AllowedHours = allowedTime.AllowedHours,
                SupervisorOverride = allowedTime.SupervisorOverride ?? (object)DBNull.Value,
                VehicleClass = allowedTime.VehicleClass,
                CreatedBy = allowedTime.CreatedBy
            };

            var result = await _database.ExecuteAddStoredProcedure<int>(
                "AllowedTime_Insert",
                parameters);

            return result;

        }

        public async Task<int> UpdateAsync(UpdateAllowedTimeDTO allowedTime)
        {
            var parameters = new
            {
                Id = allowedTime.Id,
                FK_Make = allowedTime.Make,
                FK_Model = allowedTime.Model,
                Year = allowedTime.Year,
                Fk_RTSCode = allowedTime.RTSCode,
                AllowedHours = allowedTime.AllowedHours,
                SupervisorOverride = allowedTime.SupervisorOverride ?? (object)DBNull.Value,
                VehicleClass = allowedTime.VehicleClass,
                UpdatedBy = allowedTime.UpdatedBy
            };

            return await _database.ExecuteUpdateProcedure<int>(
                "AllowedTime_Update",
                parameters);
        }

        public async Task<int> DeleteAsync(int id)
        {
            var parameters = new { Id = id };

            return await _database.ExecuteDeleteProcedure<int>(
                "AllowedTime_Delete",
                parameters);
        }
    }
}