using Dapper;
using System.Data;
using System.Linq;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class TeamRepository: ITeamRepository
    {
        private readonly DapperContext _context;

        public TeamRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> AddTeamAsync(AddTeamDTO team)
        {
            using var connection = _context.CreateConnection();

            // Build DataTable for TVP
            var technicians = new DataTable();
            technicians.Columns.Add("TechnicianId", typeof(int));
            foreach (var techId in team.Technicians)
            {
                technicians.Rows.Add(techId);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Code", team.Code, DbType.String);
            parameters.Add("@PrimaryName", team.PrimaryName, DbType.String);
            parameters.Add("@SecondaryName", team.SecondaryName, DbType.String);
            parameters.Add("@Short", team.Short, DbType.String);
            parameters.Add("@Color", team.Color, DbType.Int32);
            parameters.Add("@CreatedBy", team.CreatedBy, DbType.Int32);

            // Add TVP parameter
            parameters.Add("@Technicians", technicians.AsTableValuedParameter("dbo.TechnicianIdList"));

                // Call stored procedure and return new TeamId
                var newTeamId = await connection.ExecuteScalarAsync<int>(
                    "Teams_Add",
                    parameters,
                    commandType: CommandType.StoredProcedure);
            if (newTeamId == -1)
            {
                return -1; // duplicate code
            }
            return newTeamId;
        }
        public async Task<int> DeleteTeamAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id,DbType.Int32);

            var result = await connection.QueryAsync<int>(
                "Teams_Delete",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result.FirstOrDefault();
        }
        public async Task<IEnumerable<GetTeamDTO>> GetAllTeamsDDLAsync()
        {
            using var connection = _context.CreateConnection();
            using var multi = await connection.QueryMultipleAsync("Teams_GetDDL", null, commandType: CommandType.StoredProcedure);
            return (await multi.ReadAsync<GetTeamDTO>()).ToList();
        }
        public async Task<IEnumerable<GetTeamDTO>> GetAllTeamsAsync(FilterTeamDTO oFilterTeamDTO)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", oFilterTeamDTO.PageNumber, DbType.Int32);
            parameters.Add("@Name", oFilterTeamDTO.Name, DbType.String);
            parameters.Add("@Code", oFilterTeamDTO.Code, DbType.String);

            using var multi = await connection.QueryMultipleAsync(
                "Teams_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);

            // First result set = Teams
            var teams = (await multi.ReadAsync<GetTeamDTO>()).ToList();

            // Second result set = TeamTechnicians
            //var technicians = (await multi.ReadAsync<(int TeamId, int TechnicianId)>());

            // Map technicians into teams
            //foreach (var team in teams)
            //{
            //    team.Technicians = technicians
            //        .Where(t => t.TeamId == team.Id)
            //        .Select(t => t.TechnicianId)
            //        .ToList();
            //}

            return teams;
        }

        public async Task<PagedTeamResultDTO> GetAllTeamsPagedAsync(FilterTeamDTO oFilterTeamDTO)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@PageNumber", oFilterTeamDTO.PageNumber ?? 1, DbType.Int32);
            parameters.Add("@PageSize", oFilterTeamDTO.PageSize ?? 25, DbType.Int32);
            parameters.Add("@Name", oFilterTeamDTO.Name ?? string.Empty, DbType.String);
            parameters.Add("@Code", oFilterTeamDTO.Code ?? string.Empty, DbType.String);

            using var multi = await connection.QueryMultipleAsync(
                "Teams_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);

            // First result set = Teams
            var teams = (await multi.ReadAsync<GetTeamDTO>()).ToList();

            // Try to get total count from second result set or from TotalPages property
            int totalRecords = 0;
            int totalPages = 0;
            int pageSize = oFilterTeamDTO.PageSize ?? 25;
            
            // Check if TotalPages is set in the first team (stored procedure might set it)
            if (teams.Any() && teams.First().TotalPages.HasValue)
            {
                totalPages = teams.First().TotalPages.Value;
                totalRecords = totalPages * pageSize;
            }
            else
            {
                // Try to read from second result set
                try
                {
                    if (!multi.IsConsumed)
                    {
                        var totalCountResult = await multi.ReadAsync<int>();
                        totalRecords = totalCountResult.FirstOrDefault();
                        if (totalRecords > 0)
                        {
                            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                        }
                    }
                }
                catch
                {
                    // If reading fails, we'll calculate based on returned rows
                }
                
                // If we still don't have totalRecords, estimate from current page
                // This is a fallback - ideally the stored procedure should return total count
                if (totalRecords == 0)
                {
                    // If we got a full page, there might be more pages
                    if (teams.Count == pageSize)
                    {
                        // Estimate: assume at least current page + 1 more
                        totalRecords = (oFilterTeamDTO.PageNumber ?? 1) * pageSize + 1;
                        totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                    }
                    else
                    {
                        // Last page or only page
                        totalRecords = ((oFilterTeamDTO.PageNumber ?? 1) - 1) * pageSize + teams.Count;
                        totalPages = oFilterTeamDTO.PageNumber ?? 1;
                    }
                }
            }

            return new PagedTeamResultDTO
            {
                Teams = teams,
                TotalRecords = totalRecords,
                TotalPages = totalPages > 0 ? totalPages : 1,
                CurrentPage = oFilterTeamDTO.PageNumber ?? 1,
                PageSize = pageSize
            };
        }
        public async Task<IEnumerable<GetTeamDTO>> GetTeamAsync(int teamID)
        {
            using var connection = _context.CreateConnection();
            var parameters = new DynamicParameters();
            parameters.Add("@ID", teamID, DbType.Int32);

            var multi = await connection.QueryMultipleAsync(
                "Teams_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);


            // First result set = Teams
            var teams = (await multi.ReadAsync<GetTeamDTO>()).ToList();

            // Second result set = TeamTechnicians
            var technicians = (await multi.ReadAsync<(int TeamId, int TechnicianId)>());

            // Map technicians into teams
            foreach (var team in teams)
            {
                team.Technicians = technicians
                    .Where(t => t.TeamId == team.Id)
                    .Select(t => t.TechnicianId)
                    .ToList();
            }

            return teams;
        }
        public async Task<int> UpdateTeamAsync(UpdateTeamDTO team)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Code", team.Code,DbType.String);
            parameters.Add("@PrimaryName", team.PrimaryName, DbType.String);
            parameters.Add("@SecondaryName", team.SecondaryName, DbType.String);
            parameters.Add("@Short", team.Short, DbType.String);
            parameters.Add("@ModifiedBy", team.ModifiedBy, DbType.Int32);
            parameters.Add("@Color", team.Color, DbType.Int32);
            parameters.Add("@ID", team.Id, DbType.Int32);

            var technicians = new DataTable();
            technicians.Columns.Add("TechnicianId", typeof(int));
            foreach (var techId in team.Technicians)
            {
                technicians.Rows.Add(techId);
            }
            parameters.Add("@Technicians", technicians.AsTableValuedParameter("dbo.TechnicianIdList"));

            var result = await connection.ExecuteAsync(
             "Teams_Update",
             parameters,
            commandType: CommandType.StoredProcedure);

            return result;
        }

    }
}
