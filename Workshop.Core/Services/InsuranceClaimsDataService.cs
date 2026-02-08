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
    internal class InsuranceClaimsDataService : IInsuranceClaimsDataService
    {
        private readonly IInsuranceClaimsDataRepository _repository;

        public InsuranceClaimsDataService(IInsuranceClaimsDataRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InsuranceClaimsDataDTO>> InsuranceClaimsDataDataAsync(InsuranceClaimsDataFilterDTO filterDTO)
        {
            return await _repository.InsuranceClaimsDataDataAsync(filterDTO);
        }
    }
}
