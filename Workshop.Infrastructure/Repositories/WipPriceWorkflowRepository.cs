using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class WipPriceWorkflowRepository : IWipPriceWorkflowRepository
    {

        private readonly Database _database;
        private readonly DapperContext _context;
        public WipPriceWorkflowRepository(Database database, DapperContext context)
        {
            _database = database;
            _context = context;
        }


        public async Task<IEnumerable<WipItemPriceWorkflowDTO>> WIP_Items_GetPriceWorkflow(int WIPId)
        {
            var parameters = new
            {
                WIPId = WIPId
            };

            return await _database.ExecuteGetAllStoredProcedure<WipItemPriceWorkflowDTO>("WIP_Items_GetPriceWorkflow", parameters);
        }
        public async Task<int> WIP_Items_SetPriceWorkflowPending(ApplyWipPriceWorkflowResult applyWipPriceWorkflowResult)
        {
            var parameters = new
            {
                WipItemId = applyWipPriceWorkflowResult.WipItemId,
                MasterId = applyWipPriceWorkflowResult.MasterId,
                WorkflowEnumId = applyWipPriceWorkflowResult.WorkflowEnumId,
                UserId = applyWipPriceWorkflowResult.UserId
            };

            return await _database.ExecuteGetByIdProcedure<int>("WIP_Items_SetPriceWorkflowPending", parameters);
        }
        public async Task<bool> WIP_Items_FinishPriceWorkflow(FinishWipPriceWorkflowRequest finishWipPriceWorkflowRequest)
        {
            var parameters = new
            {
                WipItemId = finishWipPriceWorkflowRequest.WipItemId,
                MasterId = finishWipPriceWorkflowRequest.MasterId,
                Status = finishWipPriceWorkflowRequest.Status,
                Reason = finishWipPriceWorkflowRequest.Reason,
                UserId = finishWipPriceWorkflowRequest.UserId
            };

            return await _database.ExecuteGetByIdProcedure<bool>("WIP_Items_FinishPriceWorkflow", parameters);
        }
    }
}
