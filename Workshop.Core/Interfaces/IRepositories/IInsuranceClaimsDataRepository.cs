using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IInsuranceClaimsDataRepository
    {
        Task<IEnumerable<InsuranceClaimsDataDTO>> InsuranceClaimsDataDataAsync(InsuranceClaimsDataFilterDTO filterDTO);
    }
}
