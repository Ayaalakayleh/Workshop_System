using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public sealed class LogRepository : ILogRepository
    {
        private const int CommandTimeoutSeconds = 5;

        private readonly DapperContext _db;

        public LogRepository(DapperContext db) => _db = db;

        public async Task InsertAsync(LogEntryDto entry, CancellationToken ct = default)
        {
            using var connection = _db.CreateConnection();
            await connection.ExecuteAsync(
                "dbo.InsertLog",
                MapToParameters(entry),
                commandType: CommandType.StoredProcedure,
                commandTimeout: CommandTimeoutSeconds);
        }

        public async Task InsertBatchAsync(IEnumerable<LogEntryDto> entries, CancellationToken ct = default)
        {
            using var connection = _db.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                foreach (var entry in entries)
                {
                    await connection.ExecuteAsync(
                        "dbo.InsertLog",
                        MapToParameters(entry),
                        transaction: transaction,
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: CommandTimeoutSeconds);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private static object MapToParameters(LogEntryDto e) => new
        {
            Timestamp = e.Timestamp.UtcDateTime,
            Level = e.Level,
            Message = e.Message,
            Application = e.Application,
            SourceContext = e.SourceContext,
            RequestPath = e.RequestPath,
            TraceId = e.TraceId,
            MachineName = e.MachineName,
            Exception = e.Exception,
            Properties = e.Properties is { Count: > 0 }
                                ? System.Text.Json.JsonSerializer.Serialize(e.Properties)
                                : null
        };
    }
}
