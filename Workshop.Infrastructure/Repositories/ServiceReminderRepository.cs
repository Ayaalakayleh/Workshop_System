using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics.Metrics;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Workshop.Infrastructure.Repositories
{
    public class ServiceReminderRepository : IServiceReminderRepository
    {
        private readonly DapperContext _context;
        public ServiceReminderRepository(DapperContext context)
        {
            _context = context;
        }

        //public async Task<int> AddServiceReminderAsync(CreateServiceReminderDTO serviceReminder)
        //{

        //    using var connection = _context.CreateConnection();

        //    try
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@VehicleId", serviceReminder.VehicleId);
        //        parameters.Add("@ManufacturerId", serviceReminder.ManufacturerId);
        //        parameters.Add("@VehicleModelId", serviceReminder.VehicleModelId);
        //        parameters.Add("@ManufacturingYear", serviceReminder.ManufacturingYear);
        //        parameters.Add("@ItemId", serviceReminder.ItemId);
        //        parameters.Add("@Repates", serviceReminder.Repates);
        //        parameters.Add("@TimeInterval", serviceReminder.TimeInterval);
        //        parameters.Add("@TimeIntervalUnit", serviceReminder.TimeIntervalUnit);
        //        parameters.Add("@TimeDue", serviceReminder.TimeDue);
        //        parameters.Add("@TimeDueUnit", serviceReminder.TimeDueUnit);
        //        parameters.Add("@PrimaryMeterInterval", serviceReminder.PrimaryMeterInterval);
        //        parameters.Add("@PrimaryMeterDue", serviceReminder.PrimaryMeterDue);
        //        parameters.Add("@IsManually", serviceReminder.IsManually);
        //        parameters.Add("@ManualDate", serviceReminder.ManualDate);
        //        parameters.Add("@ManualPrimaryMeter", serviceReminder.ManualPrimaryMeter);
        //        parameters.Add("@HasNotification", serviceReminder.HasNotification);
        //        parameters.Add("@CreatedBy", serviceReminder.CreatedBy);
        //        parameters.Add("@CurrentMeter", serviceReminder.CurrentMeter);
        //        parameters.Add("@NotificationsGroup", serviceReminder.NotificationsGroup);
        //        parameters.Add("@VehicleGroupId", serviceReminder.VehicleGroupId);




        //        // Execute SP and return affected rows (or Id if you adjust SP to output SCOPE_IDENTITY)
        //        var rowsAffected = await connection.QueryAsync<int>(
        //            "D_ServiceReminders_Insert",
        //            parameters,
        //            commandType: CommandType.StoredProcedure
        //        );

        //        return rowsAffected.FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in InsertServiceReminderAsync: {ex.Message}");
        //        throw;
        //    }
        //}


        //public async Task<int> AddServiceReminderAsync(CreateServiceReminderDTO serviceReminder)
        //{
        //    using var connection = _context.CreateConnection();

        //    try
        //    {
        //        var parameters = new DynamicParameters();

        //        parameters.Add("@VehicleId", serviceReminder.VehicleId);
        //        parameters.Add("@ManufacturerId", serviceReminder.ManufacturerId);
        //        parameters.Add("@VehicleModelId", serviceReminder.VehicleModelId);
        //        parameters.Add("@ManufacturingYear", serviceReminder.ManufacturingYear);
        //        parameters.Add("@ItemId", serviceReminder.ItemId);
        //        parameters.Add("@Repates", serviceReminder.Repates);
        //        parameters.Add("@TimeInterval", serviceReminder.TimeInterval);
        //        parameters.Add("@TimeIntervalUnit", serviceReminder.TimeIntervalUnit);
        //        parameters.Add("@TimeDue", serviceReminder.TimeDue);
        //        parameters.Add("@TimeDueUnit", serviceReminder.TimeDueUnit);
        //        parameters.Add("@PrimaryMeterInterval", serviceReminder.PrimaryMeterInterval);
        //        parameters.Add("@PrimaryMeterDue", serviceReminder.PrimaryMeterDue);
        //        parameters.Add("@IsManually", serviceReminder.IsManually);
        //        parameters.Add("@ManualDate", serviceReminder.ManualDate);
        //        parameters.Add("@ManualPrimaryMeter", serviceReminder.ManualPrimaryMeter);
        //        parameters.Add("@HasNotification", serviceReminder.HasNotification);
        //        parameters.Add("@CreatedBy", serviceReminder.CreatedBy);
        //        parameters.Add("@CurrentMeter", serviceReminder.CurrentMeter);
        //        parameters.Add("@VehicleGroupId", serviceReminder.VehicleGroupId);

        //        // ------------------------------
        //        // 🔥 ADD: Convert List<int> to TVP
        //        // ------------------------------
        //        var dt = new DataTable();
        //        dt.Columns.Add("Id", typeof(int));

        //        if (serviceReminder.NotificationsGroupId != null)
        //        {
        //            foreach (var id in serviceReminder.NotificationsGroupId)
        //                dt.Rows.Add(id);
        //        }

        //        parameters.Add("@NotificationsGroupIds",
        //            dt.AsTableValuedParameter("IntListType"));

        //        // ------------------------------

        //        var result = await connection.QueryAsync<int>(
        //            "D_ServiceReminders_Insert",
        //            parameters,
        //            commandType: CommandType.StoredProcedure
        //        );

        //        return result.FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in AddServiceReminderAsync: {ex.Message}");
        //        throw;
        //    }
        //}


        //public async Task<int> AddServiceReminderAsync(CreateServiceReminderDTO serviceReminder)
        //{
        //    using var connection = _context.CreateConnection();

        //    try
        //    {
        //        var parameters = new DynamicParameters();

        //        parameters.Add("@VehicleId", serviceReminder.VehicleId);
        //        parameters.Add("@ManufacturerId", serviceReminder.ManufacturerId);
        //        parameters.Add("@VehicleModelId", serviceReminder.VehicleModelId);
        //        parameters.Add("@ManufacturingYear", serviceReminder.ManufacturingYear);
        //        parameters.Add("@ItemId", serviceReminder.ItemId);
        //        parameters.Add("@Repates", serviceReminder.Repates);
        //        parameters.Add("@TimeInterval", serviceReminder.TimeInterval);
        //        parameters.Add("@TimeIntervalUnit", serviceReminder.TimeIntervalUnit);
        //        parameters.Add("@TimeDue", serviceReminder.TimeDue);
        //        parameters.Add("@TimeDueUnit", serviceReminder.TimeDueUnit);
        //        parameters.Add("@PrimaryMeterInterval", serviceReminder.PrimaryMeterInterval);
        //        parameters.Add("@PrimaryMeterDue", serviceReminder.PrimaryMeterDue);
        //        parameters.Add("@IsManually", serviceReminder.IsManually);
        //        parameters.Add("@ManualDate", serviceReminder.ManualDate);
        //        parameters.Add("@ManualPrimaryMeter", serviceReminder.ManualPrimaryMeter);
        //        parameters.Add("@HasNotification", serviceReminder.HasNotification);
        //        parameters.Add("@CreatedBy", serviceReminder.CreatedBy);
        //        parameters.Add("@CurrentMeter", serviceReminder.CurrentMeter);
        //        parameters.Add("@NotificationsGroup", serviceReminder.NotificationsGroup);
        //        parameters.Add("@VehicleGroupId", serviceReminder.VehicleGroupId);

        //        // ✅ ADD TABLE-VALUED PARAMETER
        //        var tvp = new DataTable();
        //        tvp.Columns.Add("GroupId", typeof(int));

        //        if (serviceReminder.NotificationsGroupId != null)
        //        {
        //            foreach (var id in serviceReminder.NotificationsGroupId)
        //                tvp.Rows.Add(id);
        //        }

        //        parameters.Add("@NotificationsGroupIds",
        //            tvp.AsTableValuedParameter("IntListType"));

        //        var rowsAffected = await connection.QueryAsync<int>(
        //            "D_ServiceReminders_Insert",
        //            parameters,
        //            commandType: CommandType.StoredProcedure
        //        );

        //        return rowsAffected.FirstOrDefault();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in InsertServiceReminderAsync: {ex.Message}");
        //        throw;
        //    }
        //}

        public async Task<int> AddServiceReminderAsync(CreateServiceReminderDTO serviceReminder)
        {
            using var connection = _context.CreateConnection();

            try
            {
                var parameters = new DynamicParameters();

                parameters.Add("@VehicleId", serviceReminder.VehicleId);
                parameters.Add("@ManufacturerId", serviceReminder.ManufacturerId);
                parameters.Add("@VehicleModelId", serviceReminder.VehicleModelId);
                parameters.Add("@ManufacturingYear", serviceReminder.ManufacturingYear);
                parameters.Add("@ItemId", serviceReminder.ItemId);
                parameters.Add("@Repates", serviceReminder.Repates);
                parameters.Add("@TimeInterval", serviceReminder.TimeInterval);
                parameters.Add("@TimeIntervalUnit", serviceReminder.TimeIntervalUnit);
                parameters.Add("@TimeDue", serviceReminder.TimeDue);
                parameters.Add("@TimeDueUnit", serviceReminder.TimeDueUnit);
                parameters.Add("@PrimaryMeterInterval", serviceReminder.PrimaryMeterInterval);
                parameters.Add("@PrimaryMeterDue", serviceReminder.PrimaryMeterDue);
                parameters.Add("@IsManually", serviceReminder.IsManually);
                parameters.Add("@ManualDate", serviceReminder.ManualDate);
                parameters.Add("@ManualPrimaryMeter", serviceReminder.ManualPrimaryMeter);
                parameters.Add("@HasNotification", serviceReminder.HasNotification);
                parameters.Add("@CreatedBy", serviceReminder.CreatedBy);
                parameters.Add("@CurrentMeter", serviceReminder.CurrentMeter);
                //parameters.Add("@NotificationsGroup", serviceReminder.NotificationsGroup);
                parameters.Add("@VehicleGroupId", serviceReminder.VehicleGroupId);

                // -------------------------------
                // ✅ Convert List<int> to CSV
                // -------------------------------
                string csv = serviceReminder.NotificationsGroupId != null && serviceReminder.NotificationsGroupId.Any()
                    ? string.Join(",", serviceReminder.NotificationsGroupId)
                    : null;

                parameters.Add("@NotificationsGroup", csv);

                // ------------------------------------------------
                // Execute SP and return new ID
                // ------------------------------------------------
                var result = await connection.QuerySingleOrDefaultAsync<int>(
                    "D_ServiceReminders_Insert",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddServiceReminderAsync: {ex.Message}");
                throw;
            }
        }


        public async Task<bool> DeleteServiceReminderAsync(int id)
        {
            using var connection = _context.CreateConnection();

            try
            {
                var affectedRows = await connection.ExecuteAsync(
                    "D_ServiceReminders_Delete",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );

                // If deletion succeeds, affectedRows should be >= 1 (main + child rows deleted)
                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting ServiceReminder with Id {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<List<GetServiceReminderDTO>> GetAllServiceRemindersAsync(GetServiceReminderDTO getServiceReminderDTO)
        {

            using var connection = _context.CreateConnection();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Page", getServiceReminderDTO.PageNumber);
                parameters.Add("@VehicleId", getServiceReminderDTO.VehicleId);
                parameters.Add("@ManufacturerId", getServiceReminderDTO.ManufacturerId);
                parameters.Add("@VehicleModelId", getServiceReminderDTO.VehicleModelId);
                parameters.Add("@ReminderStatus", getServiceReminderDTO.ReminderStatus);
                parameters.Add("@ManufacturingYear", getServiceReminderDTO.ManufacturingYear);
                parameters.Add("@SelectedServices", getServiceReminderDTO.ServiceName);

                // ✅ Use parameters directly
                var reminders = (await connection.QueryAsync<GetServiceReminderDTO>(
                    "D_ServiceReminders_GetAll_Page",
                    parameters,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int totalPages = reminders.Count > 0 ? reminders[0].TotalPages : 0;
                return (reminders);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return (new List<GetServiceReminderDTO>());
            }
        }

        public async Task<GetServiceReminderDTO?> GetServiceReminderByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            try
            {
                var reminder = await connection.QuerySingleOrDefaultAsync<GetServiceReminderDTO>(
                    "D_ServiceReminders_ById",
                    new { Id = id },
                    commandType: CommandType.StoredProcedure
                );
                return reminder;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetServiceReminderByIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<int> UpdateServiceReminderAsync(UpdateServiceReminderDTO dto)
        {
            using var connection = _context.CreateConnection();

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Id", dto.Id);
                parameters.Add("@VehicleId", dto.VehicleId);
                parameters.Add("@ItemId", dto.ItemId);
                parameters.Add("@Repates", dto.Repates);
                parameters.Add("@TimeInterval", dto.TimeInterval);
                parameters.Add("@TimeIntervalUnit", dto.TimeIntervalUnit);
                parameters.Add("@TimeDue", dto.TimeDue);
                parameters.Add("@TimeDueUnit", dto.TimeDueUnit);
                parameters.Add("@PrimaryMeterInterval", dto.PrimaryMeterInterval);
                parameters.Add("@PrimaryMeterDue", dto.PrimaryMeterDue);
                parameters.Add("@IsManually", dto.IsManually);
                parameters.Add("@ManualDate", dto.ManualDate);
                parameters.Add("@ManualPrimaryMeter", dto.ManualPrimaryMeter);
                parameters.Add("@HasNotification", dto.HasNotification);
                parameters.Add("@ModifyBy", dto.ModifyBy);
                parameters.Add("@CurrentMeter", dto.CurrentMeter);
                parameters.Add("@StartMeter", dto.StartMeter);
                parameters.Add("@StartDate", dto.StartDate);
                parameters.Add("@NotificationsGroup", dto.NotificationsGroup);


                string csv = dto.NotificationsGroupId != null && dto.NotificationsGroupId.Any()
    ? string.Join(",", dto.NotificationsGroupId)
    : null;

                parameters.Add("@NotificationsGroup", csv);

                var rowsAffected = await connection.QueryAsync<int>(
                    "D_ServiceReminders_Update",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return rowsAffected.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateServiceReminderAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ServiceReminderDue>> GetDueServiceReminders()
        {
            using var connection = _context.CreateConnection();
            try
            {
                var rowsAffected = await connection.QueryAsync<ServiceReminderDue>("D_ServiceReminders_GetDueServiceReminders", commandType: CommandType.StoredProcedure);
                return rowsAffected.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetDueServiceReminders: {ex.Message}");
                throw;
            }

        }
        public async Task<List<ReminderStatus>> ServiceRemindersStatus()
        {

            using var connection = _context.CreateConnection();
            try
            {
                var rowsAffected = await connection.QueryAsync<ReminderStatus>("D_ServiceReminders_Status", commandType: CommandType.StoredProcedure);
                return rowsAffected.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ServiceRemindersStatus: {ex.Message}");
                throw;
            }

        }
        public async Task<int> UpdateServiceScheduleByDamageId(ServiceScheduleModel serviceScheduleModel)
        {
            using var connection = _context.CreateConnection();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@VehicleId", serviceScheduleModel.VehicleId);
                parameters.Add("@DamageId", serviceScheduleModel.DamageId);
                parameters.Add("@Meter", serviceScheduleModel.Meter);
                parameters.Add("@Date", serviceScheduleModel.Date);

                var rowsAffected = await connection.ExecuteAsync(
                "D_ServiceReminders_UpdateServiceScheduleByDamageId",
                 parameters,
                commandType: CommandType.StoredProcedure
                );
                return rowsAffected;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ServiceRemindersStatus: {ex.Message}");
                throw;
            }

        }

    }
}
