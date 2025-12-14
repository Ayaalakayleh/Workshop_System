using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IMenuRepository
    {
        Task<IEnumerable<MenuDTO>> GetAllAsync(string GroupCode, string Name, int? PageNumber = 0);
        Task<IEnumerable<MenuDTO>> GetAllMenuDDL();
        Task<MenuDTO?> GetByIdAsync(int id);
        Task<IEnumerable<MenuGroupDTO>> GetMenuItemsByIdAsync(int id);
        Task<int> AddAsync(CreateMenuDTO dto);
        Task<int> UpdateAsync(UpdateMenuDTO dto);
        Task<int> DeleteAsync(DeleteMenuDTO dto);
    }
}
