using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IServiceReminderRepository
    {
        public Task<bool> DeleteServiceReminderAsync(int id);
        public Task<int> AddServiceReminderAsync(CreateServiceReminderDTO serviceReminderDTO);
        public Task<int> UpdateServiceReminderAsync(UpdateServiceReminderDTO serviceReminderDTO);
        public Task<List<GetServiceReminderDTO>> GetAllServiceRemindersAsync(GetServiceReminderDTO getServiceReminderDTO);
        public Task<GetServiceReminderDTO?> GetServiceReminderByIdAsync(int id);
        public Task<List<ServiceReminderDue>> GetDueServiceReminders();
        public Task<List<ReminderStatus>> ServiceRemindersStatus();
        public Task<int> UpdateServiceScheduleByDamageId(ServiceScheduleModel serviceScheduleModel);


    }
}
