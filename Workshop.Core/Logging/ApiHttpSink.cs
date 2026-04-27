using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Compact;
using System.Net.Http.Json;
using Workshop.Core.DTOs;

namespace Workshop.Core.Logging
{
    public sealed class ApiHttpSink : ILogEventSink, IDisposable
    {
        private static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);
        private const int BatchSize = 50;
        private const string FallbackTemplate = "logs/web-fallback-.jsonl";

        private readonly HttpClient _http;
        private readonly string _endpoint;
        private readonly ILogEventSink _fileSink;

        private readonly Queue<LogEvent> _buffer = new();
        private readonly object _bufferLock = new();
        private readonly Timer _flushTimer;

        private volatile bool _apiAvailable = true;
        private DateTime _lastFailureUtc = DateTime.MinValue;
        private readonly object _stateLock = new();

        public ApiHttpSink(string apiBaseUrl)
        {
            _endpoint = apiBaseUrl.TrimEnd('/') + "/api/logs";
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

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

            _flushTimer = new Timer(_ => Flush(), null, FlushInterval, FlushInterval);
        }

        public void Emit(LogEvent logEvent)
        {
            lock (_bufferLock)
            {
                _buffer.Enqueue(logEvent);
                if (_buffer.Count >= BatchSize)
                    Flush();
            }
        }

        private void Flush()
        {
            List<LogEvent> batch;

            lock (_bufferLock)
            {
                if (_buffer.Count == 0) return;
                batch = [.. _buffer];
                _buffer.Clear();
            }

            if (IsApiAvailable())
            {
                try
                {
                    SendBatchAsync(batch).GetAwaiter().GetResult();
                    MarkApiAvailable();
                    return;
                }
                catch
                {
                    MarkApiUnavailable();
                }
            }

            foreach (var e in batch)
                (_fileSink as ILogEventSink)!.Emit(e);
        }

        private Task SendBatchAsync(IEnumerable<LogEvent> batch)
        {
            var dtos = batch.Select(e => new LogEntryDto
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
            }).ToList();

            return _http.PostAsJsonAsync(_endpoint, dtos);
        }

        private static string? GetProp(LogEvent e, string name)
            => e.Properties.TryGetValue(name, out var v)
                ? (v is ScalarValue sv ? sv.Value?.ToString() : v.ToString())
                : null;

        private bool IsApiAvailable()
        {
            if (_apiAvailable) return true;
            lock (_stateLock)
                return DateTime.UtcNow - _lastFailureUtc >= RetryInterval;
        }

        private void MarkApiAvailable()
        {
            if (!_apiAvailable) lock (_stateLock) _apiAvailable = true;
        }

        private void MarkApiUnavailable()
        {
            lock (_stateLock) { _apiAvailable = false; _lastFailureUtc = DateTime.UtcNow; }
        }

        public void Dispose()
        {
            _flushTimer.Dispose();
            Flush(); // drain on shutdown
            _http.Dispose();
            (_fileSink as IDisposable)?.Dispose();
        }
    }
}
