using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IPriceMatrixService
    {
        public Task<int> AddAsync(CreatePriceMatrixDTO createPriceMatrixDTO);
        public Task<List<GetPriceMatrixDTO>> GetAllAsync(PriceMatrixFilter createPriceMatrixDTO);
        public Task<int> UpdateAsync(UpdatePriceMatrixDTO updatePriceMatrixDTO);
        public Task<int> DeleteAsync(int Id);
        public Task<GetPriceMatrixDTO> GetAsync(GetPriceMatrixDTO getPriceMatrixDTO);

        // Paged
        public Task<PagedPriceMatrixResultDTO> GetAllPagedAsync(PriceMatrixFilter filter);
    }
}
