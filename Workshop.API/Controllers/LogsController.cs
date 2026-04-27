using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class LogsController : ControllerBase
    {
        private readonly ILogsService _logsService;

        public LogsController(ILogsService logsService)
            => _logsService = logsService;

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Ingest(
            [FromBody] IReadOnlyList<LogEntryDto> entries,
            CancellationToken ct)
        {
            if (entries is null || entries.Count == 0)
                return BadRequest("No log entries provided.");

            try
            {
                await _logsService.InsertBatchAsync(entries, ct);
                return Ok(new { inserted = entries.Count });
            }
            catch (Exception ex)
            {
                Serilog.Log.Warning(ex,
                    "LogsController: failed to persist {Count} entries",
                    entries.Count);

                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    "Log storage temporarily unavailable.");
            }
        }
    }
}
