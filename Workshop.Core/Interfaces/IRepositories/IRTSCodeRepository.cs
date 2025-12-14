using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IRTSCodeRepository
    {
        Task<IEnumerable<RTSCodeDTO>> GetAllAsync(string? Name, string? Code, int PageNumber = 0);
        Task<IEnumerable<RTSCodeDTO>> GetAllDDLAsync();
        Task<RTSCodeDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(CreateRTSCodeDTO dto);
        Task<int> UpdateAsync(UpdateRTSCodeDTO dto);
        Task<int> DeleteAsync(DeleteRTSCodeDTO dto);


    }
}
