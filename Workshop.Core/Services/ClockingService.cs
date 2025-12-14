using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class ClockingService : IClockingService
    {
        private readonly IClockingRepository _repository;
        public ClockingService(IClockingRepository repository)
        {
            _repository = repository;
        }
        public async Task<List<ClockingDTO>> GetClocks()
        {
            return await _repository.GetClocks();
        }

        public async Task<int> InsertClock(ClockingDTO dTO)
        {
           return await _repository.InsertClock(dTO);
        }

        public async Task<int> UpdateClock(ClockingDTO dTO)
        {
            return await _repository.UpdateClock(dTO);
        }

        public async Task<int> DeleteClock(int id)
        {
            return await _repository.DeleteClock(id);
        }
        public async Task<int> InsertClockBreak(ClockingBreakDTO dTO)
        {
            return await _repository.InsertClockBreak(dTO);
        }
        public async Task<ClockingDTO> GetClock(int Id)
        {
            return await _repository.GetClock(Id);
        }

        public async Task<int> UpdateClockBreak(ClockingBreakDTO dTO)
        {
            return await _repository.UpdateClockBreak(dTO);
        }
        public async Task<ClockingBreakDTO> GetLastBreakByClockID(int Id)
        {
            return await _repository.GetLastBreakByClockID(Id);
        }
        public async Task<List<ClockingBreakDTO>> GetAllClocksBreaksDDL()
        {
            return await _repository.GetAllClocksBreaksDDL();
        }
        public async Task<List<ClockingDTO>> GetClocksPaged(ClockingFilterDTO filterDTO)
        {
            return await _repository.GetClocksPaged(filterDTO);
        }
        public async Task<List<ClockingBreakDTO>> GetClocksBreaksPaged(ClockingFilterDTO filterDTO)
        {
            return await _repository.GetClocksBreaksPaged(filterDTO);
        }
        public async Task<List<ClockingBreakDTO>> GetBreaksByClockID(int ClockID)
        {
            return await _repository.GetBreaksByClockID(ClockID);
        }
        public async Task<List<ClockingDTO>> GetClocksHistory()
        {
            return await _repository.GetClocksHistory();
        }

        public async Task<List<GetClockingFilter>> GetClockingFilter() {

            return await _repository.GetClockingFilter();
        }

    }
}
