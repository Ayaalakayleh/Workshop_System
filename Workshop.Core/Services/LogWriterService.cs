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
    public sealed class LogWriterService : ILogWriterService
    {
        private readonly ILogRepository _repo;

        public LogWriterService(ILogRepository repo) => _repo = repo;

        public void Write(LogEntryDto entry)
        {
            _repo.InsertAsync(entry)
                 .ConfigureAwait(false)
                 .GetAwaiter()
                 .GetResult();
        }
    }
}
