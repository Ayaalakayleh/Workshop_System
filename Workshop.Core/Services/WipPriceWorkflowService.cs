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
    public class WipPriceWorkflowService : IWipPriceWorkflowService
    {
        private readonly IWipPriceWorkflowRepository _repository;
        public WipPriceWorkflowService(IWipPriceWorkflowRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<WipItemPriceWorkflowDTO>> WIP_Items_GetPriceWorkflow(int WIPId)
        {
            return await _repository.WIP_Items_GetPriceWorkflow(WIPId);
        }
        public async Task<int> WIP_Items_SetPriceWorkflowPending(ApplyWipPriceWorkflowResult applyWipPriceWorkflowResult)
        {
            return await _repository.WIP_Items_SetPriceWorkflowPending(applyWipPriceWorkflowResult);
        }
        public async Task<bool> WIP_Items_FinishPriceWorkflow(FinishWipPriceWorkflowRequest finishWipPriceWorkflowRequest)
        {
            return await _repository.WIP_Items_FinishPriceWorkflow(finishWipPriceWorkflowRequest);
        }
    }
}
