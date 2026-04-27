using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public sealed class LogService : ILogsService
    {
        private readonly ILogRepository _repository;

        public LogService(ILogRepository repository)
            => _repository = repository;

        public Task InsertAsync(LogEntryDto entry, CancellationToken ct = default)
            => _repository.InsertAsync(entry, ct);

        public Task InsertBatchAsync(IEnumerable<LogEntryDto> entries, CancellationToken ct = default)
            => _repository.InsertBatchAsync(entries, ct);
    }
}
