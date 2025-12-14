using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class TechnicianWorkScheduleRepository : ITechnicianWorkScheduleRepository
    {
        private readonly Database _database;
        public TechnicianWorkScheduleRepository(Database database)
        {
            _database = database;
        }

        public async Task<IEnumerable<TechnicianWorkScheduleDTO>> GetAllAsync(int workshopId, string Name, DateTime? Date, int PageNumber = 0)
        {
            var parameters = new { 
                workshopId = workshopId,
                Name = Name,
                Date = Date,
                PageNumber = PageNumber
            };
            return await _database.ExecuteGetAllStoredProcedure<TechnicianWorkScheduleDTO>("TechnicianWorkSchedule_GetAll", parameters);
        }

        public async Task<TechnicianWorkScheduleDTO?> GetByIdAsync(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetByIdProcedure<TechnicianWorkScheduleDTO>("TechnicianWorkSchedule_GetById", parameters);
        }

        public async Task<int> AddAsync(CreateTechnicianWorkScheduleDTO dto)
        {
            var parameters = new
            {
                TechnicianIds = dto.TechnicianIds,
                WorkshopId = dto.WorkshopId,
                WorkingDateFrom = dto.WorkingDateFrom,
                WorkingDateTo = dto.WorkingDateTo,
                WorkingTimeFrom = dto.WorkingTimeFrom,
                WorkingTimeTo = dto.WorkingTimeTo,
                FK_SlotMinutes_Id = dto.FK_SlotMinutes_Id,
                CreatedBy = dto.CreatedBy
            };
            return await _database.ExecuteAddStoredProcedure<int>("TechnicianWorkSchedule_Insert", parameters);
        }

        public async Task<int> UpdateAsync(UpdateTechnicianWorkScheduleDTO dto)
        {
            var parameters = new
            {
                Id = dto.Id,
                FK_TechnicianId = dto.FK_TechnicianId,
                WorkshopId = dto.WorkshopId,
                WorkingDateFrom = dto.WorkingDateFrom,
                WorkingDateTo = dto.WorkingDateTo,
                WorkingTimeFrom = dto.WorkingTimeFrom,
                WorkingTimeTo = dto.WorkingTimeTo,
                FK_SlotMinutes_Id = dto.FK_SlotMinutes_Id,
                UpdatedBy = dto.UpdatedBy
            };
            return await _database.ExecuteUpdateProcedure<int>("TechnicianWorkSchedule_Update", parameters);
        }

        public async Task<int> DeleteAsync(DeleteTechnicianWorkScheduleDTO dto)
        {
            return await _database.ExecuteDeleteProcedure<int>("TechnicianWorkSchedule_Delete", dto);
        }

        public async Task<IEnumerable<int>?> GetTechniciansFromWorkSchedulesAsync()
        {
            return await _database.ExecuteGetAllStoredProcedure<int>("TechniciansFromWorkSchedules_GetAll", null);
        }
    }
}
