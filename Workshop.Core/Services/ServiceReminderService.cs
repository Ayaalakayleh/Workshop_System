
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class ServiceReminderService : IServiceReminderService
    {
        private readonly IServiceReminderRepository _repository;
        public ServiceReminderService(IServiceReminderRepository repository)
        {
            _repository = repository;
        }
        public Task<int> AddServiceReminderAsync(CreateServiceReminderDTO serviceReminder)
        {
            return _repository.AddServiceReminderAsync(serviceReminder);
        }

        public Task<bool> DeleteServiceReminderAsync(int id)
        {
            return _repository.DeleteServiceReminderAsync(id);
        }

        public Task<List<GetServiceReminderDTO>>  GetAllServiceRemindersAsync(GetServiceReminderDTO getServiceReminderDTO)
        {
            return _repository.GetAllServiceRemindersAsync(getServiceReminderDTO);
        }

        public Task<List<ServiceReminderDue>> GetDueServiceReminders()
        {
            return _repository.GetDueServiceReminders();
        }

        public Task<GetServiceReminderDTO?> GetServiceReminderByIdAsync(int id)
        {
           return _repository.GetServiceReminderByIdAsync(id);
        }

        public Task<int> UpdateServiceReminderAsync(UpdateServiceReminderDTO serviceReminder)
        {
            return _repository.UpdateServiceReminderAsync(serviceReminder);
        }
        public Task<List<ReminderStatus>> ServiceRemindersStatus()
        {
            return _repository.ServiceRemindersStatus();
        }
        public Task<int> UpdateServiceScheduleByDamageId(ServiceScheduleModel serviceScheduleModel)
        {
            return _repository.UpdateServiceScheduleByDamageId(serviceScheduleModel);
        }
    }

}
