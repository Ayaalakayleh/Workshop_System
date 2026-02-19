using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Workshop.Core.DTOs;
using Workshop.Domain.Enum;
using Workshop.Resources;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    public class PriceWorkflowController : BaseController
    {

        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly ILogger<WIPController> _logger;
        private readonly IStringLocalizer<Common> _common;
        public readonly string lang;

        public PriceWorkflowController(WorkshopApiClient apiClient,ERPApiClient erpApiClient,IConfiguration configuration,IWebHostEnvironment env,IStringLocalizer<Common> common,ILogger<WIPController> logger
                , IMemoryCache cache) : base(cache, configuration, env)
        {
            _apiClient = apiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            _erpApiClient = erpApiClient;
            _logger = logger;
            _common = common;
        }

        [CustomAuthorize(Permissions.PriceWorkflow.View)]
        public async Task<IActionResult> Index()
        {
            PriceWorkflowDTO dto = new PriceWorkflowDTO
            {
                CompanyId = CompanyId,
                BranchId = BranchId
            };
            ViewBag.Data = await _apiClient.GetPriceWorkflowDefinitionAsync(dto);
            ViewBag.Workflows = Enum.GetValues(typeof(WorkflowEnum))
                .Cast<WorkflowEnum>()
                .Where(e => (int)e >= 18)
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                })
                .ToList();

            return View();
        }

        [HttpPost]
        [CustomAuthorize(Permissions.PriceWorkflow.Create)]
        public async Task<IActionResult> Edit([FromBody] List<PriceWorkflowDTO> data)
        {
            try
            {
                if (data == null || data.Count == 0)
                    return Json(new { success = false, errorMessage = "No data received" });

                var userId = UserId;
                var now = DateTime.Now; 

                foreach (var item in data)
                {
                    if (item.Id == 0)
                    {
                        item.CreatedBy = userId;

                         var entity = new PriceWorkflowDTO
                         {
                             Price = item.Price,
                             KeyId = item.KeyId,
                             WorkflowID = item.WorkflowID,
                             BranchId = BranchId,
                             CompanyId = CompanyId,
                             CreatedBy = userId
                         };
                        // _db.PriceWorkflows.Add(entity);
                        await _apiClient.AddPriceWorkflowDefinitionAsync(entity);
                    }
                    else
                    {

                        var entity = await _apiClient.GetPriceWorkflowByIdAsync(item.Id);
                        if (entity == null) continue;

                        entity.Id = item.Id;
                        entity.Price = item.Price;
                        entity.WorkflowID = item.WorkflowID;
                        entity.BranchId = BranchId;
                        entity.CompanyId = CompanyId;
                        entity.UpdatedBy = userId;
                    
                        await _apiClient.UpdatePriceWorkflowDefinitionAsync(entity);
                    }
                }

                // await _db.SaveChangesAsync();

                return Json(new { success = true });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        [CustomAuthorize(Permissions.PriceWorkflow.Delete)]
        public async Task<JsonResult> Delete([FromBody] int Id)
        {
            var response = await _apiClient.DeletePriceWorkflowDefinitionAsync(Id);
            return Json(new { success = response });
            
        }
    }
}
