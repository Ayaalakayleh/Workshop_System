using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class MenuService : IMenuService
    {
        private readonly IMenuRepository _repository;
        public MenuService(IMenuRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MenuDTO>> GetAllAsync(string GroupCode, string Name, int? PageNumber = 0)
        {
            return await _repository.GetAllAsync( GroupCode, Name, PageNumber);
        }
        public async Task<IEnumerable<MenuDTO>> GetAllMenuDDL()
        {
            return await _repository.GetAllMenuDDL();
        }

        public async Task<MenuDTO?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<MenuGroupDTO>> GetMenuItemsByIdAsync(int id)
        {
            return await _repository.GetMenuItemsByIdAsync(id);
        }

        public async Task<int> AddAsync(CreateMenuDTO dto)
        {
            return await _repository.AddAsync(dto);
        }

        public async Task<int> UpdateAsync(UpdateMenuDTO dto)
        {
            return await _repository.UpdateAsync(dto);
        }

        public async Task<int> DeleteAsync(DeleteMenuDTO dto)
        {
            return await _repository.DeleteAsync(dto);
        }
    }
}
