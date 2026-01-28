using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class WIPReportViewModel
    {
        public WIPReportFilterDTO Filter { get; set; }
        public List<WIPReportDTO> ReportData { get; set; }
    }
}
