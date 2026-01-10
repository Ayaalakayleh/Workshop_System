using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IItemsService
    {
        Task<int> WIPInsertItemsAsync(ItemsDTO dto);
    }
}