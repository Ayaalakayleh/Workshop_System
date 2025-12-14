using System.ComponentModel;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workshop.Core.Security;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{

    public class Database
    {
        private readonly ILogger<Database> _logger;
        private readonly SecurityHelper _securityHelper;
        private readonly DapperContext _context;

        public Database(
            ILogger<Database> logger,
            SecurityHelper securityHelper,
            DapperContext context
            )
        {
            _logger = logger;
            _securityHelper = securityHelper;
            _context = context;
        }

        #region Commands
        public async Task<IEnumerable<T>> ExecuteGetAllStoredProcedure<T>(string procedureName, object parameters)
        {
            //ValidateInput(procedureName, parameters);

            try
            {

                return await _context.CreateConnection().QueryAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                //LogDatabaseError(procedureName, parameters, ex);

                throw new Exception("Database operation failed", ex);
            }
        }

        public async Task<T> ExecuteGetByIdProcedure<T>(string procedureName, object parameters)
        {
            ValidateInput(procedureName, parameters);

            try
            {

                return await _context.CreateConnection().QuerySingleOrDefaultAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                LogDatabaseError(procedureName, parameters, ex);
                throw new Exception("Database operation failed", ex);
            }
        }

        public async Task<T> ExecuteAddStoredProcedure<T>(string procedureName, object parameters)
        {
            ValidateInput(procedureName, parameters);

            try
            {

                return await _context.CreateConnection().QuerySingleAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                LogDatabaseError(procedureName, parameters, ex);
                throw new Exception("Database operation failed", ex);
            }
        }

        public async Task<T> ExecuteUpdateProcedure<T>(string procedureName, object parameters)
        {
            return await ExecuteQueryFirstProcedure<T>(procedureName, parameters);
        }

        public async Task<T> ExecuteDeleteProcedure<T>(string procedureName, object parameters)
        {
            return await ExecuteQueryFirstProcedure<T>(procedureName, parameters);
        }

        private async Task<T> ExecuteQueryFirstProcedure<T>(string procedureName, object parameters)
        {
            ValidateInput(procedureName, parameters);

            try
            {

                return await _context.CreateConnection().QueryFirstAsync<T>(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                LogDatabaseError(procedureName, parameters, ex);
                throw new Exception("Database operation failed", ex);
            }
        }

        public async Task ExecuteNonReturnProcedure(string procedureName, object parameters)
        {
            ValidateInput(procedureName, parameters);

            try
            {
                await _context.CreateConnection().ExecuteAsync(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                LogDatabaseError(procedureName, parameters, ex);
                throw new Exception("Database operation failed", ex);
            }
        }

        public async Task<List<object>> ExecuteGetMultipleTablesAsync(string procedureName, object parameters, params Type[] types)
        {
            ValidateInput(procedureName, parameters);

            if (types == null || types.Length == 0)
                throw new ArgumentException("At least one type must be specified", nameof(types));

            try
            {
                using var connection = _context.CreateConnection();
                using var multi = await connection.QueryMultipleAsync(
                    procedureName,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                var results = new List<object>();

                for (int i = 0; i < types.Length; i++)
                {
                    // Use dynamic to call the appropriate ReadAsync method
                    dynamic result = await multi.ReadAsync(types[i]);
                    if (result != null)
                        results.Add(result);
                }

                connection.Close();
                connection.Dispose();

                multi.Dispose();
                return results;
            }
            catch (SqlException ex)
            {
                LogDatabaseError(procedureName, parameters, ex);
                throw new Exception("Database operation failed", ex);
            }
        }
        #endregion

        #region Connection Management

        // Should be in the dapper context
        //private string GetDecryptedConnectionString()
        //{
        //    var connectionString = _configuration.GetConnectionString("Default");

        //    var builder = new SqlConnectionStringBuilder(connectionString);


        //    //if (!string.IsNullOrEmpty(builder.Password))
        //    //{
        //    //    builder.Password = _securityHelper.DecryptString(builder.Password, "database2019");
        //    //}

        //    return builder.ConnectionString;
        //}
        #endregion

        #region Helpers
        public DataTable ToDataTable<T>(List<T> data)
        {

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            try
            {
                for (int i = 0; i < props.Count; i++)
                {
                    PropertyDescriptor prop = props[i];
                    //table.Columns.Add(prop.Name, prop.PropertyType);
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(
            prop.PropertyType) ?? prop.PropertyType);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            object[] values = new object[props.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item) ?? (object)DBNull.Value;
                }
                table.Rows.Add(values);
            }
            return table;
        }

        private void ValidateInput(string procedureName, object parameters)
        {
            if (string.IsNullOrWhiteSpace(procedureName))
                throw new ArgumentException("Procedure name cannot be empty", nameof(procedureName));

            // Additional validation if needed
        }

        private void LogDatabaseError(string procedureName, object parameters, Exception ex)
        {
            var errorDetails = new Dictionary<string, object>
            {
                ["Procedure"] = procedureName,
                ["Parameters"] = parameters
            };

            //_applicationLogger.LogError("Database Error", errorDetails, ex);
            _logger.LogError(ex, "Database error executing {Procedure}", procedureName);
        }
        #endregion
    }
}
