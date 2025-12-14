using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _repository;
        public LookupService(ILookupRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<LookupDetailsDTO>> GetAllDetailsAsync()
        {
            return await _repository.GetAllDetailsAsync();
        }

        public async Task<IEnumerable<LookupDetailsDTO>> GetDetailsByHeaderIdAsync(int headerId, int CompanyId)
        {
            return await _repository.GetDetailsByHeaderIdAsync(headerId, CompanyId);
        }

        public async Task<LookupDetailsDTO?> GetDetailsByIdAsync(int id, int headerId, int CompanyId)
        {
            return await _repository.GetDetailsByIdAsync(id, headerId, CompanyId);
        }

        public async Task<int> AddDetailsAsync(CreateLookupDetailsDTO dto)
        {
            return await _repository.AddDetailsAsync(dto);
        }

        public async Task<int> UpdateDetailsAsync(UpdateLookupDetailsDTO dto)
        {
            return await _repository.UpdateDetailsAsync(dto);
        }

        public async Task<int> DeleteDetailsAsync(DeleteLookupDetailsDTO dto)
        {
            return await _repository.DeleteDetailsAsync(dto);
        }

        public async Task<IEnumerable<LookupHeaderDTO>> GetAllHeaderAsync(string language, int CompanyId)
        {
            try
            {
                var CategoryList = await _repository.GetAllHeadersAsync(CompanyId);

                foreach (var item in CategoryList)
                {
                    item.Name = language == "en" ? item.PrimaryName : item.SecondaryName;
                }
                return CategoryList;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching lookup headers.", ex);
                return Enumerable.Empty<LookupHeaderDTO>();
            }
        }

        public async Task<LookupHeaderDTO?> GetHeaderByIdAsync(int id, int CompanyId)
        {
            return await _repository.GetHeaderByIdAsync(id, CompanyId);
        }

    }

    
}
