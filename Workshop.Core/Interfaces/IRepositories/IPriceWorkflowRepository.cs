using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IPriceWorkflowRepository
    {
        Task<IEnumerable<PriceWorkflowDTO>> GetAsync(PriceWorkflowDTO dto);
        Task<PriceWorkflowDTO> GetByIdAsync(int Id);
        Task<int> AddAsync(PriceWorkflowDTO dto);
        Task<int> UpdateAsync(PriceWorkflowDTO dto);
        Task<int> DeleteAsync(int Id);
    }
}
