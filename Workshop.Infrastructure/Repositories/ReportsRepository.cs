using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class ReportsRepository : IReportsRepository
    {
        private readonly DapperContext _context;
        public ReportsRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MonthlyRepairCostReportDTO>> GetMonthlyRepairCostReport(MonthlyRepairCostReportFilterDTO filterDTO)
        {


            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@WIPId", filterDTO.WIP);
            parameters.Add("@InvoiceDateStart", filterDTO.InvoiceDateStart);
            parameters.Add("@InvoiceDateEND", filterDTO.InvoiceDateEnd);
            parameters.Add("@CustomerId", filterDTO.CustomerId);

            var result = await connection.QueryAsync<MonthlyRepairCostReportDTO>(
            "R_MonthlyRepairCost",
            parameters,
            commandType: CommandType.StoredProcedure
            );

            return result;
        }
        public async Task<IEnumerable<MonthlyRepairCostBranchWiseReportDTO>> GetMonthlyRepairCostBranchWiseReport(MonthlyRepairCostReportFilterDTO filterDTO)
        {


            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@WIPId", filterDTO.WIP);
            parameters.Add("@InvoiceDateStart", filterDTO.InvoiceDateStart);
            parameters.Add("@InvoiceDateEND", filterDTO.InvoiceDateEnd);
            parameters.Add("@CustomerId", filterDTO.CustomerId);

            var result = await connection.QueryAsync<MonthlyRepairCostBranchWiseReportDTO>(
            "R_MonthlyRepairCostBranchWise",
            parameters,
            commandType: CommandType.StoredProcedure
            );

            return result;
        }


        public async Task<IEnumerable<ConsumptionReportDTO>> GetConsumptionReport(ConsumptionReportFilterDTO filterDTO)
        {

            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@TypeId", filterDTO.TypeId);
            parameters.Add("@WIPId", filterDTO.WIP);
            parameters.Add("@InvoiceDateStart", filterDTO.InvoiceDateStart);
            parameters.Add("@InvoiceDateEND", filterDTO.InvoiceDateEnd);
            parameters.Add("@CustomerId", filterDTO.CustomerId);

            var result = await connection.QueryAsync<ConsumptionReportDTO>(
            "R_Consumption",
            parameters,
            commandType: CommandType.StoredProcedure
            );

            return result;
        }


    }
}
