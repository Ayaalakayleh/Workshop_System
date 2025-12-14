using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IClockingRepository
    {
        public Task<List<ClockingDTO>> GetClocks();

        public Task <int> InsertClock(ClockingDTO dTO);

        public Task<int> UpdateClock(ClockingDTO dTO);

        public Task<int> DeleteClock(int id);
        public Task<int> InsertClockBreak(ClockingBreakDTO dTO);
        public Task<ClockingDTO> GetClock(int Id);
        public Task<int> UpdateClockBreak(ClockingBreakDTO dTO);
        public Task<ClockingBreakDTO> GetLastBreakByClockID(int Id);
        public Task<List<ClockingBreakDTO>> GetAllClocksBreaksDDL();
        public Task<List<ClockingDTO>> GetClocksPaged(ClockingFilterDTO filterDTO);
        public Task<List<ClockingBreakDTO>> GetClocksBreaksPaged(ClockingFilterDTO filterDTO);
        public Task<List<ClockingBreakDTO>> GetBreaksByClockID(int ClockID);
        public Task<List<ClockingDTO>> GetClocksHistory();
        public Task<List<GetClockingFilter>> GetClockingFilter();

    }
}
