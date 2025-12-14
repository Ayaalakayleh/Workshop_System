using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class TeamController : Controller
    {
        private readonly ITeamService _service; 

        public TeamController(ITeamService service)
        {
            _service = service;
        }

        [HttpPost("AddTeam")]
        public async Task<IActionResult> AddTeam([FromBody] AddTeamDTO teamToCreate)
        {
            var result = await _service.AddTeamAsync(teamToCreate);
            return Ok(result);
        }

        [HttpPost("UpdateTeam")]
        public async Task<IActionResult> UpdateTeam([FromBody] UpdateTeamDTO teamToUpdate)
        {
            var result = await _service.UpdateTeamAsync(teamToUpdate);
            return Ok(result);
        }

        [HttpPost("DeleteTeam")]
        public async Task<IActionResult> DeleteTeam([FromBody] int teamID)
        {
            var result = await _service.DeleteTeamAsync(teamID);
            return Ok(result);
        }

        [HttpPost("GetTeam")]
        public async Task<IActionResult> GetTeam([FromBody] int teamID)
        {
            var result = await _service.GetTeamAsync(teamID);
            return Ok(result);
        }

        [HttpPost("GetAllTeams")]
        public async Task<IActionResult> GetAllTeamsAsync([FromBody] FilterTeamDTO oFilterTeamDTO)
        {
            var result = await _service.GetAllTeamsAsync(oFilterTeamDTO);
            return Ok(result);
        }

        [HttpPost("GetAllTeamsPaged")]
        public async Task<IActionResult> GetAllTeamsPagedAsync([FromBody] FilterTeamDTO oFilterTeamDTO)
        {
            var result = await _service.GetAllTeamsPagedAsync(oFilterTeamDTO);
            return Ok(result);
        }

        [HttpGet("GetAllTeamsDDL")]
        public async Task<IActionResult> GetAllTeamsDDL()
        {
            var result = await _service.GetAllTeamsDDLAsync();
            return Ok(result);
        }

    }
}
