using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Web.Models;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]

    public class PettyCashController : BaseController
    {

        public PettyCashController(IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
        }

        [CustomAuthorize(Permissions.PettyCash.Expenses)]

        public ActionResult Index()
        {
            ViewBag.HostName = _configuration["ApiSettings:AccountingUrl"];
            return View();
        }
        [CustomAuthorize(Permissions.PettyCash.History)]

        public ActionResult History()
        {
            ViewBag.HostName = _configuration["ApiSettings:AccountingUrl"];
            return View();
        }
        [CustomAuthorize(Permissions.PettyCash.Request)]

        public ActionResult PettyCashRequest()
        {
            ViewBag.HostName = _configuration["ApiSettings:AccountingUrl"];
            return View();
        }
        [CustomAuthorize(Permissions.PettyCash.Close)]

        public ActionResult PettyCashClose()
        {
            ViewBag.HostName = _configuration["ApiSettings:AccountingUrl"];
            return View();
        }
    }
}
