using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface ITeamRepository
    {
        Task<int> DeleteTeamAsync(int id);
        Task<int> AddTeamAsync(AddTeamDTO team);
        Task<int> UpdateTeamAsync(UpdateTeamDTO team);
        Task<IEnumerable<GetTeamDTO>> GetTeamAsync(int teamID);
        Task<IEnumerable<GetTeamDTO>> GetAllTeamsAsync(FilterTeamDTO oFilterTeamDTO);
        Task<PagedTeamResultDTO> GetAllTeamsPagedAsync(FilterTeamDTO oFilterTeamDTO);
        Task<IEnumerable<GetTeamDTO>> GetAllTeamsDDLAsync();

    }
}
