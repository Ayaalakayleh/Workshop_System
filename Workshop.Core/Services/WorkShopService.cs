using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class WorkShopService : IWorkShopService
    {
        private readonly IWorkShopRepository _workShopRepository;
        // Add other dependencies as needed (e.g., ILogger, IValidator)

        public WorkShopService(IWorkShopRepository workShopRepository)
        {
            _workShopRepository = workShopRepository;
        }

        public async Task<IEnumerable<WorkshopListDTO>> GetAllWorkshopsPageAsync(WorkShopFilterDTO workShopFilterDTO)
        {
            return await _workShopRepository.GetAllWorkshopsPageAsync(workShopFilterDTO);
        }

        public async Task<IEnumerable<WorkShopDefinitionDTO>> WorkshopGetAllAsync(int companyId, int? branchId, int? cityId = null, string lang = "en")
        {

            var result = await _workShopRepository.WorkshopGetAllAsync(companyId, branchId, cityId);

            foreach (var item in result)
            {
                item.Name = lang == "en" ? item.PrimaryName : item.SecondaryName;
            }
            return result;
        }

        public async Task<WorkShopDefinitionDTO?> GetWorkshopByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid workshop ID");

            return await _workShopRepository.GetByIdAsync(id);
        }

        public async Task<int> CreateWorkshopAsync(CreateWorkShopDTO createDto)
        {
            // Add validation logic here if needed
            // Example: Check if email is unique

            return await _workShopRepository.CreateAsync(createDto);
        }

        public async Task<int> UpdateWorkshopAsync(UpdateWorkShopDTO updateDto)
        {
            // Validate existence first
            var existing = await _workShopRepository.GetByIdAsync(updateDto.Id.Value);
            if (existing == null)
                throw new KeyNotFoundException("Workshop not found");

            return await _workShopRepository.UpdateAsync(updateDto);
        }

        public async Task<int> DeleteWorkshopAsync(DeleteWorkShopDTO deleteDto)
        {
            // Validate existence first
            var existing = await _workShopRepository.GetByIdAsync(deleteDto.Id);
            if (existing == null)
                throw new KeyNotFoundException("Workshop not found");

            return await _workShopRepository.DeleteAsync(deleteDto);
        }


        public async Task<IEnumerable<ParentWorkshopSimpleDTO>> GetAllSipmleParentsWorkshop(int companyId, string language = "en")
        {
            return await _workShopRepository.GetAllParentsAsync<ParentWorkshopSimpleDTO>(companyId);
        }
		public async Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_RootWorkshop(int companyId)
		{
			try
			{
				return await _workShopRepository.D_Workshop_RootWorkshop(companyId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in D_Workshop_RootWorkshop", ex);
			}
		}
		public async Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_GetByCompanyIdAndBranchId(int companyId)
		{
			try
			{
				return await _workShopRepository.D_Workshop_GetByCompanyIdAndBranchId(companyId);
			}
			catch (Exception ex)
			{
				throw new Exception("Error in D_Workshop_GetByCompanyIdAndBranchId", ex);
			}
		}
	}
}