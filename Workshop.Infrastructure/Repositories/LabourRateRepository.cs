using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class LabourRateRepository : ILabourRateRepository
    {
        private readonly Database _database;
        public LabourRateRepository(Database database)
        {
            _database = database;
        }

        public async Task<IEnumerable<LabourRateDTO>> GetAllAsync(string? Name = null, int? PageNumber = 0)
        {
            var parameters = new
            {
                Name,
                PageNumber
            };
            return await _database.ExecuteGetAllStoredProcedure<LabourRateDTO>("LabourRate_GetAll", parameters);
        }

        public async Task<LabourRateDTO?> GetByIdAsync(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetByIdProcedure<LabourRateDTO>("LabourRate_GetById", parameters);
        }
        //public async Task<LabourRateDTO?> GetByGroupIdAsync(int id)
        //{
        //    var parameters = new { Id = id };
        //    return await _database.ExecuteGetByIdProcedure<LabourRateDTO>("LabourRate_GetByGroupId", parameters);
        //}

        public async Task<int> AddAsync(CreateLabourRateDTO dto)
        {
            var parameters = new
            {
                LabourCode = dto.LabourCode,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                CreatedBy = dto.CreatedBy
            };
            return await _database.ExecuteAddStoredProcedure<int>("LabourRate_Insert", parameters);
        }

        public async Task<int> UpdateAsync(UpdateLabourRateDTO dto)
        {
            var parameters = new
            {
                Id = dto.Id,
                LabourCode = dto.LabourCode,
                DescriptionEn = dto.DescriptionEn,
                DescriptionAr = dto.DescriptionAr,
                UpdatedBy = dto.UpdatedBy
            };
            return await _database.ExecuteUpdateProcedure<int>("LabourRate_Update", parameters);
        }

        public async Task<int> DeleteAsync(DeleteLabourRateDTO dto)
        {
            return await _database.ExecuteDeleteProcedure<int>("LabourRate_Delete", dto);
        }
    }
}
