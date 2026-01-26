using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IReportsRepository
    {
        Task<IEnumerable<MonthlyRepairCostReportDTO>> GetMonthlyRepairCostReport(MonthlyRepairCostReportFilterDTO filterDTO);
        Task<IEnumerable<MonthlyRepairCostBranchWiseReportDTO>> GetMonthlyRepairCostBranchWiseReport(MonthlyRepairCostReportFilterDTO filterDTO);
        Task<IEnumerable<ConsumptionReportDTO>> GetConsumptionReport(ConsumptionReportFilterDTO filterDTO);

    }
}
