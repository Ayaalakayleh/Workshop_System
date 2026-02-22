using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IWipPriceWorkflowRepository
    {
        Task<IEnumerable<WipItemPriceWorkflowDTO>> WIP_Items_GetPriceWorkflow(int WIPId);
        Task<int> WIP_Items_SetPriceWorkflowPending(ApplyWipPriceWorkflowResult applyWipPriceWorkflowResult);
        Task<bool> WIP_Items_FinishPriceWorkflow(FinishWipPriceWorkflowRequest finishWipPriceWorkflowRequest);
    }
}
