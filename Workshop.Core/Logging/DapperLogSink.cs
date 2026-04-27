using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Logging
{
    public sealed class DapperLogSink : ILogEventSink, IDisposable
    {
        private static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(2);
        private const string FallbackTemplate = "logs/api-fallback-.jsonl";

        private readonly ILogWriterService _writer;
        private readonly ILogEventSink _fileSink;

        private volatile bool _dbAvailable = true;
        private DateTime _lastFailureUtc = DateTime.MinValue;
        private readonly object _stateLock = new();

        public DapperLogSink(ILogWriterService writer)
        {
            _writer = writer;

            Directory.CreateDirectory("logs");
            _fileSink = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(
                    new CompactJsonFormatter(),
                    FallbackTemplate,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    shared: true)
                .CreateLogger();
        }

        public void Emit(LogEvent logEvent)
        {
            if (IsDbAvailable())
            {
                try
                {
                    _writer.Write(ToDto(logEvent));
                    MarkDbAvailable();
                    return;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"[DapperLogSink] DB write failed — falling back to file.\n" +
                        $"  Type   : {ex.GetType().FullName}\n" +
                        $"  Message: {ex.Message}\n" +
                        $"  Inner  : {ex.InnerException?.Message}");

                    MarkDbUnavailable();
                }
            }

            (_fileSink as ILogEventSink)!.Emit(logEvent);
        }

        public void Dispose() => (_fileSink as IDisposable)?.Dispose();

        private static LogEntryDto ToDto(LogEvent e) => new()
        {
            Timestamp = e.Timestamp,
            Level = e.Level.ToString(),
            Message = e.RenderMessage(),
            Exception = e.Exception?.ToString(),
            SourceContext = GetProp(e, "SourceContext"),
            RequestPath = GetProp(e, "RequestPath"),
            TraceId = GetProp(e, "TraceIdentifier"),
            MachineName = GetProp(e, "MachineName"),
            Application = GetProp(e, "Application")
        };

        private static string? GetProp(LogEvent e, string name)
            => e.Properties.TryGetValue(name, out var v)
                ? (v is ScalarValue sv ? sv.Value?.ToString() : v.ToString())
                : null;

        private bool IsDbAvailable()
        {
            if (_dbAvailable) return true;
            lock (_stateLock)
                return DateTime.UtcNow - _lastFailureUtc >= RetryInterval;
        }

        private void MarkDbAvailable()
        {
            if (!_dbAvailable) lock (_stateLock) _dbAvailable = true;
        }

        private void MarkDbUnavailable()
        {
            lock (_stateLock)
            {
                _dbAvailable = false;
                _lastFailureUtc = DateTime.UtcNow;
            }
        }
    }
}
