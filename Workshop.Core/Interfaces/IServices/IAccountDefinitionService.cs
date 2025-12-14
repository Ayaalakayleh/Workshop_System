using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IAccountDefinitionService
    {
        Task<AccountDefinitionDTO?> GetAsync(int CompanyId);
        Task<int> AddAsync(AccountDefinitionDTO dto);
        Task<int> UpdateAsync(AccountDefinitionDTO dto);
    }
}
