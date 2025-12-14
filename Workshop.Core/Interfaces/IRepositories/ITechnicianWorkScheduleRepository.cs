using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface ITechnicianWorkScheduleRepository
    {
        Task<IEnumerable<TechnicianWorkScheduleDTO>> GetAllAsync(int workshopId, string Name, DateTime? Date, int PageNumber = 0);
        Task<TechnicianWorkScheduleDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(CreateTechnicianWorkScheduleDTO dto);
        Task<int> UpdateAsync(UpdateTechnicianWorkScheduleDTO dto);
        Task<int> DeleteAsync(DeleteTechnicianWorkScheduleDTO dto);
        Task<IEnumerable<int>> GetTechniciansFromWorkSchedulesAsync();
    }
}
