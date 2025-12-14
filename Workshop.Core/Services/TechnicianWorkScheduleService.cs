using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class TechnicianWorkScheduleService : ITechnicianWorkScheduleService
    {
        private readonly ITechnicianWorkScheduleRepository _repository;
        public TechnicianWorkScheduleService(ITechnicianWorkScheduleRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TechnicianWorkScheduleDTO>> GetAllAsync(int workshopId, string Name, DateTime? Date, string language, int PageNumber = 0)
        {

            var data = await _repository.GetAllAsync(workshopId, Name, Date, PageNumber);
            foreach (var item in data)
            {
                item.Name = language == "en" ? item.PrimaryName : item.SecondaryName;
            }
            return data;
        }

        public async Task<TechnicianWorkScheduleDTO?> GetByIdAsync(int id, string language)
        {
            var item = await _repository.GetByIdAsync(id);
            item.Name = language == "en" ? item.PrimaryName : item.SecondaryName;
            return item;
        }

        public async Task<int> AddAsync(CreateTechnicianWorkScheduleDTO dto)
        {
            return await _repository.AddAsync(dto);
        }

        public async Task<int> UpdateAsync(UpdateTechnicianWorkScheduleDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<int> DeleteAsync(DeleteTechnicianWorkScheduleDTO dto)
        {
            return await _repository.DeleteAsync(dto);
        }

        public async Task<IEnumerable<int>> GetTechniciansFromWorkSchedulesAsync()
        {
            return await _repository.GetTechniciansFromWorkSchedulesAsync();
        }
    }
}
