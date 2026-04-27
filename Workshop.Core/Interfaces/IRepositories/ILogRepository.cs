
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface ILogRepository
    {
        Task InsertAsync(LogEntryDto entry, CancellationToken ct = default);
        Task InsertBatchAsync(IEnumerable<LogEntryDto> entries, CancellationToken ct = default);
    }
}
