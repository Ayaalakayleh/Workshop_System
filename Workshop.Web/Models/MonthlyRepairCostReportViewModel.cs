using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class MonthlyRepairCostReportViewModel
    {
        public MonthlyRepairCostReportFilterDTO Filter { get; set; }
        public List<MonthlyRepairCostReportDTO> ReportData { get; set; }
    }
}
