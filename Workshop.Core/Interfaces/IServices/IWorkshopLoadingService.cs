using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.WorkshopDTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IWorkshopLoadingService
    {
        Task<IEnumerable<TechniciansNameDTO>> GetTechniciansName(int? Id);
        Task<IEnumerable<TechnicianScheduleDTO>> GetTechnicianSchedule(DateTime Date, DateTime? DateTO, int BranchId);
        Task<IEnumerable<TechnicianAvailabiltyDTO>> Get_TechnicianAvailabilty();
        Task<IEnumerable<GroupedServicesDTO>> GetGroupedServices(int Id);
    }
}
