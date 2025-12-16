using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class ClockingController : BaseController
    {

        WorkshopApiClient _apiClient;
        public readonly string lang;
        public ClockingController(WorkshopApiClient apiClient, 
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {

            _apiClient = apiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.Clocking.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                var filterTechnician = new FilterTechnicianDTO
                {
                    PageNumber = 1,
                    Email = "",
                    Name = "",
                    WorkshopId = BranchId
                };

                var technicians = await _apiClient.GetAllPINTechniciansAsync(filterTechnician);

                var clockingModel = new ClockingModel
                {
                    ClockingForm = new ClockingDTO
                    {
                        TechnicianID = 0,
                        WIPID = 0,
                        RTSID = 0,
                        TechnicianName = ""
                    },
                    ClockingList = new List<ClockingDTO>(),
                    Technicians = technicians?.ToList() ?? new List<TechnicianDTO>(),
                    Labourlines = new List<CreateWIPServiceDTO>(),
                    WIPS = new List<WIPDTO>() // if you have this property in your model
                };

                return View(clockingModel);
            }
            catch (Exception ex)
            {
                // optional: log exception
                return View(new ClockingModel
                {
                    ClockingForm = new ClockingDTO(),
                    Technicians = new List<TechnicianDTO>(),
                    Labourlines = new List<CreateWIPServiceDTO>()
                });
            }
        }


    }
}
