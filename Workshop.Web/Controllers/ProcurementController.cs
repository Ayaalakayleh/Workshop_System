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

        [CustomAuthorize(Permissions.Procurement.InternalPurchaseRequest)]
        public ActionResult InternalPurchaseRequestEdit(int? Id = null, int? FromWhere = null, int? itemId = null, int? wipId = null, decimal? qty = null)
        {
            ViewBag.HostName = _configuration["ApiSettings:SalesAndProcurementUrl"];

            var query = new List<string>();

            if (Id.HasValue)
                query.Add($"Id={Id.Value}");

            if (FromWhere.HasValue)
                query.Add($"FromWhere={FromWhere.Value}");

            if (itemId.HasValue)
                query.Add($"itemId={itemId.Value}");

            if (wipId.HasValue)
                query.Add($"wipId={wipId.Value}");

            if (qty.HasValue)
                query.Add($"qty={qty.Value}");

            var queryString = query.Any() ? "?" + string.Join("&", query) : "";

            ViewBag.TargetUrl = $"{ViewBag.HostName}/InternalPurchaseRequest/Edit{queryString}";

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
