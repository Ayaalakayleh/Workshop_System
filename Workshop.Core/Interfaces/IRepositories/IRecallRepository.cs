using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IRecallRepository
    {
        Task<IEnumerable<RecallDTO>> GetAllAsync(FilterRecallDTO filterRecallDTO);
        Task<IEnumerable<RecallDTO>> GetAllDDLAsync();
        Task<RecallDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(CreateRecallDTO dto);
        Task<int> UpdateAsync(UpdateRecallDTO dto);
        Task<int> DeleteAsync(DeleteRecallDTO dto);
        Task<ActiveRecallsByChassisResponseDto> GetActiveRecallsByChassisAsync(string chassisNo);
    }
}
