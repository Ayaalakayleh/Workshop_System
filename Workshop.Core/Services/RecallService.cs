using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
	public class RecallService : IRecallService
	{
		private readonly IRecallRepository _repository;

		public RecallService(IRecallRepository repository)
		{
			_repository = repository;
		}

		public async Task<int> AddAsync(CreateRecallDTO dto)
		{
			return await _repository.AddAsync(dto);
		}

		public async Task<IEnumerable<RecallDTO>> GetAllAsync(FilterRecallDTO filterRecallDTO)
		{
			return await _repository.GetAllAsync(filterRecallDTO);
		}

		public async Task<RecallDTO?> GetByIdAsync(int id)
		{
			return await _repository.GetByIdAsync(id);
		}
        public async Task<ActiveRecallsByChassisResponseDto> GetActiveRecallsByChassisAsync(string chassisNo)
        {
            return await _repository.GetActiveRecallsByChassisAsync(chassisNo);
        }
        public async Task<int> UpdateAsync(UpdateRecallDTO dto)
		{
			return await _repository.UpdateAsync(dto);
		}
        public async Task<int> DeleteAsync(DeleteRecallDTO dto)
		{
			return await _repository.DeleteAsync(dto);
		}


    }
}