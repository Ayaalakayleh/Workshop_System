

using Workshop.Core.DTOs;
using Workshop.Infrastructure.Repositories;

namespace Workshop.Core.Interfaces.IRepositories
{
    public class JobCardRepository: IJobCardRepository
    {
        private readonly Database _database;
        public JobCardRepository(Database database) { 
            _database = database;
        }

        public async Task<JobCardDTO> GetJobCardByMasterIdAsync(Guid id)
        {
            try{
            var parameters = new { MasterId = id };
                return await _database.ExecuteGetByIdProcedure<JobCardDTO?>("JobCard.D_JobCard_GetByMasterId", parameters);
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
