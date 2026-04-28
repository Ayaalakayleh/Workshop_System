
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface ILogsService
    {
        Task InsertAsync(LogEntryDto entry, CancellationToken ct = default);
        Task InsertBatchAsync(IEnumerable<LogEntryDto> entries, CancellationToken ct = default);
    }
}
