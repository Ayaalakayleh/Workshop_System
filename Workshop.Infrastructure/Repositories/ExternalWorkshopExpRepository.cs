
using Workshop.Core.DTOs.ExternalWorkshopExp;
using Workshop.Core.Interfaces.IRepositories;


namespace Workshop.Infrastructure.Repositories
{
    public class ExternalWorkshopExpRepository : IExternalWorkshopExpRepository
    {
        private readonly Database _database;

        public ExternalWorkshopExpRepository(Database database)
        {
            _database = database;
        }

        public async Task<IEnumerable<MExternalWorkshopExpDTO>> ExternalWorkshopExpGetAsync(ExternalWorkshopExpFilterDTO filter)
        {
            try
            {
                var parameters = new
                {
                    ExternalWorkshopId = filter.ExternalWorkshopId,
                    PageNumber = filter.Page,
                    RowsPerPage = filter.TotalPages,
                    FromDate = filter.FromDate ?? (object)DBNull.Value,
                    ToDate = filter.ToDate ?? (object)DBNull.Value,
                    companyId = filter.CompanyId,
                    branchId = filter.BranchId ?? (object)DBNull.Value
                };

                return await _database.ExecuteGetAllStoredProcedure<MExternalWorkshopExpDTO>(
                    "External_Workshop_Exp_Get",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving external workshop expenses");
                throw;
            }
        }

        public async Task<int> ExternalWorkshopExpInsertAsync(CreateExternalWorkshopExpDTO createDto)
        {
            try
            {
                //createDto.PRData = _database.ToDataTable<DExternalWorkshopExpDTO>(createDto.DExternalWorkshopExp);

                var parameters = new
                {
                    ExternalWorkshopId = createDto.ExternalWorkshopId ?? (object)DBNull.Value,
                    Excel_Date = createDto.Excel_Date ?? (object)DBNull.Value,
                    Type = createDto.Type ?? (object)DBNull.Value,
                    CompanyId = createDto.CompanyId,
                    BranchId = createDto.BranchId,
                    CreatedBy = createDto.CreatedBy ?? (object)DBNull.Value,
                    PRDetails = _database.ToDataTable<DExternalWorkshopExpDTO>(createDto.DExternalWorkshopExp)
                };

                return await _database.ExecuteAddStoredProcedure<int>(
                    "[External_Workshop_Exp_Insert]",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error inserting external workshop expense");
                throw;
            }
        }

        public async Task<IEnumerable<DExternalWorkshopExpDTO>> ExternalWorkshopExpGetDetailsByIdAsync(int headerId)
        {
            try
            {
                var parameters = new { HeaderId = headerId };

                return await _database.ExecuteGetAllStoredProcedure<DExternalWorkshopExpDTO>(
                    "External_Workshop_Exp_GetDetailsByID",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, $"Error retrieving external workshop expense details for header ID {headerId}");
                throw;
            }
        }

        public async Task<MExternalWorkshopExpDTO?> ExternalWorkshopExpGetByIdAsync(int id)
        {
            try
            {
                var parameters = new { Id = id };

                var result = await _database.ExecuteGetByIdProcedure<MExternalWorkshopExpDTO>(
                    "External_Workshop_Exp_GetByID",
                    parameters);

                return result;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, $"Error retrieving external workshop expense with ID {id}");
                throw;
            }
        }

        public async Task<int> ExternalWorkshopExpDetailsUpdateAsync(List<DExternalWorkshopExpDTO> prData)
        {
            try
            {
                var parameters = new { PRDetails = prData };

                return await _database.ExecuteUpdateProcedure<int>(
                    "External_Workshop_Exp_Details_Update",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error updating external workshop expense details");
                throw;
            }
        }

        public async Task<IEnumerable<MExcelMappingDTO>> ExcelMappingGetAsync(ExcelMappingFilterDTO filter)
        {
            try
            {
                var parameters = new
                {
                    WorkshopId = filter.WorkshopId ?? 0,
                    companyId = filter.CompanyId,
                    BranchId = filter.BranchId ?? (object)DBNull.Value,
                    PageNumber = filter.Page ?? 1,
                    Id = filter.Id ?? 0
                };

                return await _database.ExecuteGetAllStoredProcedure<MExcelMappingDTO>(
                    "M_Excel_Mapping_Get",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving excel mappings");
                throw;
            }
        }

        public async Task<int> ExcelMappingInsertAsync(CreateExcelMappingDTO createDto)
        {
            try
            {
                var parameters = new
                {
                    WorkshopId = createDto.WorkshopId,
                    FilePath = createDto.FilePath,
                    FileName = createDto.FileName,
                    Started_Column = createDto.Started_Column,
                    Started_Row = createDto.Started_Row,
                    CompanyId = createDto.CompanyId,
                    BranchId = createDto.BranchId,
                    CreatedBy = createDto.CreatedBy,
                    PRDetails = _database.ToDataTable<DExcelMappingDTO>(createDto.DExcelMappingList) 
                };

                return await _database.ExecuteAddStoredProcedure<int>(
                    "Excel_Mapping_Insert",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error inserting excel mapping");
                throw;
            }
        }

        public async Task<int> ExcelMappingUpdateAsync(UpdateExcelMappingDTO updateDto)
        {
            try
            {
                var parameters = new
                {
                    Id = updateDto.Id,
                    FilePath = updateDto.FilePath,
                    FileName = updateDto.FileName,
                    Started_Column = updateDto.Started_Column,
                    Started_Row = updateDto.Started_Row,
                    CompanyId = updateDto.CompanyId,
                    BranchId = updateDto.BranchId,
                    UpdatedBy = updateDto.UpdatedBy,
                    PRDetails = _database.ToDataTable<DExcelMappingDTO>(updateDto.DExcelMappingList)
                };

                return await _database.ExecuteUpdateProcedure<int>(
                    "[Excel_Mapping_Update]",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, $"Error updating excel mapping with ID {updateDto.Id}");
                throw;
            }
        }

        public async Task<IEnumerable<ExcelMappingColumnDTO>> ExcelMappingGetColumnsAsync()
        {
            try
            {
                var result = await _database.ExecuteGetAllStoredProcedure<ExcelMappingColumnDTO>(
                    "Excel_Mapping_GetColumns",
                    null);

                return result;
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving excel mapping columns");
                throw;
            }
        }

        public async Task<IEnumerable<DExcelMappingDTO>> ExcelMappingGetDetailsByIdAsync(int? id, int? workshopId)
        {
            try
            {
                var parameters = new
                {
                    Id = id ?? (object)DBNull.Value,
                    WorkshopId = workshopId ?? (object)DBNull.Value
                };

                return await _database.ExecuteGetAllStoredProcedure<DExcelMappingDTO>(
                    "Excel_Mapping_GetDetailsByID",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, $"Error retrieving excel mapping details for ID {id}, Workshop ID {workshopId}");
                throw;
            }
        }

        public async Task<IEnumerable<ExternalWorkshopExpReportDTO>> ExternalWorkshopExpReportAsync(ExternalWorkshopExpReportFilterDTO filter)
        {
            try
            {
                var parameters = new
                {
                    ExternalWorkshopId = filter.ExternalWorkshopId ?? (object)DBNull.Value,
                    FromDate = filter.FromDate ?? (object)DBNull.Value,
                    ToDate = filter.ToDate ?? (object)DBNull.Value,
                    companyId = filter.CompanyId,
                    branchId = filter.BranchId ?? (object)DBNull.Value,
                    VehicleId = filter.VehicleId ?? (object)DBNull.Value
                };

                return await _database.ExecuteGetAllStoredProcedure<ExternalWorkshopExpReportDTO>(
                    "[External_Workshop_Exp_Report]",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving external workshop expense report");
                throw;
            }
        }

    }
}