using Dapper;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class RTSCodeRepository : IRTSCodeRepository
    {
        private readonly DapperContext _context;
        public RTSCodeRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RTSCodeDTO>> GetAllAsync(string? Name, string? Code, int PageNumber = 0)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Name", Name, DbType.String);
            parameters.Add("@Code", Code, DbType.String);
            parameters.Add("@PageNumber", PageNumber, DbType.Int32);
            parameters.Add("@PageSize", 25, DbType.Int32); // قيمة افتراضية

            var result = await connection.QueryAsync<RTSCodeDTO>(
                "RTSCode_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);

            return result;
        }

        public async Task<IEnumerable<RTSCodeDTO>> GetAllDDLAsync()
        {
            using var connection = _context.CreateConnection();
            using var multi = await connection.QueryMultipleAsync("RTSCode_GetAllDDL", null, commandType: CommandType.StoredProcedure);
            return (await multi.ReadAsync<RTSCodeDTO>()).ToList();
            //return await _database.ExecuteGetAllStoredProcedure<RTSCodeDTO>("RTSCode_GetAllDDL", new object());
        }

        public async Task<RTSCodeDTO?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            using var multi = await connection.QueryMultipleAsync(
                "RTSCode_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            // First result: RTS Code details
            var rtsCode = await multi.ReadSingleOrDefaultAsync<RTSCodeDTO>();
            if (rtsCode == null) return null;

            // Second result: Franchise IDs
            var franchises = await multi.ReadAsync<int>();
            rtsCode.FranchiseIds = franchises.ToList();

            // Third result: Vehicle Class IDs
            var vehicleClasses = await multi.ReadAsync<int>();
            rtsCode.VehicleClassIds = vehicleClasses.ToList();

            return rtsCode;
        }

        public async Task<int> AddAsync(CreateRTSCodeDTO dto)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Code", dto.Code);
            parameters.Add("@PrimaryDescription", dto.PrimaryDescription);
            parameters.Add("@SecondaryDescription", dto.SecondaryDescription);
            parameters.Add("@FK_CategoryId", dto.FK_CategoryId);
            parameters.Add("@FK_SkillId", dto.FK_SkillId);
            parameters.Add("@CompanyId", dto.CompanyId);
            parameters.Add("@StandardHours", dto.StandardHours);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@EffectiveDate", dto.EffectiveDate);
            parameters.Add("@Notes", dto.Notes);
            parameters.Add("@CreatedBy", dto.CreatedBy);
            parameters.Add("@DefaultRate", dto.DefaultRate);
            parameters.Add("@PrimaryName", dto.PrimaryName);
            parameters.Add("@SecondaryName", dto.SecondaryName);

            // Table-Valued Parameters
            var franchiseIdsTable = new DataTable();
            franchiseIdsTable.Columns.Add("FranchiseId", typeof(int));

            if (dto.FranchiseIds != null && dto.FranchiseIds.Any())
            {
                foreach (var franchiseId in dto.FranchiseIds)
                {
                    franchiseIdsTable.Rows.Add(franchiseId);
                }
            }

            var vehicleClassIdsTable = new DataTable();
            vehicleClassIdsTable.Columns.Add("VehicleClassId", typeof(int));

            if (dto.VehicleClassIds != null && dto.VehicleClassIds.Any())
            {
                foreach (var vehicleClassId in dto.VehicleClassIds)
                {
                    vehicleClassIdsTable.Rows.Add(vehicleClassId);
                }
            }

            parameters.Add("@FranchiseIds", franchiseIdsTable.AsTableValuedParameter("dbo.RTSFranchiseIdList"));
            parameters.Add("@VehicleClassIds", vehicleClassIdsTable.AsTableValuedParameter("dbo.RTSVehicleClassIdList"));

            var result = await connection.QuerySingleAsync<int>(
                "RTSCode_Insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<int> UpdateAsync(UpdateRTSCodeDTO dto)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", dto.Id);
            parameters.Add("@Code", dto.Code);
            parameters.Add("@PrimaryDescription", dto.PrimaryDescription);
            parameters.Add("@SecondaryDescription", dto.SecondaryDescription);
            parameters.Add("@FK_CategoryId", dto.FK_CategoryId);
            parameters.Add("@FK_SkillId", dto.FK_SkillId);
            parameters.Add("@CompanyId", dto.CompanyId);
            parameters.Add("@StandardHours", dto.StandardHours);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@EffectiveDate", dto.EffectiveDate);
            parameters.Add("@Notes", dto.Notes);
            parameters.Add("@UpdatedBy", dto.UpdatedBy);
            parameters.Add("@DefaultRate", dto.DefaultRate);
            parameters.Add("@PrimaryName", dto.PrimaryName);
            parameters.Add("@SecondaryName", dto.SecondaryName);

            // Table-Valued Parameters
            var franchiseIdsTable = new DataTable();
            franchiseIdsTable.Columns.Add("FranchiseId", typeof(int));

            if (dto.FranchiseIds != null && dto.FranchiseIds.Any())
            {
                foreach (var franchiseId in dto.FranchiseIds)
                {
                    franchiseIdsTable.Rows.Add(franchiseId);
                }
            }

            var vehicleClassIdsTable = new DataTable();
            vehicleClassIdsTable.Columns.Add("VehicleClassId", typeof(int));

            if (dto.VehicleClassIds != null && dto.VehicleClassIds.Any())
            {
                foreach (var vehicleClassId in dto.VehicleClassIds)
                {
                    vehicleClassIdsTable.Rows.Add(vehicleClassId);
                }
            }

            parameters.Add("@FranchiseIds", franchiseIdsTable.AsTableValuedParameter("dbo.RTSFranchiseIdList"));
            parameters.Add("@VehicleClassIds", vehicleClassIdsTable.AsTableValuedParameter("dbo.RTSVehicleClassIdList"));

            var result = await connection.QuerySingleAsync<int>(
                "RTSCode_Update",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<int> DeleteAsync(DeleteRTSCodeDTO dto)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", dto.Id);
            parameters.Add("@UpdatedBy", dto.UpdatedBy);
            parameters.Add("@IsActive", dto.IsActive);

                var result = await connection.QuerySingleOrDefaultAsync<int>(
                    "RTSCode_Delete",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                // result could be 0 (not allowed), or 1 (deleted)
                return result;

        }

    }
}
