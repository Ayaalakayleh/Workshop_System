using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Core.DTOs;
using Workshop.Infrastructure;
using Workshop.Web.Services;


namespace Workshop.Web.Controllers
{
    public class LabourRateManagementController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly string lang;
        public LabourRateManagementController(WorkshopApiClient apiClient, ERPApiClient erpApiClient,
           IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            _configuration = configuration;
            _env = env;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }
        public async Task<IActionResult> Index([FromQuery] FilterLabourRateDTO oFilterLabourRateDTO)
        {
            oFilterLabourRateDTO ??= new FilterLabourRateDTO();
            oFilterLabourRateDTO.lang = lang;
            oFilterLabourRateDTO.PageNumber = oFilterLabourRateDTO.PageNumber ?? 1;
           
            var JobTypeList = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(1, CompanyId);
            ViewBag.JobTypes = JobTypeList.Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();

            var data = await _apiClient.GetAllLabourRatesAsync(oFilterLabourRateDTO);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_LabourRateList", data);
            }
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> Find(int Id)
        {
            try
            {
                var oLabourRateDTO = new LabourRateDTO();
                oLabourRateDTO = await _apiClient.GetLabourRateByIdAsync(Id);
                return PartialView("_Edit", oLabourRateDTO);
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLabourRateDTO dto)
        {

            int? result;
            try
            {
                if (dto == null)
                {
                    return Json(new { success = false });
                }

                dto.CreatedBy = UserId; // need fix it later
                result = await _apiClient.AddLabourRateAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] UpdateLabourRateDTO dto)
        {
          
            int? result;
            try
            {
                if (dto == null || dto.Id <= 0)
                {
                    return Json(false);
                }

                dto.UpdatedBy = UserId;
                result = await _apiClient.UpdateLabourRateAsync(dto);
                return Json(true);
            }
            catch
            {
                return Json(false);
            }

        }

        [HttpPost]
        public async Task<ActionResult> DeleteLookup(int Id)
        {
            try
            {
                DeleteLabourRateDTO dto = new DeleteLabourRateDTO();
                dto.Id = Id;

                var result = await _apiClient.DeleteLabourRateAsync(dto);
                if (result)
                {
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return Json(new { success = false, message = "An error occurred while deleting the lookup." });
            }
        }

       
    }
}
