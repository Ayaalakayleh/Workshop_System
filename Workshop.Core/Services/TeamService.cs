using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class TeamService: ITeamService
    {
        private readonly ITeamRepository _repository;
        public TeamService(ITeamRepository teamRepository) {
        
            _repository = teamRepository;
        }
        public Task<int> AddTeamAsync(AddTeamDTO team)
        {
            return _repository.AddTeamAsync(team);
        }

        public Task<int> DeleteTeamAsync(int id)
        {
            return _repository.DeleteTeamAsync(id);
        }

        public Task<IEnumerable<GetTeamDTO>> GetTeamAsync(int teamID)
        {
            return _repository.GetTeamAsync(teamID);
        }

        public Task<int> UpdateTeamAsync(UpdateTeamDTO team)
        {
            return _repository.UpdateTeamAsync(team);
        }
        public Task<IEnumerable<GetTeamDTO>> GetAllTeamsAsync(FilterTeamDTO oFilterTeamDTO)
        {
            return _repository.GetAllTeamsAsync(oFilterTeamDTO);
        }
        public Task<PagedTeamResultDTO> GetAllTeamsPagedAsync(FilterTeamDTO oFilterTeamDTO)
        {
            return _repository.GetAllTeamsPagedAsync(oFilterTeamDTO);
        }
        public Task<IEnumerable<GetTeamDTO>> GetAllTeamsDDLAsync()
        {
            return _repository.GetAllTeamsDDLAsync();
        }

    }
}
