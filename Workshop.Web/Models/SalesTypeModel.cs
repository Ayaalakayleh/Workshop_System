using Microsoft.AspNetCore.Mvc.Rendering;

namespace Workshop.Web.Models
{
    public class SalesTypeModel
    {
        public List<SelectListItem> SalesType { get; set; }
        public List<SelectListItem> PartialSalesType { get; set; }
    }
}
