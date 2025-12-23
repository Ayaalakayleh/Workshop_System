using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class WorkShopRepository : IWorkShopRepository
    {
        private readonly Database _database;

        public WorkShopRepository(Database database)
        {
            _database = database;
        }

        public async Task<IEnumerable<WorkshopListDTO>> GetAllWorkshopsPageAsync(WorkShopFilterDTO workShopFilterDTO)
        {
            try
            {
                var parameters = new
                {
                    Id = workShopFilterDTO.Id,
                    ParentId = workShopFilterDTO.ParentId,
                    PrimaryName = workShopFilterDTO.PrimaryName,
                    SecondaryName = workShopFilterDTO.SecondaryName,
                    CityId = workShopFilterDTO.CityId,
                    Email = workShopFilterDTO.Email,
                    Phone = workShopFilterDTO.Phone,
                    PrimaryAddress = workShopFilterDTO.PrimaryAddress,
                    SecondaryAddress = workShopFilterDTO.SecondaryAddress,
                    Page = workShopFilterDTO.Page,
                    RowsOfPage = workShopFilterDTO.RowsOfPage,
                    CompanyId = workShopFilterDTO.CompanyId
                };

                return await _database.ExecuteGetAllStoredProcedure<WorkshopListDTO>(
                    "D_Workshop_GetAll_Page",
                    parameters);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving workshops");
                throw;
            }
        }

        public async Task<IEnumerable<WorkShopDefinitionDTO>> WorkshopGetAllAsync(int companyId, int? branchId, int? cityId = null)
        {
            try
            {
                var parameters = new
                {
                    CompanyId = companyId,
                    BranchId = branchId ?? (object)DBNull.Value,
                    CityId = cityId ?? (object)DBNull.Value
                };

                return await _database.ExecuteGetAllStoredProcedure<WorkShopDefinitionDTO>(
                    "D_Workshop_GetAll",
                    parameters);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error retrieving all workshops");
                throw;
            }
        }

        public async Task<WorkShopDefinitionDTO?> GetByIdAsync(int id)
        {
            try
            {
                var parameters = new { Id = id };
                var result = await _database.ExecuteGetMultipleTablesAsync(
                    "D_Workshop_GetById",
                    parameters,
                    new Type[] { typeof(WorkShopDefinitionDTO), typeof(int[]) }
                    );

                if (result == null || result.Count < 1 || result[0] == null)
                    return null;

                // First result set is the workshop details
                var table1 = (List<object>?)result[0];
                
                if (table1 == null || table1.Count == 0)
                    return null;

                var workshop = (WorkShopDefinitionDTO)table1[0];

                // Second result set is the insurance company IDs
                List<object> table2 = null;
                
                if (result.Count > 1)
                    table2 = (List<object>)result[1];

                if (table2 != null && table2.Count > 0)
                {
                    var insuranceCompanyIds = table2.Cast<int>().ToList();
                    workshop.InsuranceCompany = string.Join(",", insuranceCompanyIds);
                }

                return workshop;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error retrieving workshop with ID {id}");
                throw;
            }
        }

        public async Task<int> CreateAsync(CreateWorkShopDTO createDto)
        {
            try
            {
                var parameters = new
                {
                    PrimaryName = createDto.PrimaryName,
                    SecondaryName = createDto.SecondaryName,
                    Email = createDto.Email,
                    Phone = createDto.Phone,
                    GoogleURL = createDto.GoogleURL,
                    PrimaryAddress = createDto.PrimaryAddress,
                    SecondaryAddress = createDto.SecondaryAddress,
                    CityId = createDto.CityId ?? (object)DBNull.Value,
                    IsActive = createDto.IsActive,
                    ParentId = createDto.ParentId ?? 0,
                    VatClassificationId = createDto.VatClassificationId ?? (object)DBNull.Value,
                    AccountId = createDto.AccountId ?? (object)DBNull.Value,
                    SupplierId = createDto.SupplierId ?? (object)DBNull.Value,
                    CompanyId = createDto.CompanyId,
                    BranchId = createDto.BranchId,
                    CreatedBy = createDto.CreatedBy,
                    insuranceCompany = createDto.InsuranceCompany
                };

                return await _database.ExecuteAddStoredProcedure<int>(
                    "D_Workshop_Insert",
                    parameters);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error creating workshop");
                throw;
            }
        }

        public async Task<int> UpdateAsync(UpdateWorkShopDTO updateDto)
        {
            try
            {
                var parameters = new
                {
                    Id = updateDto.Id,
                    PrimaryName = updateDto.PrimaryName,
                    SecondaryName = updateDto.SecondaryName,
                    Email = updateDto.Email,
                    Phone = updateDto.Phone,
                    GoogleURL = updateDto.GoogleURL,
                    CityId = updateDto.CityId ?? (object)DBNull.Value,
                    IsActive = updateDto.IsActive,
                    ParentId = updateDto.ParentId ?? 0,
                    VatClassificationId = updateDto.VatClassificationId ?? (object)DBNull.Value,
                    AccountId = updateDto.AccountId ?? (object)DBNull.Value,
                    SupplierId = updateDto.SupplierId ?? (object)DBNull.Value,
                    CompanyId = updateDto.CompanyId,
                    BranchId = updateDto.BranchId,
                    UpdatedBy = updateDto.UpdatedBy,
                    PrimaryAddress = updateDto.PrimaryAddress,
                    SecondaryAddress = updateDto.SecondaryAddress,
                    WorkshopBranchId = (object)DBNull.Value,//ToDo: Value
                    insuranceCompany = updateDto.InsuranceCompany,

                };

                return await _database.ExecuteUpdateProcedure<int>(
                    "D_Workshop_Update",
                    parameters);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error updating workshop {updateDto.Id}");
                throw;
            }
        }

        public async Task<int> DeleteAsync(DeleteWorkShopDTO deleteDto)
        {
            try
            {
                var parameters = new
                {
                    Id = deleteDto.Id,
                    UpdatedBy = deleteDto.UpdatedBy
                };

                return await _database.ExecuteDeleteProcedure<int>(
                    "Workshop_Delete",
                    parameters);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Error deleting workshop {deleteDto.Id}");
                throw;
            }
        }


        public async Task<IEnumerable<T>> GetAllParentsAsync<T>(int CompanyId)
        {
            try
            {
                var parameters = new
                {
                    CompanyId = CompanyId
                };

                return await _database.ExecuteGetAllStoredProcedure<T>(
                    "D_Workshop_GetParents",
                    parameters);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error retrieving workshops");
                throw;
            }
        }
		public async Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_RootWorkshop(int companyId)
		{
			try
			{
				var parameters = new
				{
					CompanyId = companyId
				};

				return await _database.ExecuteGetAllStoredProcedure<WorkShopDefinitionDTO>(
					"D_Workshop_RootWorkshop",
					parameters
				);
			}
			catch (Exception)
			{
				throw;
			}
		}
		public async Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_GetByCompanyIdAndBranchId(int companyId)
		{
			try
			{
				var parameters = new
				{
					CompanyId = companyId
				};

				return await _database.ExecuteGetAllStoredProcedure<WorkShopDefinitionDTO>(
					"D_Workshop_GetByCompanyIdAndBranchId",
					parameters
				);
			}
			catch (Exception)
			{
				throw;
			}
		}

	}
}