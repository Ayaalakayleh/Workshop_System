using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IReportsService
    {
        Task<IEnumerable<MonthlyRepairCostReportDTO>> GetMonthlyRepairCostReport(MonthlyRepairCostReportFilterDTO filterDTO);
        Task<IEnumerable<MonthlyRepairCostBranchWiseReportDTO>> GetMonthlyRepairCostBranchWiseReport(MonthlyRepairCostReportFilterDTO filterDTO);
        Task<IEnumerable<ConsumptionReportDTO>> GetConsumptionReport(ConsumptionReportFilterDTO filterDTO);
        Task<IEnumerable<WIPReportDTO>> GetWIPReport(WIPReportFilterDTO filterDTO);
        Task<IEnumerable<PartsSummaryDTO>> GetPartsSummaryReport(PartsSummaryFilterDTO filterDTO);
    }
}
