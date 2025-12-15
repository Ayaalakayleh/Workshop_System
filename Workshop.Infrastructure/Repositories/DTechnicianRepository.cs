using Dapper;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class DTechnicianRepository : IDTechnicianRepository
    {
        private readonly DapperContext _context;
        private readonly Database _database;
        public DTechnicianRepository(DapperContext context, Database database)
        {
            _context = context;
            _database = database;
        }

        public async Task<IEnumerable<TechnicianDTO>> GetAllAsync(
    int? WorkshopId, string? Name, string? Email, int PageNumber = 1)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@WorkshopId", WorkshopId, DbType.Int32);
            parameters.Add("@Name", Name, DbType.String);
            parameters.Add("@Email", Email, DbType.String);
            parameters.Add("@PageNumber", PageNumber, DbType.Int32);

            using var multi = await connection.QueryMultipleAsync(
                "D_Technicians_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);

            // First result: Technicians
            var technicians = (await multi.ReadAsync<TechnicianDTO>()).ToList();

            // Second result: Teams
            var teams = await multi.ReadAsync<(int TechnicianId, int TeamId)>();

            // Third result: Skills
            var skills = await multi.ReadAsync<(int TechnicianId, int SkillId)>();

            // Map teams to technicians
            foreach (var tech in technicians)
            {
                tech.Teams = teams
                    .Where(t => t.TechnicianId == tech.Id)
                    .Select(t => t.TeamId)
                    .ToList();

                tech.FK_SkillId = skills
                    .Where(s => s.TechnicianId == tech.Id)
                    .Select(s => s.SkillId)
                    .ToList();
            }

            return technicians;

        }

        public async Task<IEnumerable<TechnicianDTO>> GetAllPINAsync(int? WorkshopId, string? Name, string? Email, int PageNumber = 1)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@WorkshopId", WorkshopId, DbType.Int32);
            parameters.Add("@Name", Name, DbType.String);
            parameters.Add("@Email", Email, DbType.String);
            parameters.Add("@PageNumber", PageNumber, DbType.Int32);

            using var multi = await connection.QueryMultipleAsync(
                "D_Technicians_GetAllPIN",
                parameters,
                commandType: CommandType.StoredProcedure);

            var technicians = (await multi.ReadAsync<TechnicianDTO>()).ToList();

            return technicians;

        }

        public async Task<TechnicianDTO?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            using var multi = await connection.QueryMultipleAsync(
                "D_Technicians_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            // First result set: Technician
            var technician = await multi.ReadSingleOrDefaultAsync<TechnicianDTO>();
            if (technician == null)
                return null;

            // Second result set: Teams
            var teams = await multi.ReadAsync<int>();
            technician.Teams = teams.ToList();

            // Third result set: Skills
            var skills = await multi.ReadAsync<int>();
            technician.FK_SkillId = skills.ToList();

            return technician;
        }


        public async Task<int> AddAsync(CreateDTechnicianDto dto)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@WorkshopId", dto.WorkshopId);
            parameters.Add("@PrimaryName", dto.PrimaryName);
            parameters.Add("@SecondaryName", dto.SecondaryName);
            parameters.Add("@Email", dto.Email);
            //parameters.Add("@Phone", dto.Phone);
            //parameters.Add("@BirthDate", dto.BirthDate);
            //parameters.Add("@PrimaryAddress", dto.PrimaryAddress);
            //parameters.Add("@SecondaryAddress", dto.SecondaryAddress);
            parameters.Add("@CreatedBy", dto.CreatedBy);
            parameters.Add("@FilePath", dto.FilePath);
            parameters.Add("@FileName", dto.FileName);
            //added
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@Code", dto.Code);
            //parameters.Add("@HourCost", dto.HourCost);
            parameters.Add("@FordPID", dto.FordPID);
            parameters.Add("@PIN", dto.PIN);
            parameters.Add("@IsResigned", dto.IsResigned);
            parameters.Add("@ResignedDate", dto.ResignedDate);
            parameters.Add("@FK_ShiftId", dto.FK_ShiftId);
            parameters.Add("@Type", dto.Type);

            var SkillId_Table = new DataTable();
            SkillId_Table.Columns.Add("FK_SkillId", typeof(int));

            if (dto.FK_SkillId != null)
            {
                foreach (var Skill_Id in dto.FK_SkillId)
                {
                    SkillId_Table.Rows.Add(Skill_Id);
                }
            }
            var teamIdsTable = new DataTable();
            teamIdsTable.Columns.Add("TeamId", typeof(int));

            if (dto.Teams != null)
            {
                foreach (var teamId in dto.Teams)
                {
                    teamIdsTable.Rows.Add(teamId);
                }
            }

            parameters.Add("@TeamIds", teamIdsTable.AsTableValuedParameter("dbo.TeamIdList"));
            parameters.Add("@FK_SkillId", SkillId_Table.AsTableValuedParameter("dbo.SkillsIdList"));

            int result = await connection.QuerySingleAsync<int>(
                "D_Technicians_Insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return result;


        }

        public async Task<int> UpdateAsync(UpdateDTechnicianDto dto)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", dto.Id);
            parameters.Add("@WorkshopId", dto.WorkshopId);
            parameters.Add("@PrimaryName", dto.PrimaryName);
            parameters.Add("@SecondaryName", dto.SecondaryName);
            parameters.Add("@Email", dto.Email);
            //parameters.Add("@Phone", dto.Phone);
            //parameters.Add("@BirthDate", dto.BirthDate);
            //parameters.Add("@PrimaryAddress", dto.PrimaryAddress);
            //parameters.Add("@SecondaryAddress", dto.SecondaryAddress);
            parameters.Add("@FilePath", dto.FilePath);
            parameters.Add("@FileName", dto.FileName);
            //added
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@Code", dto.Code);
            //parameters.Add("@HourCost", dto.HourCost);
            parameters.Add("@FordPID", dto.FordPID);
            parameters.Add("@PIN", dto.PIN);
            parameters.Add("@IsResigned", dto.IsResigned);
            parameters.Add("@ResignedDate", dto.ResignedDate);
            parameters.Add("@FK_ShiftId", dto.FK_ShiftId);
            parameters.Add("@Type", dto.Type);


            var SkillId_Table = new DataTable();
            SkillId_Table.Columns.Add("FK_SkillId", typeof(int));

            if (dto.FK_SkillId != null)
            {
                foreach (var Skill_Id in dto.FK_SkillId)
                {
                    SkillId_Table.Rows.Add(Skill_Id);
                }
            }
            var teamIdsTable = new DataTable();
            teamIdsTable.Columns.Add("TeamId", typeof(int));

            if (dto.Teams != null)
            {
                foreach (var teamId in dto.Teams)
                {
                    teamIdsTable.Rows.Add(teamId);
                }
            }

            parameters.Add("@TeamIds", teamIdsTable.AsTableValuedParameter("dbo.TeamIdList"));
            parameters.Add("@FK_SkillId", SkillId_Table.AsTableValuedParameter("dbo.SkillsIdList"));


            try
            {
                var result = await connection.QuerySingleAsync<int>(
                    "D_Technicians_Update",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return 0;
        }

        public async Task<int> DeleteAsync(DeleteDTechnicianDto dto)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("ID", dto.Id);
            parameters.Add("@UpdatedBy", dto.UpdatedBy);

            var result = await connection.QueryFirstAsync<dynamic>(
                "D_Technicians_Delete",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result.RowsAffected;
        }

        public async Task<IEnumerable<TechnicianDTO>> GetTechniciansDDL(int BranchIDId)
        {
            var parameters = new
            {
                BranchId = BranchIDId
            };
            return await _database.ExecuteGetAllStoredProcedure<TechnicianDTO>("GetTechniciansDDL", parameters);
        }
        public async Task<IEnumerable<TechnicianAvailabilityDTO>> GetAvailableTechniciansAsync(DateTime date, decimal duration, int branchId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Date", date, DbType.Date);
            parameters.Add("@Duration", duration, DbType.Decimal);
            parameters.Add("@BranchId", branchId, DbType.Int32);
            var result = await connection.QueryAsync<TechnicianAvailabilityDTO>(
                "D_Technicians_GetAvailable",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            foreach (var tech in result)
            {
                if (!string.IsNullOrWhiteSpace(tech.FreeIntervals))
                {
                    try
                    {
                        tech.FreeIntervalsList =
                            JsonConvert.DeserializeObject<List<FreeIntervalDTO>>(tech.FreeIntervals)
                            ?? new List<FreeIntervalDTO>();
                    }
                    catch
                    {
                        // Defensive fallback in case JSON is malformed
                        tech.FreeIntervalsList = new List<FreeIntervalDTO>();
                    }
                }
                else
                {
                    tech.FreeIntervalsList = new List<FreeIntervalDTO>();
                }
            }

            return result;
        }

    }
}
