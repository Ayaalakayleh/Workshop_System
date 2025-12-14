using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class RecallModel
    {

        public IEnumerable<RecallDTO> Recalls { get; set; }
        public FilterRecallDTO RecallFilter { get; set; }
    }
}
