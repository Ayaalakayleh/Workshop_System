using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IServiceReminderService
    {

        public Task<bool> DeleteServiceReminderAsync(int id);
        public Task<int> AddServiceReminderAsync(CreateServiceReminderDTO serviceReminder);
        public Task<int> UpdateServiceReminderAsync(UpdateServiceReminderDTO serviceReminder);
        public Task<List<GetServiceReminderDTO>> GetAllServiceRemindersAsync(GetServiceReminderDTO getServiceReminderDTO);
        public Task<GetServiceReminderDTO?> GetServiceReminderByIdAsync(int id);
        public Task<List<ServiceReminderDue>> GetDueServiceReminders();
        public Task<List<ReminderStatus>> ServiceRemindersStatus();
        public Task<int> UpdateServiceScheduleByDamageId(ServiceScheduleModel serviceScheduleModel);

    }
}
