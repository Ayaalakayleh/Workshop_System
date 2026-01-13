using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class PriceWorkflowService : IPriceWorkflowService
    {
        private readonly IPriceWorkflowRepository _repository;

        public PriceWorkflowService(IPriceWorkflowRepository repository)
        {
            _repository = repository;
        }


        public async Task<IEnumerable<PriceWorkflowDTO>> GetAsync(PriceWorkflowDTO dto)
        {
            return await _repository.GetAsync(dto);

        }

        public async Task<PriceWorkflowDTO> GetByIdAsync(int Id)
        {
            return await _repository.GetByIdAsync(Id);

        }

        public async Task<int> AddAsync(PriceWorkflowDTO dto)
        {
            return await _repository.AddAsync(dto);

        }

        public async Task<int> UpdateAsync(PriceWorkflowDTO dto)
        {
            return await _repository.UpdateAsync(dto);

        }

        public async Task<int> DeleteAsync(int Id)
        {
            return await _repository.DeleteAsync(Id);

        }
    }
}
