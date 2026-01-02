using Dapper;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;


namespace Workshop.Infrastructure.Repositories
{
    public class ClockingRepository : IClockingRepository
    {
        private readonly DapperContext _context;
        public ClockingRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<List<ClockingDTO>> GetClocks()
        {
            try
            {
                using var connection = _context.CreateConnection();
                var result = await connection.QueryAsync<ClockingDTO>(
                    "[dbo].[Clockings_GetAll]"

                );

                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<List<ClockingDTO>> GetClocksPaged(ClockingFilterDTO filterDTO)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@PageNumber", filterDTO.PageNumber, DbType.Int32);
                parameters.Add("@PageSize", filterDTO.PageSize, DbType.Int32);
                var result = await connection.QueryAsync<ClockingDTO>(
                    "[dbo].[Clockings_GetAll]"
                    ,parameters
                    , commandType: CommandType.StoredProcedure
                );

                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<ClockingDTO> GetClock(int Id)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", Id, DbType.Int32);
                var result = await connection.QueryAsync<ClockingDTO>(
                    "[dbo].[Clockings_GetById]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }
        public async Task<List<ClockingDTO>> GetClocksHistory()
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();

                var result = await connection.QueryAsync<ClockingDTO>(
                    "[dbo].[Clockings_GetHistory]",
                    commandType: CommandType.StoredProcedure
                );

                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<int> InsertClock(ClockingDTO dTO)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@TechnicianID", dTO.TechnicianID, DbType.Int32);
                parameters.Add("@WIPID", dTO.WIPID, DbType.Int32);
                parameters.Add("@RTSID", dTO.RTSID, DbType.Int32);
                parameters.Add("@KeyId", dTO.KeyId, DbType.Int32);
                parameters.Add("@StatusID", dTO.StatusID, DbType.Int32);
                parameters.Add("@AllowedTime", dTO.AllowedTime, DbType.Decimal);
                parameters.Add("@CreatedBy", dTO.CreatedBy, DbType.Int32);
                parameters.Add("@StartedAt", dTO.StartedAt, DbType.DateTime);

                var result = await connection.QueryAsync<int>(
                    "Clockings_InsertClocking",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<int> UpdateClock(ClockingDTO dTO)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@ID", dTO.ID, DbType.Int32);
                parameters.Add("@TechnicianID", dTO.TechnicianID, DbType.Int32);
                parameters.Add("@WIPID", dTO.WIPID, DbType.Int32);
                parameters.Add("@RTSID", dTO.RTSID, DbType.Int32);
                parameters.Add("@StatusID", dTO.StatusID, DbType.Int32);
                parameters.Add("@AllowedTime", dTO.AllowedTime, DbType.Decimal);
                parameters.Add("@EndedAt", dTO.EndedAt, DbType.DateTime);
                parameters.Add("@Elapsed", dTO.Elapsed?.ToString(@"hh\:mm\:ss"), DbType.Time);
                parameters.Add("@Breaks", dTO.Breaks, DbType.Int32);
                parameters.Add("@StartedAt", dTO.StartedAt, DbType.DateTime);
                var result = await connection.QueryAsync<int>(
                   "Clockings_UpdateClocking",
                   parameters,
                   commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<int> DeleteClock(int id)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@ID", id, DbType.Int32);

                var result = await connection.ExecuteAsync(
                   "Clockings_DeleteClocking",
                   parameters,
                   commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<int> InsertClockBreak(ClockingBreakDTO dTO)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@FK_ClockingID", dTO.ClockingID, DbType.Int32);
                parameters.Add("@Reason", dTO.Reason, DbType.Int32);
                parameters.Add("@Note", dTO.Note, DbType.String);
                parameters.Add("@Hint", dTO.Hint, DbType.String);
                parameters.Add("@StartAt", dTO.StartAt, DbType.DateTime2);
                parameters.Add("@EndAt", dTO.EndAt, DbType.DateTime2);
  

                var result = await connection.QueryAsync<int>(
                    "ClockingBreak_Insert",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<int> UpdateClockBreak(ClockingBreakDTO dTO)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@Id", dTO.ID, DbType.Int32);
                parameters.Add("@FK_ClockingID", dTO.ClockingID, DbType.Int32);
                parameters.Add("@Reason", dTO.Reason, DbType.Int32);
                parameters.Add("@Note", dTO.Note, DbType.String);
                parameters.Add("@Hint", dTO.Hint, DbType.String);
                parameters.Add("@StartAt", dTO.StartAt, DbType.DateTime2);
                parameters.Add("@EndAt", dTO.EndAt, DbType.DateTime2);


                var result = await connection.QueryAsync<int>(
                    "ClockingBreak_Update",
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<ClockingBreakDTO> GetLastBreakByClockID(int Id)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@ClockingID", Id, DbType.Int32);
                var result = await connection.QueryAsync<ClockingBreakDTO>(
                    "[dbo].[ClockingBreak_GetLastBreakByClockID]",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }


        public async Task<List<ClockingBreakDTO>> GetAllClocksBreaksDDL()
        {
            try
            {
                using var connection = _context.CreateConnection();
                var result = await connection.QueryAsync<ClockingBreakDTO>(
                    "[dbo].[ClockingBreak_GetALLDDL]"

                );

                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<List<ClockingBreakDTO>> GetClocksBreaksPaged(ClockingFilterDTO filterDTO)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@PageNumber", filterDTO.PageNumber, DbType.Int32);
                parameters.Add("@PageSize", filterDTO.PageSize, DbType.Int32);
                var result = await connection.QueryAsync<ClockingBreakDTO>(
                    "[dbo].[ClockingBreak_GetALLPaged]"
                    ,
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }

        public async Task<List<ClockingBreakDTO>> GetBreaksByClockID(int ClockID)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var parameters = new DynamicParameters();
                parameters.Add("@ClockID", ClockID, DbType.Int32);
                var result = await connection.QueryAsync<ClockingBreakDTO>(
                    "[dbo].[ClockingBreak_GetALLByClockID]"
                    ,parameters
                    , commandType: CommandType.StoredProcedure
                );

                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }
        }



        public async Task<List<GetClockingFilter>> GetClockingFilter()
        {

            try
            {
                using var connection = _context.CreateConnection();
                var result = await connection.QueryAsync<GetClockingFilter>(
                    "[dbo].[WIP_GetClockingFilter]"

                );

                return result?.ToList();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving work orders with filters {@Filter}", filter);
                throw;
            }

        }

    }
}
