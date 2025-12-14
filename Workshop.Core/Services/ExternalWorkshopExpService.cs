using Workshop.Core.DTOs.ExternalWorkshopExp;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class ExternalWorkshopExpService : IExternalWorkshopExpService
    {
        private readonly IExternalWorkshopExpRepository _externalWorkshopExpRepository;

        public ExternalWorkshopExpService(
            IExternalWorkshopExpRepository externalWorkshopExpRepository)
        {
            _externalWorkshopExpRepository = externalWorkshopExpRepository;
        }

        public async Task<IEnumerable<MExternalWorkshopExpDTO>> ExternalWorkshopExpGetAsync(ExternalWorkshopExpFilterDTO filter)
        {
            try
            {
                if (filter.CompanyId <= 0)
                    throw new ArgumentException("Invalid company ID");

                return await _externalWorkshopExpRepository.ExternalWorkshopExpGetAsync(filter);
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public async Task<bool> InsertExternalWorkshopExpAsync(CreateExternalWorkshopExpDTO createDto)
        {
            try
            {
                var result = await _externalWorkshopExpRepository.ExternalWorkshopExpInsertAsync(createDto);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DExternalWorkshopExpDTO>> ExternalWorkshopExpGetDetailsByIdAsync(int headerId)
        {
            try
            {
                if (headerId <= 0)
                    throw new ArgumentException("Invalid header ID");

                return await _externalWorkshopExpRepository.ExternalWorkshopExpGetDetailsByIdAsync(headerId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<MExternalWorkshopExpDTO> ExternalWorkshopExpGetByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentException("Invalid ID");

                var result = await _externalWorkshopExpRepository.ExternalWorkshopExpGetByIdAsync(id);
                return result ?? throw new KeyNotFoundException($"External workshop expense with ID {id} not found");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> ExternalWorkshopExpDetailsUpdateAsync(List<DExternalWorkshopExpDTO> prData)
        {
            try
            {
                if (prData == null || prData.Count == 0)
                    throw new ArgumentException("PR data cannot be empty");

                var result = await _externalWorkshopExpRepository.ExternalWorkshopExpDetailsUpdateAsync(prData);
                return result > 0;
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }

        public async Task<IEnumerable<MExcelMappingDTO>> ExcelMappingGetAsync(ExcelMappingFilterDTO filter)
        {
            try
            {
                if (filter.CompanyId <= 0)
                    throw new ArgumentException("Invalid company ID");

                return await _externalWorkshopExpRepository.ExcelMappingGetAsync(filter);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> ExcelMappingInsertAsync(CreateExcelMappingDTO createDto)
        {
            try
            {
                var result = await _externalWorkshopExpRepository.ExcelMappingInsertAsync(createDto);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> ExcelMappingUpdateAsync(UpdateExcelMappingDTO updateDto)
        {
            try
            {
                if (updateDto.Id <= 0)
                    throw new ArgumentException("Invalid ID");


                // Check if exists
                var existingMappings = await _externalWorkshopExpRepository.ExcelMappingGetAsync(new ExcelMappingFilterDTO
                {
                    Id = updateDto.Id,
                    CompanyId = updateDto.CompanyId
                });

                if (!existingMappings.Any())
                    throw new KeyNotFoundException($"Excel mapping with ID {updateDto.Id} not found");

                var result = await _externalWorkshopExpRepository.ExcelMappingUpdateAsync(updateDto);
                return result > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ExcelMappingColumnDTO>> ExcelMappingGetColumnsAsync()
        {
            try
            {
                var result = await _externalWorkshopExpRepository.ExcelMappingGetColumnsAsync();
                return result?.Skip(3) ?? Enumerable.Empty<ExcelMappingColumnDTO>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DExcelMappingDTO>> ExcelMappingGetDetailsByIdAsync(int? id, int? workshopId)
        {
            try
            {
                if (id <= 0 && workshopId <= 0)
                    throw new ArgumentException("Either ID or Workshop ID must be provided");

                return await _externalWorkshopExpRepository.ExcelMappingGetDetailsByIdAsync(id, workshopId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<IEnumerable<ExternalWorkshopExpReportDTO>> ExternalWorkshopExpReportAsync(ExternalWorkshopExpReportFilterDTO filter)
        {
            try
            {
                if (filter.CompanyId <= 0)
                    throw new ArgumentException("Invalid company ID");

                return await _externalWorkshopExpRepository.ExternalWorkshopExpReportAsync(filter);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


       
    }
}