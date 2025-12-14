using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface ILabourRateRepository
    {
        Task<IEnumerable<LabourRateDTO>> GetAllAsync(string Name, int? PageNumber = 0);
        Task<LabourRateDTO?> GetByIdAsync(int id);
        //Task<LabourRateDTO?> GetByGroupIdAsync(int id);
        Task<int> AddAsync(CreateLabourRateDTO dto);
        Task<int> UpdateAsync(UpdateLabourRateDTO dto);
        Task<int> DeleteAsync(DeleteLabourRateDTO dto);
    }
}
