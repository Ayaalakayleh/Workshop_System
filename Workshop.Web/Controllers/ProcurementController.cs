using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Web.Models;

namespace Workshop.Web.Controllers
{
    public class ProcurementController : BaseController
    {

        public ProcurementController(IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
        }

        [CustomAuthorize(Permissions.Procurement.RFQ)]
        public ActionResult RFQ()
        {
            ViewBag.HostName = _configuration["ApiSettings:SalesAndProcurementUrl"];
            return View();
        }

        [CustomAuthorize(Permissions.Procurement.InternalPurchaseRequest)]
        public ActionResult InternalPurchaseRequest()
        {
            ViewBag.HostName = _configuration["ApiSettings:SalesAndProcurementUrl"];
            return View();
        }

        [CustomAuthorize(Permissions.Procurement.PurchaseOrder)]
        public ActionResult PurchaseOrder()
        {
            ViewBag.HostName = _configuration["ApiSettings:SalesAndProcurementUrl"];
            return View();
        }

        [CustomAuthorize(Permissions.Procurement.PurchaseInvoice)]
        public ActionResult PurchaseInvoice()
        {
            ViewBag.HostName = _configuration["ApiSettings:SalesAndProcurementUrl"];
            return View();
        }

    }
}
