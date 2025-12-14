using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface ITechnicianWorkScheduleService
    {
        Task<IEnumerable<TechnicianWorkScheduleDTO>> GetAllAsync(int workshopId, string Name, DateTime? Date, string language, int PageNumber = 0);
        Task<TechnicianWorkScheduleDTO?> GetByIdAsync(int id, string language);
        Task<int> AddAsync(CreateTechnicianWorkScheduleDTO dto);
        Task<int> UpdateAsync(UpdateTechnicianWorkScheduleDTO dto);
        Task<int> DeleteAsync(DeleteTechnicianWorkScheduleDTO dto);
        Task<IEnumerable<int>> GetTechniciansFromWorkSchedulesAsync();

    }
}
