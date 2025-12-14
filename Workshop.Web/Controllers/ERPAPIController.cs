using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs.General;
using Workshop.Web.Controllers;
using Workshop.Web.Services;
namespace Workshop.Controllers
{
    public class ERPAPIController : BaseController
    {

        private readonly ERPApiClient _eRPApiClient;
        private readonly string lang;

        public ERPAPIController(ERPApiClient eRPApiClient, IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _eRPApiClient = eRPApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }


        // GET: ERPAPI
        [HttpGet]
        public async Task<IActionResult> Branches(string search, bool currentBranchOnly = false, int id = 0)
        {
            int comapnyId = CompanyId;  //((CompanyInfo)Session["CompanyInfo"]).Id;

            var ColBranches = new List<CompanyBranch>();
            ColBranches = await _eRPApiClient.GetActiveBranchesByCompanyId(comapnyId, lang);
            return Json(ColBranches);
        }

        //[HttpGet]
        //public async Task<IActionResult> Users()
        //{
        //    string language = LanguageController.GetCurrentLanguage();
        //    var colUsers = new List<User>();
        //    int comapnyId = ((CompanyInfo)Session["CompanyInfo"]).Id;
        //    colUsers = ERPAPI.Get_UsersByCompanyId(comapnyId);

        //    return Json(colUsers, JsonRequestBehavior.AllowGet);
        //}
    }
}