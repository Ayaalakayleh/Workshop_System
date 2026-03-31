using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class ReportsService : IReportsService
    {
        private readonly IReportsRepository _repository;
        public ReportsService(IReportsRepository repository)
        {
            _repository = repository;
        }
        public async Task<IEnumerable<MonthlyRepairCostReportDTO>> GetMonthlyRepairCostReport(MonthlyRepairCostReportFilterDTO filterDTO)
        {
            return await _repository.GetMonthlyRepairCostReport(filterDTO);
        }
        public async Task<IEnumerable<MonthlyRepairCostBranchWiseReportDTO>> GetMonthlyRepairCostBranchWiseReport(MonthlyRepairCostReportFilterDTO filterDTO)
        {
            return await _repository.GetMonthlyRepairCostBranchWiseReport(filterDTO);
        }
        public async Task<IEnumerable<ConsumptionReportDTO>> GetConsumptionReport(ConsumptionReportFilterDTO filterDTO)
        {
            return await _repository.GetConsumptionReport(filterDTO);
        }

        public async Task<IEnumerable<WIPReportDTO>> GetWIPReport(WIPReportFilterDTO filterDTO)
        {
            return await _repository.GetWIPReport(filterDTO);
        }
        public async Task<IEnumerable<PartsSummaryDTO>> GetPartsSummaryReport(PartsSummaryFilterDTO filterDTO)
        {
            return await _repository.GetPartsSummaryReport(filterDTO);
        }   
       
    }
}

