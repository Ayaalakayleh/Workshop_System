using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class WorkOrderModel
    {
        public List<MWorkOrderDTO>? WorkOrderDTOs { get; set; }
        public WorkOrderFilterDTO? WorkOrderFilter { get; set; }
        public MWorkOrderDTO? WorkOrderForm { get; set; }

    }

    public class PriceMatrixRequest
    {
        public List<int> MatchValue { get; set; } = new();
        public int BasisId { get; set; }
    }
}
