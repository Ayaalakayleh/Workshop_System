using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkshopLoadingController : ControllerBase
    {

        private readonly IWorkshopLoadingService _service;
        public WorkshopLoadingController(IWorkshopLoadingService service)
        {
            this._service = service;
        }


        //[HttpGet("GetAllTechnicians")]
        //public async Task<ActionResult<IEnumerable<TechniciansNameDTO>>> GetAllTechnicians([FromQuery] int? Id)
        //{
        //    var result = await _service.GetTechniciansName(Id);
        //    return Ok(result);
        //}


        [HttpGet("GetTechniciansSchedule")]
        public async Task<ActionResult<IEnumerable<TechniciansNameDTO>>> GetTechniciansSchedule([FromQuery] DateTime Date, DateTime? DateTo, int BranchId)
        {
            var result = await _service.GetTechnicianSchedule(Date, DateTo, BranchId);
            return Ok(result);
        }
        [HttpGet("GetTechnicianAvailabilty")]
        public async Task<ActionResult<IEnumerable<TechniciansNameDTO>>> GetTechnicianAvailabilty()
        {
            var result = await _service.Get_TechnicianAvailabilty();
            return Ok(result);
        }


        [HttpGet("GetGroupedServices")]
        public async Task<IEnumerable<GroupedServicesDTO>> GetGroupedServices([FromQuery] int Id)
        {

            var Obj = await _service.GetGroupedServices(Id);
            return Obj;
        }
    }
}
