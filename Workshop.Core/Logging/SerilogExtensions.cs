using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Logging
{
    public static class SerilogExtensions
    {
        /// <summary>
        /// Used by Inventory.API — writes directly to dbo.Logs
        /// via Dapper + InsertLog. Falls back to a rolling file.
        /// </summary>
        public static LoggerConfiguration WithDapperSink(
            this LoggerSinkConfiguration sinkConfig,
            ILogWriterService writer)
            => sinkConfig.Sink(new DapperLogSink(writer));

        /// <summary>
        /// Used by Inventory.Web — POSTs batches to Inventory.API
        /// /api/v1/logs. Falls back to a rolling file when API is down.
        /// </summary>
        public static LoggerConfiguration WithApiFallback(
            this LoggerSinkConfiguration sinkConfig,
            string apiBaseUrl)
            => sinkConfig.Sink(new ApiHttpSink(apiBaseUrl));
    }
}
