using Workshop.Core.DTOs.WorkshopDTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IWorkShopService
    {
        Task<IEnumerable<WorkshopListDTO>> GetAllWorkshopsPageAsync(WorkShopFilterDTO workShopFilterDTO);
        Task<IEnumerable<WorkShopDefinitionDTO>> WorkshopGetAllAsync(int companyId, int? branchId, int? cityId = null, string lang = "en");
        Task<WorkShopDefinitionDTO?> GetWorkshopByIdAsync(int id);
        Task<int> CreateWorkshopAsync(CreateWorkShopDTO createDto);
        Task<int> UpdateWorkshopAsync(UpdateWorkShopDTO updateDto);
        Task<int> DeleteWorkshopAsync(DeleteWorkShopDTO deleteDto);
        Task<IEnumerable<ParentWorkshopSimpleDTO>> GetAllSipmleParentsWorkshop(int companyId, string language);
		Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_RootWorkshop(int companyId);
		Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_GetByCompanyIdAndBranchId(int companyId);
	}

}