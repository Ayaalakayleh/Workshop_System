using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public sealed class LogEntryDto
    {
        public DateTimeOffset Timestamp { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Application { get; set; }
        public string? SourceContext { get; set; }
        public string? RequestPath { get; set; }
        public string? TraceId { get; set; }
        public string? MachineName { get; set; }
        public string? Exception { get; set; }
        public Dictionary<string, string>? Properties { get; set; }
    }
}
