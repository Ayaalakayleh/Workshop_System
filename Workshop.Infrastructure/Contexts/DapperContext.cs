using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Infrastructure.Contexts
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _connectionString = configuration.GetConnectionString("Default")
                                ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
