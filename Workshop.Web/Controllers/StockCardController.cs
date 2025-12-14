using Microsoft.AspNetCore.Mvc;
using Workshop.Web.Models;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class StockCardController : Controller
    {
        private readonly IConfiguration _configuration;

        public StockCardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [CustomAuthorize(Permissions.StockCard.View)]
        public IActionResult Index()
        {
            ViewBag.HostName = _configuration["ApiSettings:InventoryUrl"];

            return View();
        }
    }
}
