using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{

    public class WorkshopLoadingRepository : IWorkshopLoadingRepository
    {
        private readonly Database _database;

        public WorkshopLoadingRepository(Database Database)
        {
            this._database = Database;
        }
        public async Task<IEnumerable<TechniciansNameDTO>> GetTechniciansName(int? Id)
        {
            var parameter = new
            {
                Id = Id
            };
            var obj = await _database.ExecuteGetAllStoredProcedure<TechniciansNameDTO>("GetTechniciansSchedule", parameter);
            return obj.ToList();
        }

        public async Task<IEnumerable<TechnicianScheduleDTO>> GetTechnicianSchedule(DateTime Date, DateTime? DateTo, int BranchId)
        {
            try
            {
                var parameter = new
                {
                    RequestDate = Date,
                    RequestEndDate = DateTo,
                    BranchId = BranchId
                };
                var obj = await _database.ExecuteGetAllStoredProcedure<TechnicianScheduleDTO>("GetTechniciansSchedule", parameter);
                return obj.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<IEnumerable<TechnicianAvailabiltyDTO>> GetTechnicianAvailabilty()
        {
            try
            {
                var obj = await _database.ExecuteGetAllStoredProcedure<TechnicianAvailabiltyDTO>("Get_TechnicianAvailabilty", null);
                return obj.ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<IEnumerable<GroupedServicesDTO>> GetGroupedServices(int Id)
        {
            try
            {
                var parameter = new
                {
                    Id = Id
                };
                var obj = _database.ExecuteGetAllStoredProcedure<GroupedServicesDTO>("WIP_GetGroupedServices", Id);
                return obj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
