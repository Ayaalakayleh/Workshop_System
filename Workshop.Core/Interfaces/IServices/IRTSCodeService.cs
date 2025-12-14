using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IRTSCodeService
    {
        Task<IEnumerable<RTSCodeDTO>> GetAllAsync(string Name, string Code, int PageNumber = 0);
        Task<IEnumerable<RTSCodeDTO>> GetAllDDLAsync();
        Task<RTSCodeDTO?> GetByIdAsync(int id);
        Task<int> AddAsync(CreateRTSCodeDTO dto);
        Task<int> UpdateAsync(UpdateRTSCodeDTO dto);
        Task<int> DeleteAsync(DeleteRTSCodeDTO dto);
    }
}
