using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class ServiceReminderPageVM
    {
        public IEnumerable<ServiceReminderDTO> Reminders { get; set; }
        public ServiceReminderDTO ReminderForm { get; set; }
        public int? ScheduledRemindersCount { get; set; } = 0;
        public int? DueSoonRemindersCount { get; set; } = 0;
        public int? OverdueRemindersCount { get; set; } = 0;
    }

}
