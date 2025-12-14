using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class LabourRateService : ILabourRateService
    {
        private readonly ILabourRateRepository _repository;
        public LabourRateService(ILabourRateRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<LabourRateDTO>> GetAllAsync(string Name, string Language = "en", int? PageNumber = 0)
        {
            var data = await _repository.GetAllAsync(Name, PageNumber);
            foreach (var item in data)
            {
                item.Name = Language == "en" ? item.DescriptionEn : item.DescriptionAr;
            }
            return data;
        }

        public async Task<LabourRateDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        //public async Task<LabourRateDTO?> GetByGroupIdAsync(int id)
        //{
        //    return await _repository.GetByGroupIdAsync(id);
        //}

        public async Task<int> AddAsync(CreateLabourRateDTO dto)
        {
            return await _repository.AddAsync(dto);
        }

        public async Task<int> UpdateAsync(UpdateLabourRateDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<int> DeleteAsync(DeleteLabourRateDTO dto)
        {
            return await _repository.DeleteAsync(dto);
        }


    }
}
