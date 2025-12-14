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
    public class PriceMatrixService : IPriceMatrixService
    {
        private readonly IPriceMatrixRepository _repository;
        public PriceMatrixService(IPriceMatrixRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> AddAsync(CreatePriceMatrixDTO createPriceMatrixDTO)
        {
            return await _repository.AddAsync(createPriceMatrixDTO);
        }

        public async Task<int> DeleteAsync(int Id)
        {
            return await _repository.DeleteAsync(Id);
        }

        public async Task<List<GetPriceMatrixDTO>> GetAllAsync(PriceMatrixFilter createPriceMatrixDTO)
        {
            return await _repository.GetAllAsync(createPriceMatrixDTO);
        }

        public async Task<int> UpdateAsync(UpdatePriceMatrixDTO updatePriceMatrixDTO)
        {
            return await _repository.UpdateAsync(updatePriceMatrixDTO);
        }
        public async Task<GetPriceMatrixDTO> GetAsync(GetPriceMatrixDTO getPriceMatrixDTO)
        {
            return await _repository.GetAsync(getPriceMatrixDTO);
        }

        public async Task<PagedPriceMatrixResultDTO> GetAllPagedAsync(PriceMatrixFilter filter)
        {
            return await _repository.GetAllPagedAsync(filter);
        }
    }
}
