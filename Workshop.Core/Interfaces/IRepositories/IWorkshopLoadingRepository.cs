using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IWorkshopLoadingRepository
    {
        Task<IEnumerable<TechniciansNameDTO>> GetTechniciansName(int? Id);
        Task<IEnumerable<TechnicianScheduleDTO>> GetTechnicianSchedule(DateTime Date, DateTime? DateTo, int BranchId);
        Task<IEnumerable<TechnicianAvailabiltyDTO>> GetTechnicianAvailabilty();
        Task<IEnumerable<GroupedServicesDTO>> GetGroupedServices(int Id);
    }
}
