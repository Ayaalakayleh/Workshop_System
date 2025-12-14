using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class LookupRepository : ILookupRepository
    {
        private readonly Database _database;
        public LookupRepository(Database database)
        {
            _database = database;
        }

        public async Task<IEnumerable<LookupDetailsDTO>> GetAllDetailsAsync()
        {
            return await _database.ExecuteGetAllStoredProcedure<LookupDetailsDTO>("LookupDetails_GetAll", null);
        }

        public async Task<IEnumerable<LookupDetailsDTO>> GetDetailsByHeaderIdAsync(int headerId, int CompanyId)
        {
            var parameters = new { HeaderId = headerId, CompanyId = CompanyId };
            return await _database.ExecuteGetAllStoredProcedure<LookupDetailsDTO>("LookupDetails_GetByHeaderId", parameters);
        }

        public async Task<LookupDetailsDTO?> GetDetailsByIdAsync(int id, int headerId, int CompanyId)
        {
            var parameters = new { 
                LookupId = id, 
                HeaderId = headerId,
                CompanyId = CompanyId
            };
            return await _database.ExecuteGetByIdProcedure<LookupDetailsDTO>("LookupDetails_GetById", parameters);
        }

        public async Task<int> AddDetailsAsync(CreateLookupDetailsDTO dto)
        {
            var parameters = new
            {
                HeaderId = dto.HeaderId,
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                CreatedBy = dto.CreatedBy,
                Code = dto.Code,
                CompanyId = dto.CompanyId,
            };
            return await _database.ExecuteAddStoredProcedure<int>("LookupDetails_Insert", parameters);
        }

        public async Task<int> UpdateDetailsAsync(UpdateLookupDetailsDTO dto)
        {
            var parameters = new
            {
                Id = dto.Id,
                HeaderId = dto.HeaderId,
                Code = dto.Code,
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                CompanyId = dto.CompanyId,
                ModifiedBy = dto.ModifiedBy
            };
            return await _database.ExecuteUpdateProcedure<int>("LookupDetails_Update", parameters);
        }

        public async Task<int> DeleteDetailsAsync(DeleteLookupDetailsDTO dto)
        {
            var parameters = new { LookupId = dto.Id };
            return await _database.ExecuteDeleteProcedure<int>("D_Lookup_Delete", parameters);
        }
        public async Task<IEnumerable<LookupHeaderDTO>> GetAllHeadersAsync(int CompanyId)
        {
            var parameters = new { CompanyId = CompanyId };
            return await _database.ExecuteGetAllStoredProcedure<LookupHeaderDTO>("M_LookupHeader_Get", parameters);
        }

        public async Task<LookupHeaderDTO?> GetHeaderByIdAsync(int id, int CompanyId)
        {
            var parameters = new 
            {
                Id = id,
                CompanyId = CompanyId
            };
            return await _database.ExecuteGetByIdProcedure<LookupHeaderDTO>("M_LookupHeader_GetById", parameters);
        }

    }


}
