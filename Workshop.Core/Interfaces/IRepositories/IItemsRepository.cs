using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IItemsRepository
    {
        Task<int> WIPInsertItemsAsync(ItemsDTO dto);
    }
}