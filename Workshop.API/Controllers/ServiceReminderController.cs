using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceReminderController : Controller
    {
        private readonly IServiceReminderService _service;

        public ServiceReminderController(IServiceReminderService service)
        {
            _service = service;
        }

        [HttpPost("AddReminder")]
        public async Task<ActionResult> AddReminder([FromBody] CreateServiceReminderDTO serviceToCreate)
        {
            var result = await _service.AddServiceReminderAsync(serviceToCreate);
            return Ok(result);
        }

        [HttpPost("UpdateReminder")]
        public async Task<ActionResult> UpdateReminder([FromBody] UpdateServiceReminderDTO serviceToUpdate)
        {
            var result = await _service.UpdateServiceReminderAsync(serviceToUpdate);
            return Ok(result);
        }

        [HttpPost("GetReminder")]
        public async Task <ActionResult> GetReminder([FromBody] int serviceToGet)
        {
            var result = await _service.GetServiceReminderByIdAsync(serviceToGet);
            return Ok(result);
        }

        [HttpPost("GetAllReminder")]
        public async Task<ActionResult> GetAllReminder([FromBody] GetServiceReminderDTO getServiceReminderDTO)
        {
            var result = await _service.GetAllServiceRemindersAsync(getServiceReminderDTO);
            return Ok(result);
        }

        [HttpPost("DeleteReminder")]
        public async Task<ActionResult> DeleteReminder([FromBody] int serviceId)
        {
            var result = await _service.DeleteServiceReminderAsync(serviceId);
            return Ok(result);
        }

        [HttpGet("GetDueServiceReminders")]
        public async Task<ActionResult> GetDueServiceReminders()
        {
            var result = await _service.GetDueServiceReminders();
            return Ok(result);
        }

        [HttpGet("GetServiceRemindersStatus")]
        public async Task<ActionResult> ServiceRemindersStatus()
        {
            var result = await _service.ServiceRemindersStatus();
            return Ok(result);
        }

        [HttpPost("UpdateServiceScheduleByDamageId")]
        public async Task<ActionResult> UpdateServiceScheduleByDamageId(ServiceScheduleModel serviceScheduleModel)
        {
            var result = await _service.UpdateServiceScheduleByDamageId(serviceScheduleModel);
            return Ok(result);
        }

    }
}
