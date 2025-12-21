using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Workshop.Core.DTOs;
using Workshop.Domain.Enum;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class WorkshopLoadingController : BaseController
    {
        private readonly WorkshopApiClient _apiclient;
        public WorkshopLoadingController(WorkshopApiClient apiclient, IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            this._apiclient = apiclient;
        }

        [CustomAuthorize(Permissions.WorkshopLoading.View)]
        public IActionResult Index()
        {
            return View();
        }


        //public async Task<IActionResult> GetTechniciansName(int? Id)
        //{
        //    try
        //    {
        //        var Obj = await _apiclient.GetTechniciansName(Id);
        //        return Ok(Obj);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Problem(title: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        //    }
        //}

        public async Task<JsonResult> GetGroupedServices(int Id)
        {
            try
            {
                var Obj = await _apiclient.GetGroupedServices(Id);
                return Json(new { success = true, data = Obj });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> GetServicesById(int Id, string lang)
        {
            try
            {
                var Obj = await _apiclient.WIP_GetServicesById(Id, lang);
                Obj = Obj.Where(x => x?.Status == 23).ToList();
                var scObj = await _apiclient.WIP_SChedule_GetAll();
                var objWipIds = scObj.Select(x => x?.WIPId).ToHashSet();
                //var notInScObj = Obj
                //.Where(o => !objWipIds.Contains(o?.WIPId))
                //.ToList();

                return Ok(Obj);
            }
            catch (Exception ex)
            {
                return Problem(title: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTechniciansSchedule(DateTime Date)
        {
            try
            {
                var Obj = await _apiclient.GetTechniciansSchedule(Date, BranchId);
                return Ok(Obj);
            }
            catch (Exception ex)
            {
                return Problem(title: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get_TechnicianAvailabilty()
        {
            try
            {
                var Obj = await _apiclient.Get_TechnicianAvailabilty();
                return Ok(Obj);
            }
            catch (Exception ex)
            {
                return Problem(title: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPost]
        [CustomAuthorize(Permissions.WorkshopLoading.Create)]
        public async Task<JsonResult> WIPSChedule([FromBody] WIPSChedule oWIPSChedule)
        {
            try
            {
                var scheduleList = await _apiclient.WIPSCheduleInsert(oWIPSChedule);
                if (scheduleList != null)
                {
                    UpdateService updateService = new UpdateService()
                    {
                        WIPId = oWIPSChedule.WIPId,
                        RTSId = oWIPSChedule.RTSId,
                        Status = (int)LabourLineEnum.Booked
                    };
                    await UpdateServiceStatus(updateService);
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        [CustomAuthorize(Permissions.WorkshopLoading.Edit)]
        private async Task UpdateServiceStatus(UpdateService updateService)
        {
            var result = await _apiclient.UpdateServiceStatus(updateService);
        }


        [HttpPost]
        public async Task<JsonResult> GetAvailableTechnicians([FromBody] UnscheduledLabourDTO dTO)
        {
            try
            {
                var avaliableTime = await _apiclient.GetAvailableTechniciansAsync(dTO.requestedDate, dTO.duration, BranchId);
                return Json(new
                {
                    success = true,
                    data = avaliableTime
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

    }
}
