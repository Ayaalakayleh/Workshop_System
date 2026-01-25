using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class MonthlyRepairCostBranchWiseReportViewModel
    {
        public MonthlyRepairCostReportFilterDTO Filter { get; set; }
        public IEnumerable<MonthlyRepairCostBranchWiseReportDTO> ReportData { get; set; }
    }
}
