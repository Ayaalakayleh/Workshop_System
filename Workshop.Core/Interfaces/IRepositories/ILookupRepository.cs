using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface ILookupRepository
    {
        Task<IEnumerable<LookupDetailsDTO>> GetAllDetailsAsync();
        Task<IEnumerable<LookupDetailsDTO>> GetDetailsByHeaderIdAsync(int headerId, int CompanyId);
        Task<LookupDetailsDTO?> GetDetailsByIdAsync(int id, int headerId, int CompanyId);
        Task<int> AddDetailsAsync(CreateLookupDetailsDTO dto);
        Task<int> UpdateDetailsAsync(UpdateLookupDetailsDTO dto);
        Task<int> DeleteDetailsAsync(DeleteLookupDetailsDTO dto);
        Task<IEnumerable<LookupHeaderDTO>> GetAllHeadersAsync(int CompanyId);
        Task<LookupHeaderDTO?> GetHeaderByIdAsync(int id, int CompanyId);

    }
}
