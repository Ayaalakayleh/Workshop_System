using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class PriceMatrixModel
    {

        public List<GetPriceMatrixDTO>? PriceMatrixList { get; set; }

        public PriceMatrixFilter? PriceMatrixFilter { get; set; }

        public int TotalPages { get; set; } = 0;

    }
}
