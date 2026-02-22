using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Enum
{
    public enum PriceWorkflowStatusEnum
    {
        None = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Failed = 9
    }
}
