using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class WIPServiceHistoryDetailsModel
    {
        public IEnumerable<WIPServiceHistoryDetails_Labour?> LabourDetail { get; set; }
        public IEnumerable<WIPServiceHistoryDetails_Parts?> PartDetail { get; set; }
    }
}
