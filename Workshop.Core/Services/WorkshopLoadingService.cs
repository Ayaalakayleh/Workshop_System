using Newtonsoft.Json;
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

    public class WorkshopLoadingService : IWorkshopLoadingService
    {
        private readonly IWorkshopLoadingRepository _LoadingRepository;
        public WorkshopLoadingService(IWorkshopLoadingRepository LoadingRepository)
        {
            this._LoadingRepository = LoadingRepository;
        }


        public async Task<IEnumerable<TechniciansNameDTO>> GetTechniciansName(int? Id)
        {
            return await _LoadingRepository.GetTechniciansName(Id);
        }

        public async Task<IEnumerable<TechnicianScheduleDTO>> GetTechnicianSchedule(DateTime date, DateTime? DateTo, int BranchId)
        {
            var data = await _LoadingRepository.GetTechnicianSchedule(date, DateTo, BranchId);

            foreach (var item in data)
            {
                try
                {
                    item.WorkingHoursList = JsonConvert.DeserializeObject<List<WorkingHourDTO>>(item.WorkingHours ?? "[]") ?? new();
                }
                catch
                {
                    item.WorkingHoursList = new();
                }
            }

            return data;
        }
        public async Task<IEnumerable<TechnicianAvailabiltyDTO>> Get_TechnicianAvailabilty()
        {
            return await _LoadingRepository.GetTechnicianAvailabilty();
        }

        public async Task<IEnumerable<GroupedServicesDTO>> GetGroupedServices(int Id)
        {
            var obj = await _LoadingRepository.GetGroupedServices(Id);
            return obj;
        }
    }
}
