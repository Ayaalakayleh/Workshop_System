using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Workshop.Core.DTOs;
using Workshop.Infrastructure;
using Workshop.Web.Models;
using Workshop.Web.Services;
namespace Workshop.Web.Controllers
{
    [SessionTimeout]

    public class LookupManagementController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public readonly string lang;
        public LookupManagementController(WorkshopApiClient apiClient, ERPApiClient erpApiClient,
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            _configuration = configuration;
            _env = env;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;

        }

        [CustomAuthorize(Permissions.LookupManagement.View)]
        public async Task<IActionResult> Index()
        {
            ViewBag.lookupHeaders = await _apiClient.GetAllLookupHeadersAsync(lang, CompanyId);
            ViewBag.HeaderId = TempData["CategoryId"] == null ? 0 : int.Parse(TempData["CategoryId"].ToString());
            return View();
        }

        public async Task<IActionResult> GetDetailsByCategory(int id)
        {
            var Details = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(id, CompanyId);
            return PartialView("_LookupDetailsList", Details);
        }

        [HttpGet("Lookup_Find")]
        [CustomAuthorize(Permissions.LookupManagement.View)]
        public async Task<IActionResult> Lookup_Find(int LookupId, int headerId)
        {
            try
            {
                LookupDetailsDTO dto = new LookupDetailsDTO();
                dto = await _apiClient.GetLookupDetailByIdAsync(LookupId, headerId, CompanyId);
                return PartialView("_EditLookup", dto);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred while fetching lookup details." });
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.LookupManagement.Create)]
        public async Task<IActionResult> LookupCreate(CreateLookupDetailsDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return Json(new { success = false, message = "Invalid lookup details." });
                }

                dto.CreatedBy = UserId; 
                dto.CompanyId = CompanyId; 
                int result = await _apiClient.AddLookupDetailAsync(dto)??0;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.LookupManagement.Create)]
        public async Task<IActionResult> LookupUpdate([FromBody] UpdateLookupDetailsDTO dto)
        {
            try
            {
                TempData["CategoryId"] = dto.HeaderId;
                if (dto == null || dto.Id <= 0)
                {
                    return Json(false);
                }

                dto.ModifiedBy = UserId;
                dto.CompanyId = CompanyId;
                int result = await _apiClient.UpdateLookupDetailAsync(dto) ?? 0;
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
                DeleteLookupDetailsDTO dto = new DeleteLookupDetailsDTO();
                dto.Id = Id;
               
                var result = await _apiClient.DeleteLookupDetailAsync(dto);
                if (result)
                {
                    return Json( true );
                }
                else
                {
                    return Json( false );
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
