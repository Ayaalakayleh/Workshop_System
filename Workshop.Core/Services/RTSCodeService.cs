using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class RTSCodeService : IRTSCodeService
    {
        private readonly IRTSCodeRepository _repository;

        public RTSCodeService(IRTSCodeRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<RTSCodeDTO>> GetAllAsync(string? Name, string? Code, int PageNumber = 0)
        {
            return await _repository.GetAllAsync(Name, Code, PageNumber);
        }

        public async Task<IEnumerable<RTSCodeDTO>> GetAllDDLAsync()
        {
            return await _repository.GetAllDDLAsync();
        }

        public async Task<RTSCodeDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<int> AddAsync(CreateRTSCodeDTO dto)
        {
            return await _repository.AddAsync(dto);
        }

        public async Task<int> UpdateAsync(UpdateRTSCodeDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }
        public async Task<int> DeleteAsync(DeleteRTSCodeDTO dto)
        {
            return await _repository.DeleteAsync(dto);
        }


    }

}