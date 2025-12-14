using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IDTechnicianRepository
    {
        Task<IEnumerable<TechnicianDTO>> GetAllAsync(int? WorkshopId, string? Name, string? Email, int PageNumber = 0);
        Task<IEnumerable<TechnicianDTO>> GetAllPINAsync(int? WorkshopId, string? Name, string? Email, int PageNumber = 1);
        Task<TechnicianDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(CreateDTechnicianDto dto);
        Task<int> UpdateAsync(UpdateDTechnicianDto dto);
        Task<int> DeleteAsync(DeleteDTechnicianDto dto);
        Task<IEnumerable<TechnicianDTO>> GetTechniciansDDL(int Id);
        Task<IEnumerable<TechnicianAvailabilityDTO>> GetAvailableTechniciansAsync(DateTime date, decimal duration, int branchId);
    }
}
