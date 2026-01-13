using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class PriceWorkflowRepository : IPriceWorkflowRepository
    {
  
        private readonly Database _database;
        public PriceWorkflowRepository(Database database)
        {
            _database = database;
        }

        public async Task<IEnumerable<PriceWorkflowDTO>> GetAsync(PriceWorkflowDTO dto)
        {
            var parameters = new
            {
                BranchId = dto.BranchId,
                CompanyId = dto.CompanyId
            };
            return await _database.ExecuteGetAllStoredProcedure<PriceWorkflowDTO>("PriceWorkflow_Get", parameters);
        }

        public async Task<PriceWorkflowDTO> GetByIdAsync(int Id)
        {
            var parameters = new
            {
                Id = Id
            };
            return await _database.ExecuteGetByIdProcedure<PriceWorkflowDTO>("PriceWorkflow_GetById", parameters);
        }

        public async Task<int> AddAsync(PriceWorkflowDTO dto)
        {
            var parameters = new
            {
                Price = dto.Price,
                KeyId = dto.KeyId,
                WorkflowID = dto.WorkflowID,
                BranchId = dto.BranchId,
                CompanyId = dto.CompanyId,
                CreatedBy = dto.CreatedBy
            };

            return await _database.ExecuteAddStoredProcedure<int>("PriceWorkflow_Insert", parameters);
        }

        public async Task<int> UpdateAsync(PriceWorkflowDTO dto)
        {
            var parameters = new
            {
                Id = dto.Id,
                Price = dto.Price,
                WorkflowID = dto.WorkflowID,
                BranchId = dto.BranchId,
                CompanyId = dto.CompanyId,
                UpdatedBy = dto.UpdatedBy
            };

             var result = await _database.ExecuteUpdateProcedure<int>("PriceWorkflow_Update", parameters);
            return result;
        }

        public async Task<int> DeleteAsync(int Id)
        {
            var parameters = new
            {
                Id = Id
            };

             var result = await _database.ExecuteDeleteProcedure<int>("PriceWorkflow_Delete", parameters);
            return result;
        }

    }
}
