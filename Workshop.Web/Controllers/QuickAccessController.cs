using Microsoft.AspNetCore.Mvc;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class QuickAccessController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpApiClient;
        public readonly string lang;

        public QuickAccessController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, 
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.QuickAccess.View)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
