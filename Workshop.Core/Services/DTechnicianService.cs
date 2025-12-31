using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class DTechnicianService : IDTechnicianService
    {
        private readonly IDTechnicianRepository _repository;
        public DTechnicianService(IDTechnicianRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<TechnicianDTO>> GetAllAsync(int workshopId, string Name, string Email, int PageNumber = 0)
        {
            return await _repository.GetAllAsync(workshopId, Name, Email, PageNumber);
        }
        public async Task<IEnumerable<TechnicianDTO>> GetAllPINAsync(int? WorkshopId, string? Name, string? Email, int PageNumber = 1)
        {
            return await _repository.GetAllPINAsync(WorkshopId, Name, Email, PageNumber);
        }
        public async Task<IEnumerable<TechnicianDTO>> GetTechniciansDDL(int Id)
        {
            return await _repository.GetTechniciansDDL(Id);
        }

        public async Task<TechnicianDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> AddAsync(CreateDTechnicianDto dto)
        {
            return await _repository.AddAsync(dto);
        }

        public async Task<int> UpdateAsync(UpdateDTechnicianDto dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<int> DeleteAsync(DeleteDTechnicianDto dto)
        {
            return await _repository.DeleteAsync(dto);
        }
        public async Task<IEnumerable<TechnicianAvailabilityDTO>> GetAvailableTechniciansAsync(DateTime date, decimal duration, int branchId, bool trimPastIntervals = false)
        {
            var technicians = await _repository.GetAvailableTechniciansAsync(date, duration, branchId);

            if (!trimPastIntervals || date.Date != DateTime.Now.Date)
            {
                return technicians;
            }

            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            var result = technicians.Select(tech =>
            {
                var filteredIntervals = new List<FreeIntervalDTO>();

                foreach (var interval in tech.FreeIntervalsList)
                {
                    TimeSpan startTime = TimeSpan.Parse(interval.StartFree);
                    TimeSpan endTime = TimeSpan.Parse(interval.EndFree);

                    if (endTime <= currentTime)
                    {
                        continue;
                    }

                    if (startTime < currentTime && endTime > currentTime)
                    {
                        filteredIntervals.Add(new FreeIntervalDTO
                        {
                            StartFree = currentTime.ToString(@"hh\:mm\:ss"),
                            EndFree = interval.EndFree
                        });
                    }
                    else if (startTime >= currentTime)
                    {
                        filteredIntervals.Add(interval);
                    }
                }

                tech.FreeIntervalsList = filteredIntervals;
                tech.FreeIntervals = filteredIntervals.Count > 0
                    ? System.Text.Json.JsonSerializer.Serialize(filteredIntervals.Select(i => new { StartFree = i.StartFree, EndFree = i.EndFree }))
                    : "[]";

                return tech;
            })
            .Where(tech => tech.FreeIntervalsList.Any()) 
            .ToList();

            return result;
        }
    }
}
