using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class ItemsService:IItemsService
    {
        private readonly IItemsRepository _repository;
        public ItemsService(IItemsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> WIPInsertItemsAsync(ItemsDTO dto)
        { 
            return await _repository.WIPInsertItemsAsync(dto);
        }
    }
}
