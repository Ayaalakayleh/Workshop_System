using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class AccountDefinitionRepository : IAccountDefinitionRepository
    {
        private readonly Database _database;
        public AccountDefinitionRepository(Database database)
        {
            _database = database;
        }
        public async Task<AccountDefinitionDTO?> GetAsync(int CompanyId)
        {

            try
            {
                var parameters = new { CompanyId };
                var result = await _database.ExecuteGetByIdProcedure<AccountDefinitionDTO>("AccountDefinition_Get", parameters);

                if (result == null || result.Id == 0)
                    return null;

                return result;
            }
            catch
            {
                return null;
            }
        }

        public async Task<int> AddAsync(AccountDefinitionDTO dto)
        {
            var parameters = new
            {
                JournalId = dto.JournalId,
                WIPAccountId = dto.WIPAccountId,
                MaintenanceAccountId = dto.MaintenanceAccountId,
                AccessoriesAccountId = dto.AccessoriesAccountId,
                AccidentAccountId = dto.AccidentAccountId,
                MaintenanceProjectsAccountId = dto.MaintenanceProjectsAccountId,
                InternalCostPartId = dto.InternalCostPartId,
                InternalCostLabourId = dto.InternalCostLabourId,
                ExternalCostPartId = dto.ExternalCostPartId,
                ExternalCostLabourId = dto.ExternalCostLabourId,
                InternalRevenuePartId = dto.InternalRevenuePartId,
                InternalRevenueLabourId = dto.InternalRevenueLabourId,
                InvoiceTypeId = dto.InvoiceTypeId,
                CompanyId = dto.CompanyId,
                CreatedBy = dto.CreatedBy
            };

            return await _database.ExecuteAddStoredProcedure<int>("AccountDefinition_Insert", parameters);
        }

        public async Task<int> UpdateAsync(AccountDefinitionDTO dto)
        {
            var parameters = new
            {
                Id = dto.Id,
                JournalId = dto.JournalId,
                WIPAccountId = dto.WIPAccountId,
                MaintenanceAccountId = dto.MaintenanceAccountId,
                AccessoriesAccountId = dto.AccessoriesAccountId,
                AccidentAccountId = dto.AccidentAccountId,
                MaintenanceProjectsAccountId = dto.MaintenanceProjectsAccountId,
                InternalCostPartId = dto.InternalCostPartId,
                InternalCostLabourId = dto.InternalCostLabourId,
                ExternalCostPartId = dto.ExternalCostPartId,
                ExternalCostLabourId = dto.ExternalCostLabourId,
                InternalRevenuePartId = dto.InternalRevenuePartId,
                InternalRevenueLabourId = dto.InternalRevenueLabourId,
                InvoiceTypeId = dto.InvoiceTypeId,
                UpdatedBy = dto.UpdatedBy
            };

            return await _database.ExecuteUpdateProcedure<int>("AccountDefinition_Update", parameters);
        }

    }
}
