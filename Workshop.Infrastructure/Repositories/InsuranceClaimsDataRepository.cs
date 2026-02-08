using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Dapper;
using System.Data;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class InsuranceClaimsDataRepository : IInsuranceClaimsDataRepository
    {
        private readonly DapperContext _context;
        public InsuranceClaimsDataRepository(DapperContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<InsuranceClaimsDataDTO>> InsuranceClaimsDataDataAsync(InsuranceClaimsDataFilterDTO filterDTO)
        {
            using var connection = _context.CreateConnection();

            var parameters = new DynamicParameters();
            parameters.Add("@CompanyId", filterDTO.CompanyId);
            parameters.Add("@BranchId", filterDTO.BranchId);
            parameters.Add("@ClaimStatus", filterDTO.ClaimStatus);
            parameters.Add("@WorkOrderId", filterDTO.WorkOrderId);

            var result = await connection.QueryAsync<InsuranceClaimsDataDTO>(
            "WorkOrder.GetInsuranceClaimsData",
            parameters,
            commandType: CommandType.StoredProcedure
            );

            return result;
        }

   
    }
}
