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

        public async Task<int> AddAsync(CreateRecallDTO dto)
        {
            using var connection = _context.CreateConnection();

            var vehicleTable = new DataTable();
            vehicleTable.Columns.Add("RecallID", typeof(int));
            vehicleTable.Columns.Add("MakeId", typeof(int));
            vehicleTable.Columns.Add("ModelId", typeof(int));
            vehicleTable.Columns.Add("Chassis", typeof(string));

            if (dto.Vehicles != null && dto.Vehicles.Count > 0)
            {
                foreach (var v in dto.Vehicles)
                {
                    vehicleTable.Rows.Add(DBNull.Value, v.MakeID, v.ModelID, v.Chassis);
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
            vehicleTable.Columns.Add("RecallID", typeof(int));
            vehicleTable.Columns.Add("MakeId", typeof(int));
            vehicleTable.Columns.Add("ModelId", typeof(int));
            vehicleTable.Columns.Add("Chassis", typeof(string));

            if (dto.Vehicles != null && dto.Vehicles.Count > 0)
            {
                foreach (var v in dto.Vehicles)
                {
                    vehicleTable.Rows.Add(v.RecallID, v.MakeID, v.ModelID, v.Chassis);
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
            parameters.Add("@VehicleList", vehicleTable.AsTableValuedParameter("dbo.RecallVehicleList"));

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


    }
}

