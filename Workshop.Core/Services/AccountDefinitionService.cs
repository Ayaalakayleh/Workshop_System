using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class AccountDefinitionService : IAccountDefinitionService
    {
        private readonly IAccountDefinitionRepository _repository;

        public AccountDefinitionService(IAccountDefinitionRepository repository)
        {
            _repository = repository;
        }

        public async Task<AccountDefinitionDTO?> GetAsync(int CompanyId)
        {
            return await _repository.GetAsync(CompanyId);
        }

        public async Task<int> AddAsync(AccountDefinitionDTO dto)
        {
            return await _repository.AddAsync(dto);
        }

        public async Task<int> UpdateAsync(AccountDefinitionDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }
    }
}
