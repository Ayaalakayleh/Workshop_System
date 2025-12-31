using Dapper;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class RecallREpository : IRecallRepository
    {
        private readonly DapperContext _context;
        public RecallREpository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecallDTO>> GetAllAsync(FilterRecallDTO filterRecallDTO)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Title", filterRecallDTO.Tittle, DbType.String);
            parameters.Add("@Code", filterRecallDTO.Code, DbType.String);
            parameters.Add("@PageNumber", filterRecallDTO.PageNumber, DbType.Int32);
            parameters.Add("@PageSize", 25 , DbType.Int32);

            var result = await connection.QueryAsync<RecallDTO>(
                "Recall_GetAll",
                parameters,
                commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<IEnumerable<RecallDTO>> GetAllDDLAsync()
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                "Recall_GetAllDDL",
                commandType: CommandType.StoredProcedure
            );

            var recalls = (await multi.ReadAsync<RecallDTO>()).ToList();
            var vehicles = (await multi.ReadAsync<VehicleRecallDTO>()).ToList();

            var vehicleLookup = vehicles
                .Where(v => v.RecallID.HasValue)
                .GroupBy(v => v.RecallID!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var recall in recalls)
            {

                recall.Vehicles ??= new List<VehicleRecallDTO>();

                if (vehicleLookup.TryGetValue(recall.Id, out var recallVehicles))
                {
                    recall.Vehicles.AddRange(recallVehicles);
                }
            }

            return recalls;
        }



        public async Task<RecallDTO?> GetByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", id, DbType.Int32);

            using var multi = await connection.QueryMultipleAsync(
                "Recall_GetById",
                parameters,
                commandType: CommandType.StoredProcedure);

            // First result: Recall details
            var recall = await multi.ReadSingleOrDefaultAsync<RecallDTO>();

            if (recall == null)
                return null;

            // Second result: Vehicle list
            var vehicles = (await multi.ReadAsync<VehicleRecallDTO>()).ToList();

            recall.Vehicles = vehicles;

            return recall;
        }
        public async Task<ActiveRecallsByChassisResponseDto> GetActiveRecallsByChassisAsync(string chassisNo)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@ChassisNo", chassisNo, DbType.String);

            var activeRecalls = await connection.QueryAsync<ActiveRecallDto>(
                "GetActiveRecallsByChassis",
                parameters,
                commandType: CommandType.StoredProcedure);

            return new ActiveRecallsByChassisResponseDto
            {
                ChassisNo = chassisNo,
                Recalls = activeRecalls.ToList()
            };
        }
        public async Task<int> AddAsync(CreateRecallDTO dto)
        {
            using var connection = _context.CreateConnection();

            var vehicleTable = new DataTable();
            vehicleTable.Columns.Add("MakeId", typeof(int));
            vehicleTable.Columns.Add("ModelId", typeof(int));
            vehicleTable.Columns.Add("Chassis", typeof(string));
            vehicleTable.Columns.Add("RecallStatus", typeof(int));

            if (dto.Vehicles != null && dto.Vehicles.Count > 0)
            {
                foreach (var v in dto.Vehicles)
                {
                    vehicleTable.Rows.Add(
                        v.MakeID,
                        v.ModelID,
                        v.Chassis,
                        v.RecallStatus
                    );
                }
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Code", dto.Code);
            parameters.Add("@Title", dto.Title);
            parameters.Add("@Description", dto.Description);
            parameters.Add("@EndDate", dto.EndDate);
            parameters.Add("@StartDate", dto.StartDate);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@CreatedBy", dto.CreatedBy);
            parameters.Add("@VehicleList", vehicleTable.AsTableValuedParameter("dbo.RecallVehicleList"));

            var result = await connection.QuerySingleAsync<int>(
                "Recall_Insert",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }

        public async Task<int> UpdateAsync(UpdateRecallDTO dto)
        {
            using var connection = _context.CreateConnection();

            var vehicleTable = new DataTable();
            vehicleTable.Columns.Add("MakeId", typeof(int));
            vehicleTable.Columns.Add("ModelId", typeof(int));
            vehicleTable.Columns.Add("Chassis", typeof(string));
            vehicleTable.Columns.Add("RecallStatus", typeof(int));

            if (dto.Vehicles != null && dto.Vehicles.Count > 0)
            {
                foreach (var v in dto.Vehicles)
                {
                    vehicleTable.Rows.Add(
                        v.MakeID,
                        v.ModelID,
                        v.Chassis,
                        v.RecallStatus
                    );
                }
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Id", dto.Id);
            parameters.Add("@Code", dto.Code);
            parameters.Add("@Title", dto.Title);
            parameters.Add("@Description", dto.Description);
            parameters.Add("@EndDate", dto.EndDate);
            parameters.Add("@StartDate", dto.StartDate);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@UpdatedBy", dto.UpdatedBy);
            parameters.Add("@VehicleList",
                vehicleTable.AsTableValuedParameter("dbo.RecallVehicleList"));

                var result = await connection.QuerySingleAsync<int>(
                    "Recall_Update",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
                return result;

        }

        public async Task<int> DeleteAsync(DeleteRecallDTO dto)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Id", dto.Id);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@UpdatedBy", dto.UpdatedBy);


            var result = await connection.QuerySingleAsync<int>(
            "Recall_Delete",
            parameters,
            commandType: CommandType.StoredProcedure
        );

            return result;
        }

        public async Task<int> UpdateRecallVehicleStatus(string chassisNo, int statusId)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Chassis", chassisNo, DbType.String);
            parameters.Add("@RecallStatusId", statusId, DbType.Int32);

            var rowsAffected = await connection.QuerySingleAsync<int>(
                "RecallVehicle_UpdateStatus",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected;
        }
        public async Task<List<ActiveRecallsByChassisResponseDto>> GetActiveRecallsByChassisBulkAsync(List<string> chassisList)
        {
            using var connection = _context.CreateConnection();

            if (chassisList == null || chassisList.Count == 0)
                return new List<ActiveRecallsByChassisResponseDto>();

            var parameters = new DynamicParameters();
            parameters.Add( "@ChassisList",Newtonsoft.Json.JsonConvert.SerializeObject(chassisList), DbType.String);

            var rows = (await connection.QueryAsync<ActiveRecallBulkRowDto>("GetActiveRecallsByChassisBulk", parameters, commandType: CommandType.StoredProcedure)).ToList();

            var result = new List<ActiveRecallsByChassisResponseDto>();

            foreach (var chassis in chassisList)
            {
                var recalls = rows
                    .Where(r => r.ChassisNo == chassis && r.RecallId.HasValue)
                    .Select(r => new ActiveRecallDto
                    {
                        RecallId = r.RecallId!.Value,
                        Code = r.Code!,
                        Title = r.Title!,
                        Description = r.Description,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate,
                        IsActive = r.IsActive ?? false,
                        CreatedAt = r.CreatedAt,
                        CreatedBy = r.CreatedBy,
                        UpdatedAt = r.UpdatedAt,
                        UpdatedBy = r.UpdatedBy
                    })
                    .ToList();

                result.Add(new ActiveRecallsByChassisResponseDto
                {
                    ChassisNo = chassis,
                    Recalls = recalls
                });
            }

            return result;
        }

        public async Task<bool> CodeExistsAsync(string code)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@Code", code, DbType.String);

            var result = await connection.QuerySingleAsync<bool>(
                "Recall_CodeExists",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }




    }
}
