using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class ItemsRepository: IItemsRepository
    {
        private readonly Database _database;
        private readonly DapperContext _context;
        public ItemsRepository(Database database, DapperContext context)
        {
            _database = database;
            _context = context;
        }
        public async Task<IEnumerable<CreateItemDTO?>> WIP_GetItemsById(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetAllStoredProcedure<CreateItemDTO>("WIP_GetItemsById", parameters);
        }

        public async Task<int> WIPInsertItemsAsync(ItemsDTO dto)
        {
            var parameters = new DynamicParameters();
      
            parameters.Add("WIPId", dto.WIPId);

            var table = _ToDataTable_Items(dto.ItemsList);
            parameters.Add("Items", table.AsTableValuedParameter("dbo.WIP_ItemTableType"));

            return await _database.ExecuteAddStoredProcedure<int>("WIP_Insert_Items", parameters);
        }

        private DataTable _ToDataTable_Items(IEnumerable<BaseItemDTO> Items)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("KeyId", typeof(int));
            table.Columns.Add("WIPId", typeof(int));
            table.Columns.Add("RequestId", typeof(int));
            table.Columns.Add("ItemId", typeof(int));
            table.Columns.Add("fk_UnitId", typeof(int));
            table.Columns.Add("WarehouseId", typeof(int));
            table.Columns.Add("LocatorId", typeof(int));
            table.Columns.Add("RequestQuantity", typeof(decimal));
            table.Columns.Add("Quantity", typeof(decimal));
            table.Columns.Add("UsedQuantity", typeof(decimal));
            table.Columns.Add("Price", typeof(decimal));
            table.Columns.Add("CostPrice", typeof(decimal));
            table.Columns.Add("SalePrice", typeof(decimal));
            table.Columns.Add("Discount", typeof(decimal));
            table.Columns.Add("Total", typeof(decimal));
            table.Columns.Add("ModifyBy", typeof(int));
            table.Columns.Add("AccountType", typeof(int));

            foreach (var item in Items)
            {
                table.Rows.Add(item.Id, item.KeyId, item.WIPId, item.RequestId, item.ItemId, item.fk_UnitId, item.WarehouseId, item.LocatorId, item.RequestQuantity, item.Quantity, item.UsedQuantity,
                   item.Price, item.CostPrice, item.SalePrice, item.Discount, item.Total, item.ModifyBy, item.AccountType);
            }

            return table;
        }
    }
}
