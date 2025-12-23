using System.ComponentModel.Design;
using Workshop.Core.DTOs.WorkshopDTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IWorkShopRepository
    {
        Task<IEnumerable<WorkshopListDTO>> GetAllWorkshopsPageAsync(WorkShopFilterDTO workShopFilterDTO);
        Task<IEnumerable<WorkShopDefinitionDTO>> WorkshopGetAllAsync(int companyId, int? branchId, int? cityId = null);
        Task<WorkShopDefinitionDTO?> GetByIdAsync(int id);
        Task<int> CreateAsync(CreateWorkShopDTO createDto);
        Task<int> UpdateAsync(UpdateWorkShopDTO updateDto);
        Task<int> DeleteAsync(DeleteWorkShopDTO deleteDto);
        Task<IEnumerable<T>> GetAllParentsAsync<T>(int companyId);
		Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_RootWorkshop(int companyId);
		Task<IEnumerable<WorkShopDefinitionDTO>> D_Workshop_GetByCompanyIdAndBranchId(int companyId);
	}
}