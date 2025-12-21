using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Web.Controllers;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Controllers
{
    [SessionTimeout]
    public class WorkshopDefinitionsController : BaseController
    {
        private readonly WorkshopApiClient _workshopApiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly string lang;
        public WorkshopDefinitionsController(
            WorkshopApiClient workshopApiClient,
            ERPApiClient erpApiClient,
            AccountingApiClient accountingApiClient,
            VehicleApiClient vehicleApiClient,
            IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _erpApiClient = erpApiClient;
            _workshopApiClient = workshopApiClient;
            _accountingApiClient = accountingApiClient;
            _vehicleApiClient = vehicleApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.WorkshopDefinitions.View)]
        public async Task<IActionResult> Index([FromQuery] WorkShopFilterDTO filter)
        {


            //Fill dropdowns
            var Parents = await _workshopApiClient.GetAllSimpleParentsWorkshop(CompanyId);
            //Parents.Insert(0, new ParentWorkshopSimpleDTO() { ParentId = 0, PrimaryName = "Main Workshops", SecondaryName = "الورشات الرئيسية" });
            ViewBag.ColCity = await _erpApiClient.GetCities(null, lang); // cities
            ViewBag.ParentWorkshops = Parents?.Select(r => new SelectListItem { Text = lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();


            //Filter
            filter ??= new WorkShopFilterDTO();
            filter.CompanyId = CompanyId;


            //Get Data
            var workshops = await _workshopApiClient.WorkshopGetAllPageAsync(filter);

            if (workshops != null && workshops.Any())
            {
                foreach (var workshop in workshops)
                {
                    workshop.ParentName = lang == "en" ? workshop.ParentPrimaryName : workshop.ParentSecondaryName;
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_WorkshopList", workshops);
            }

            return View(workshops);
        }



        ///// <summary>
        ///// Edit
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>

        [CustomAuthorize(Permissions.WorkshopDefinitions.Edit)]
        public async Task<IActionResult> Edit(int? Id)
        {

            WorkShopDefinitionDTO workshop = null;

            if (Id != null)
            {
                workshop = await _workshopApiClient.GetWorkshopByIdAsync((int)Id);
                workshop.SetInsuranceCompanyIds();
            }
            else
            {
                workshop = new WorkShopDefinitionDTO();
                workshop.IsActive = true;
            }

            if (Id != null && workshop == null)
            {
                return RedirectToAction("Index");
            }


            var ColTaxClassification = await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            var Parents = await _workshopApiClient.GetAllSimpleParentsWorkshop(CompanyId);

            ViewBag.ColInsuranceCompany = await _vehicleApiClient.GetInsuranceCompanySummery(CompanyId, lang);

            ViewBag.ColCity = await _erpApiClient.GetCities(null, lang); // cities
            ViewBag.SupplierList = await _accountingApiClient.M_GetSuppliers(CompanyId, BranchId, 1, lang);
            ViewBag.AccountList = AccountList.Select(r => new SelectListItem { Text = lang == "en" ? r.AccountName : r.AccountSecondaryName, Value = r.ID.ToString() }).ToList();
            ViewBag.VatClassificationList = ColTaxClassification.Select(r => new SelectListItem { Text = lang == "en" ? r.TaxClassificationName : r.TaxClassificationArabicName, Value = r.TaxClassificationNo.ToString() }).ToList();
            ViewBag.ParentWorkshops = Parents?.Select(r => new SelectListItem { Text = lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();



            return View(workshop);
        }

        /// <summary>
        /// Edit
        /// </summary>
        /// <param name="workshop"></param>
        /// <returns></returns>
        /// 

        [CustomAuthorize(Permissions.WorkshopDefinitions.Edit)]
        [HttpPost]
        public async Task<IActionResult> Edit(WorkShopDefinitionDTO workshop)
        {
            try
            {
                workshop.CompanyId = CompanyId;
                workshop.BranchId = BranchId;
                workshop.UserId = UserId;

                if (workshop.InsuranceCompanyIds != null && workshop.InsuranceCompanyIds.Length > 0)
                {
                    workshop.InsuranceCompany = string.Join(",", workshop.InsuranceCompanyIds);
                }
                else
                {
                    workshop.InsuranceCompany = null;
                }


                bool isSuccess = false;

                if (workshop.Id != null)
                {
                    var data = await _workshopApiClient.GetWorkshopByIdAsync(workshop.Id.Value);

                    if (data != null)
                    {

                        var updateWorkshop = new UpdateWorkShopDTO();
                        updateWorkshop.MapDefinitionToUpdateDto(workshop);
                        updateWorkshop.UpdatedBy = UserId;// SessionManager.GetSessionUserInfo.UserID;

                        var res = await _workshopApiClient.UpdateAsync(updateWorkshop);
                        isSuccess = res;
                    }
                    else
                    {
                        isSuccess = false;
                    }
                }
                else
                {
                    var createWorkshop = new CreateWorkShopDTO();
                    createWorkshop.MapToCreateDto(workshop);

                    var res = await _workshopApiClient.AddAsync(createWorkshop);
                    isSuccess = res.HasValue && res.Value > 0;
                }


                //ToDo
                //ClearCacheByName(CacheKeys.ExternalWorkshop);

                return Json(new { isSuccess });
            }
            catch
            {
                
                return Json(new { isSuccess = false });
            }

        }


    }
}