using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;

namespace Workshop.Infrastructure.Repositories
{
    public class MenuRepository : IMenuRepository
    {
        private readonly Database _database;
        public MenuRepository(Database database)
        {
            _database = database;
        }

        public async Task<IEnumerable<MenuDTO>> GetAllAsync(string GroupCode, string Name, int? PageNumber = 0)
        {
            var parameters = new
            {
                GroupCode,
                Name,
                PageNumber
            };
            return await _database.ExecuteGetAllStoredProcedure<MenuDTO>("Menu_GetAll", parameters);
        }

        public async Task<IEnumerable<MenuDTO>> GetAllMenuDDL()
        {
            return await _database.ExecuteGetAllStoredProcedure<MenuDTO>("Menu_GetAll",null);
        }


        public async Task<MenuDTO?> GetByIdAsync(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetByIdProcedure<MenuDTO>("Menu_GetById", parameters);
        }

        public async Task<IEnumerable<MenuGroupDTO>> GetMenuItemsByIdAsync(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetAllStoredProcedure<MenuGroupDTO>("Menu_GetItemsById", parameters);
        }

        public async Task<int> AddAsync(CreateMenuDTO dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("GroupCode", dto.GroupCode);
            parameters.Add("GroupName", dto.GroupName);
            parameters.Add("PrimaryDescription", dto.PrimaryDescription);
            parameters.Add("SecondaryDescription", dto.SecondaryDescription);
            parameters.Add("PricingMethod", dto.PricingMethod);
            parameters.Add("Price", dto.Price);
            parameters.Add("TotalTime", dto.TotalTime);
            parameters.Add("EffectiveDate", dto.EffectiveDate);
            parameters.Add("IsActive", dto.IsActive);
            parameters.Add("CreatedBy", dto.CreatedBy);


            //var table = ToDataTable(dto.MenuGroup ?? new IEnumerable<MenuGroupDTO>());
            var table = ToDataTable(dto.MenuGroup);
            parameters.Add("MenuGroup", table.AsTableValuedParameter("dbo.MenuGroupType"));

           
            return await _database.ExecuteAddStoredProcedure<int>("Menu_Insert", parameters);
        }

        private DataTable ToDataTable(IEnumerable<MenuGroupDTO> menuGroup)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Code", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("IsPart", typeof(bool));
            table.Columns.Add("Quantity", typeof(decimal));
            table.Columns.Add("StandardHours", typeof(decimal));
            table.Columns.Add("Price", typeof(decimal));
            table.Columns.Add("Discount", typeof(decimal));
            table.Columns.Add("Total", typeof(decimal));

            foreach (var item in menuGroup)
            {
                table.Rows.Add(item.Id, item.Code, item.Description, item.IsPart, item.Quantity, item.StandardHours,
                               item.Price, item.Discount, item.Total);
            }

            return table;
        }

        public async Task<int> UpdateAsync(UpdateMenuDTO dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Id", dto.Id);
            parameters.Add("GroupCode", dto.GroupCode);
            parameters.Add("GroupName", dto.GroupName);
            parameters.Add("PrimaryDescription", dto.PrimaryDescription);
            parameters.Add("SecondaryDescription", dto.SecondaryDescription);
            parameters.Add("PricingMethod", dto.PricingMethod);
            parameters.Add("Price", dto.Price);
            parameters.Add("TotalTime", dto.TotalTime);
            parameters.Add("EffectiveDate", dto.EffectiveDate);
            parameters.Add("IsActive", dto.IsActive);
            parameters.Add("UpdatedBy", dto.UpdatedBy);


            //var table = ToDataTable(dto.MenuGroup ?? new IEnumerable<MenuGroupDTO>());
            var table = ToDataTable(dto.MenuGroup);
            parameters.Add("MenuGroup", table.AsTableValuedParameter("dbo.MenuGroupType"));
            
            return await _database.ExecuteUpdateProcedure<int>("Menu_Update", parameters);
        }

        public async Task<int> DeleteAsync(DeleteMenuDTO dto)
        {
            return await _database.ExecuteDeleteProcedure<int>("M_Menu_Delete", dto);
        }
    }
}
