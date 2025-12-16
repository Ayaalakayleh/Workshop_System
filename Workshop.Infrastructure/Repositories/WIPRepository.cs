using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Services;
using Workshop.Domain.Entities;
using Workshop.Infrastructure.Contexts;

namespace Workshop.Infrastructure.Repositories
{
    public class WIPRepository : IWIPRepository
    {
        private readonly Database _database;
        private readonly DapperContext _context;
        public WIPRepository(Database database, DapperContext context)
        {
            _database = database;
            _context = context;
        }

        public async Task<IEnumerable<WIPDTO>> GetAllAsync(FilterWIPDTO oFilter)
        {
            var parameters = new
            {
                WIPNo = oFilter.WIPNo,
                Status = oFilter.Status,
                CustomerId = oFilter.CustomerId,
                PageNumber = oFilter.PageNumber,
                WorkshopId = oFilter.WorkshopId
            };
            return await _database.ExecuteGetAllStoredProcedure<WIPDTO>("WIP_GetAll", parameters);
        }

        public async Task<IEnumerable<CreateWIPServiceDTO>> GetAllInternalLabourLineAsync(int WIPId)
        {
            var parameters = new
            {
                WIPId = WIPId
              
            };
            return await _database.ExecuteGetAllStoredProcedure<CreateWIPServiceDTO>("WIP_GetInternalLabourLine", parameters);
        }

        public async Task<IEnumerable<CreateItemDTO>> GetAllInternalPartsLineAsync(int WIPId)
        {
            var parameters = new
            {
                WIPId = WIPId
              
            };
            return await _database.ExecuteGetAllStoredProcedure<CreateItemDTO>("WIP_GetInternalPartsLine", parameters);
        }

        public async Task<IEnumerable<WIPDTO>> GetAllDDLAsync()
        {
            using var connection = _context.CreateConnection();

            var result = await connection.QueryAsync<WIPDTO>(
                "WIP_GetAllDDL",
                commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<WIPDTO?> GetByIdAsync(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetByIdProcedure<WIPDTO>("WIP_GetById", parameters);
        }

        public async Task<int> AddAsync(CreateWIPDTO dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("VehicleId", dto.VehicleId);
            parameters.Add("MovementId", dto.MovementId);
            parameters.Add("WorkOrderId", dto.WorkOrderId);
            parameters.Add("JobCardNo", dto.JobCardNo);
            parameters.Add("Status", dto.Status);
            parameters.Add("WipDate", dto.WipDate);
            parameters.Add("Note", dto.Note);
            parameters.Add("TotalParts", dto.TotalParts);
            parameters.Add("TotalTechnicians", dto.TotalTechnicians);
            parameters.Add("WorkshopId", dto.WorkshopId);
            parameters.Add("FK_WarehouseId", dto.FK_WarehouseId);
            parameters.Add("IsExternal", dto.IsExternal);
            parameters.Add("CreatedBy", dto.CreatedBy);

            var table = _ToDataTable_Items(dto.ItemsList);
            var table2 = _ToDataTable_RTS(dto.ServicesList);
            parameters.Add("Items", table.AsTableValuedParameter("dbo.WIP_ItemTableType"));
            parameters.Add("Services", table2.AsTableValuedParameter("dbo.ServiceType"));

            return await _database.ExecuteAddStoredProcedure<int>("WIP_Insert", parameters);
        }

        public async Task<int> InsertWIPAccountAsync(AccountDTO dto)
        {
            var parameters = new
            {
                WIPId = dto.WIPId,
                AccountType = dto.AccountType,
                SalesType = dto.SalesType,
                CustomerId = dto.CustomerId,
                CurrencyId = dto.CurrencyId,
                TermsId = dto.TermsId,
                Vat = dto.Vat,
                PartialAccountType = dto.PartialAccountType,
                PartialSalesType = dto.PartialSalesType,
                PartialCustomerId = dto.PartialCustomerId,
                PartialCurrencyId = dto.PartialCurrencyId,
                PartialTermsId = dto.PartialTermsId,
                PartialVat = dto.PartialVat
            };
            return await _database.ExecuteAddStoredProcedure<int>("WIP_Account_Insert", parameters);
        }

        public async Task<int> InsertWIPVehicleDetailsAsync(VehicleTabDTO dto)
        {
            var parameters = new
            {
                WIPId = dto.WIPId,
                DepartmentId = dto.DepartmentId,
                CarPark = dto.CarPark,
                VehServiceDesc = dto.VehServiceDesc,
                VehConcerns = dto.VehConcerns,
                VehAdvisorNotes = dto.VehAdvisorNotes,
                OdometerPrevious = dto.OdometerPrevious,
                OdometerCurrentIN = dto.OdometerCurrentIN,
                OdometerCurrentOUT = dto.OdometerCurrentOUT,
                ManufacturerId = dto.ManufacturerId,
                ModelId = dto.ModelId,
                ClassId = dto.ClassId,
                ManufacturingYear = dto.ManufacturingYear,
                Color = dto.Color,
                PlateNumber = dto.PlateNumber
            };


            return await _database.ExecuteAddStoredProcedure<int>("WIP_Details_Insert", parameters);
        }

        public async Task<int> UpdateAsync(UpdateWIPDTO dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Id", dto.Id);
            parameters.Add("Status", dto.Status);
            parameters.Add("WipDate", dto.WipDate);
            parameters.Add("FK_WarehouseId", dto.FK_WarehouseId);
            parameters.Add("TotalParts", dto.TotalParts);
            parameters.Add("TotalTechnicians", dto.TotalTechnicians);
            parameters.Add("WorkshopId", dto.WorkshopId);
            parameters.Add("Note", dto.Note);
            parameters.Add("ModifyBy", dto.ModifyBy);

            var table = _ToDataTable_Items(dto.ItemsList);
            var table2 = _ToDataTable_RTS(dto.ServicesList);
            parameters.Add("Items", table.AsTableValuedParameter("dbo.WIP_ItemTableType"));
            parameters.Add("Services", table2.AsTableValuedParameter("dbo.ServiceType"));

            return await _database.ExecuteUpdateProcedure<int>("WIP_Update", parameters);
        }
        private DataTable _ToDataTable_Items(IEnumerable<BaseItemDTO> Items)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
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
                table.Rows.Add(item.Id, item.WIPId, item.RequestId, item.ItemId, item.fk_UnitId, item.WarehouseId, item.LocatorId, item.RequestQuantity, item.Quantity, item.UsedQuantity,
                   item.Price, item.CostPrice, item.SalePrice, item.Discount, item.Total, item.ModifyBy, item.AccountType);
            }

            return table;
        }
        //Inventory Integration
        private DataTable _ToDataTable_BaseItems(IEnumerable<WIPGetItems> dto)
        {
            var table = new DataTable();
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
            table.Columns.Add("ModifyBy", typeof(int));
            table.Columns.Add("Discount", typeof(decimal));
            table.Columns.Add("Total", typeof(decimal));
            table.Columns.Add("AccountType", typeof(int));


            foreach (var item in dto)
            {
                if (item.Items == null || item.Items.Count == 0)
                    continue;

                foreach (var i in item.Items)
                {
                    table.Rows.Add(item.Id, i.RequestId, i.ItemId, i.fk_UnitId, i.WarehouseId, i.LocatorId, i.RequestQuantity, i.Quantity, i.UsedQuantity,
                        i.Price, i.CostPrice, i.SalePrice,i.Discount, i.Total, i.ModifyBy, i.AccountType);
                }
            }

            return table;
        }
        private DataTable _ToDataTable_RTS(IEnumerable<CreateWIPServiceDTO> Services)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("WIPId", typeof(int));
            table.Columns.Add("Code", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("LongDescription", typeof(string));
            table.Columns.Add("StandardHours", typeof(decimal));
            table.Columns.Add("BaseRate", typeof(decimal));
            table.Columns.Add("Rate", typeof(decimal));
            table.Columns.Add("TimeTaken", typeof(decimal));
            table.Columns.Add("Discount", typeof(decimal));
            table.Columns.Add("Total", typeof(decimal));
            table.Columns.Add("Status", typeof(int));
            table.Columns.Add("AccountType", typeof(int));

            foreach (var item in Services)
            {
                table.Rows.Add(item.Id, item.WIPId, item.Code, item.Description, item.LongDescription, item.StandardHours,
                               item.BaseRate, item.Rate, item.TimeTaken, item.Discount, item.Total, item.Status, item.AccountType);
            }


            return table;
        }
        public async Task<IEnumerable<RTSCodeDTO>> GetAllServicesWithTimeAsync(RTSWithTimeDTO dto)
        {
            var parameters = new
            {
                CompanyId = dto.CompanyId,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                Class = dto.Class
            };
            return await _database.ExecuteGetAllStoredProcedure<RTSCodeDTO>("GetAllServicesWithTime", parameters);
        }
        public async Task<IEnumerable<MenuDTO>> GetMenuServicesAsync()
        {
            return await _database.ExecuteGetAllStoredProcedure<MenuDTO>("GetMenuDDL", null);
        }
        public async Task<IEnumerable<CreateItemDTO?>> WIP_GetItemsById(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetAllStoredProcedure<CreateItemDTO>("WIP_GetItemsById", parameters);
        }

        public async Task<AccountDTO?> WIP_GetAccountById(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetByIdProcedure<AccountDTO>("WIP_GetAccountById", parameters);
        }

        public async Task<VehicleTabDTO?> WIP_GetVehicleDetailsById(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetByIdProcedure<VehicleTabDTO>("WIP_GetVehicleDetailsById", parameters);
        }

        public async Task<IEnumerable<CreateWIPServiceDTO?>> WIP_GetServicesById(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetAllStoredProcedure<CreateWIPServiceDTO>("WIP_GetServicesById", parameters);
        }
        //Inventory Integration
        public async Task<IEnumerable<WIPGetItems>> WIP_Get(int Id = 0)
        {
            try
            {
                var parameters = new { Id = Id };
                var result = await _database.ExecuteGetMultipleTablesAsync("WIP_Get", parameters,
                     new Type[] { typeof(WIPGetItems), typeof(BaseItemDTO) });


                if (result == null || result.Count < 1 || result[0] == null)
                    return null;

                // First result 
                var table1 = (List<object>?)result[0];
                if (table1 == null || table1.Count == 0)
                    return null;


                // Second result 
                List<object> table2 = null;
                if (result.Count > 1)
                    table2 = (List<object>)result[1];


                var wipItemsList = new List<WIPGetItems>();

                foreach (var item in table1)
                {
                    var oWIPGetItems = (WIPGetItems)item;

                    if (table2 != null && table2.Count > 0)
                    {
                        //var items = table2.Cast<BaseItemDTO>().ToList();
                        var relatedItems = table2.Cast<BaseItemDTO>().Where(x => x.WIPId == oWIPGetItems.Id).ToList();
                        oWIPGetItems.Items = relatedItems;
                    }
                    else
                    {
                        oWIPGetItems.Items = new List<BaseItemDTO>();
                    }

                    wipItemsList.Add(oWIPGetItems);
                }

                return wipItemsList;


            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ReturnItems>> GetReturnParts(int WIPId = 0)
        {
            try
            {
                var parameters = new { WIPId = WIPId };
                var result = await _database.ExecuteGetMultipleTablesAsync("GetReturnParts", parameters,
                     new Type[] { typeof(ReturnItems), typeof(ReturnDetailsDTO) });


                if (result == null || result.Count < 1 || result[0] == null)
                    return null;

                // First result 
                var table1 = (List<object>?)result[0];
                if (table1 == null || table1.Count == 0)
                    return null;


                // Second result 
                List<object> table2 = null;
                if (result.Count > 1)
                    table2 = (List<object>)result[1];


                var itemList = new List<ReturnItems>();

                foreach (var item in table1)
                {
                    var dto = (ReturnItems)item;

                    if (table2 != null && table2.Count > 0)
                    {
                        var relatedItems = table2.Cast<ReturnDetailsDTO>().Where(x => x.WIPId == dto.WIPId).ToList();
                        dto.Items = relatedItems;
                    }
                    else
                    {
                        dto.Items = new List<ReturnDetailsDTO>();
                    }

                    itemList.Add(dto);
                }

                return itemList;


            }
            catch (Exception ex)
            {
                throw;
            }
        }
        //Inventory Integration
        public async Task<IEnumerable<GeneralRequest>> WIP_IDs(int Id = 0)
        {
            var parameters = new { Id = Id };
            return await _database.ExecuteGetAllStoredProcedure<GeneralRequest>("WIP_IDs", parameters);
        }
        //Inventory Integration
        public async Task<int> WIP_GeneralRequest_Insert(GeneralRequest dto)
        {
            var parameters = new
            {
                WIPId = dto.WIPId,
                CreatedBy = dto.CreatedBy,
                RequestDescription = dto.RequestDescription
            };
            return await _database.ExecuteAddStoredProcedure<int>("WIP_GeneralRequest_Insert", parameters);
        }
        //Inventory Integration
        public async Task<int> AddItemsAsync(List<WIPGetItems> items)
        {
            var parameters = new DynamicParameters();
            var itemsTable = _ToDataTable_BaseItems(items);
            parameters.Add("Items", itemsTable.AsTableValuedParameter("dbo.WIP_ItemTableType"));
            return await _database.ExecuteAddStoredProcedure<int>("WIP_ItemsInsert", parameters);
        }

        public async Task<int> WIPSChedule_Insert(WIPSChedule dto)
        {
            var parameters = new
            {
                WIPId = dto.WIPId,
                RTSId = dto.RTSId,
                TechnicianId = dto.TechnicianId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                Duration = dto.Duration,
                EndTime = dto.EndTime
            };
            return await _database.ExecuteAddStoredProcedure<int>("WIP_SChedule_Insert", parameters);
        }

        public async Task<WIPSChedule?> WIP_SChedule_Get(int RTSId, int WIPId)
        {
            var parameters = new { RTSId = RTSId, WIPId = WIPId };

            return await _database.ExecuteGetByIdProcedure<WIPSChedule>("WIP_SChedule_Get", parameters);
        }
        public async Task<IEnumerable<WIPSChedule>> WIP_SChedule_GetAll()
        {

            return await _database.ExecuteGetAllStoredProcedure<WIPSChedule>("WIP_SChedule_GetAll", new object() { });
        }

        public async Task<int> DeleteAsync(DeleteWIPDTO dto)
        {
            var parameters = new { Id = dto.Id };
            return await _database.ExecuteDeleteProcedure<int>("WIP_Delete", parameters);
        }

        public async Task<int> WIP_DeleteItems(int WIPId, int Id)
        {
            var parameters = new { 
                WIPId = WIPId,
                Id = Id
            };
            return await _database.ExecuteDeleteProcedure<int>("WIP_DeleteItems", parameters);
        }

        //Inventory Integration
        public async Task<int> DeleteItem(DeleteWIPDTO dto)
        {
            var parameters = new { WIPId = dto.Id, ItemId = dto.ItemId };
            return await _database.ExecuteDeleteProcedure<int>("DeleteItem", parameters);
        }

        public async Task<int> UpdateServiceStatus(UpdateService dto)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("WIPId", dto.WIPId);
                parameters.Add("RTSId", dto.RTSId);
                parameters.Add("Status", dto.Status);
                return await _database.ExecuteUpdateProcedure<int>("UpdateServiceStatus", parameters);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> UpdateWIPStatus(UpdateWIPStatusDTO dto)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("WIPId", dto.WIPId);
                parameters.Add("StatusId", dto.StatusId);
                return await _database.ExecuteUpdateProcedure<int>("WIP_UpdateStatus", parameters);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> UpdateWIPOptions(WIPOptionsDTO dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WIPId", dto.WIPId);
            parameters.Add("PartialInvoicing", dto.PartialInvoicing);
            parameters.Add("ReturnParts", dto.ReturnParts);
            parameters.Add("RepeatRepair", dto.RepeatRepair);
            parameters.Add("UpdateDemand", dto.UpdateDemand);
            return await _database.ExecuteUpdateProcedure<int>("UpdateWIPOptions", parameters);
        }

        //Inventory Integration
        public async Task<int> UpdateReturnStatusById(int WIPId)
        {
            var parameters = new DynamicParameters();
            parameters.Add("WIPId", WIPId);
            return await _database.ExecuteUpdateProcedure<int>("WIP_UpdateReturnStatusById", parameters);
        }
        public async Task<int> UpdatePartStatus(UpdatePartStatus dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("StatusId", dto.StatusId);
            parameters.Add("WIPId", dto.WIPId);
            return await _database.ExecuteUpdateProcedure<int>("WIP_UpdatePartStatus", parameters);
        }

        public async Task<WIPOptionsDTO?> WIP_GetOptionsById(int id)
        {
            var parameters = new { Id = id };
            return await _database.ExecuteGetByIdProcedure<WIPOptionsDTO>("WIP_GetOptionsById", parameters);
        }

        //Inventory integration
        public async Task<IEnumerable<M_WIPServiceHistoryDTO?>> M_WIPServiceHistoryAsync(int VehicleId)
        {
            var parameters = new { VehicleId = VehicleId };
            return await _database.ExecuteGetAllStoredProcedure<M_WIPServiceHistoryDTO>("M_WIP_ServiceHistory", parameters);
        }
        //public async Task<IEnumerable<PartsRequest_RemainingQtyGroupDTO?>> WIP_PartsRequest_RemainingQty(int WIPId)
        //{
        //    var parameters = new { WIPId = WIPId };
        //    return await _database.ExecuteGetAllStoredProcedure<PartsRequest_RemainingQtyGroupDTO>("WIP_PartsRequest_RemainingQty", parameters);
        //}

        public async Task<IEnumerable<PartsRequest_RemainingQtyGroupDTO>> WIP_PartsRequest_RemainingQty(int WIPId)
        {
            var parameters = new { WIPId = WIPId };
            var result = await _database.ExecuteGetAllStoredProcedure<PartsRequest_RemainingQtyDTO>("WIP_PartsRequest_RemainingQty", parameters);

            var groupedResult = result
                .GroupBy(x => x.WIPId)
                .Select(g => new PartsRequest_RemainingQtyGroupDTO
                {
                    WIPId = g.Key,
                    Items = g.Select(i => new PartsRequest_RemainingQtyItemDTO
                    {
                        ItemId = i.ItemId,
                        fk_UnitId = i.fk_UnitId,
                        RemainingQuantity = i.RemainingQuantity
                    }).ToList()
                })
                .ToList();

            return groupedResult;
        }

        public async Task<decimal?> WIP_GetLabourRate(LabourRateFilterDTO filter)
        {

            var dt = new DataTable();
            dt.Columns.Add("Value", typeof(int));
            if (filter.Skills != null)
            {
                foreach (var skill in filter.Skills)
                    dt.Rows.Add(skill);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@TechnicianId", filter.TechnicianId);
            parameters.Add("@CustomerId", filter.CustomerId);
            parameters.Add("@RTSId", filter.RTSId);
            parameters.Add("@Make", filter.Make);
            parameters.Add("@AccountType", filter.AccountType);
            parameters.Add("@SalesType", filter.SalesType);
            parameters.Add("@Skills", dt.AsTableValuedParameter("dbo.IntList"));
            return await _database.ExecuteGetByIdProcedure<decimal>("WIP_GetLabourRate", parameters);
        }
        public async Task<IEnumerable<WIPServiceHistoryDetails_Parts?>> WIP_ServiceHistoryDetails_GetPartsByWIPId(int id)
        {
            var parameters = new { WIPId = id };
            return await _database.ExecuteGetAllStoredProcedure<WIPServiceHistoryDetails_Parts?>("D_WIP_ServiceHistoryDetails_GetParts", parameters);
        }

        public async Task<IEnumerable<WIPServiceHistoryDetails_Labour?>> WIP_ServiceHistoryDetails_GetLaboursByWIPId(int id)
        {
            var parameters = new { WIPId = id };
            return await _database.ExecuteGetAllStoredProcedure<WIPServiceHistoryDetails_Labour?>("D_WIP_ServiceHistoryDetails_GetLabours", parameters);
        }
        public async Task<IEnumerable<WIPServiceHistoryDetails_Parts?>> WIP_ServiceHistoryDetails_GetParts()
        {
            var parameters = new { };
            return await _database.ExecuteGetAllStoredProcedure<WIPServiceHistoryDetails_Parts?>("D_WIP_ServiceHistoryDetails_GetParts", parameters);
        }

        public async Task<IEnumerable<WIPServiceHistoryDetails_Labour?>> WIP_ServiceHistoryDetails_GetLabours()
        {
            var parameters = new { };
            return await _database.ExecuteGetAllStoredProcedure<WIPServiceHistoryDetails_Labour?>("D_WIP_ServiceHistoryDetails_GetLabours", parameters);
        }

        public async Task<int> DeleteService(DeleteServiceDTO dto)
        {
            var parameters = new
            {
                Id = dto.Id,
                WIPID = dto.WIPId
            };
            return await _database.ExecuteDeleteProcedure<int>("WIP_DeleteService", parameters);
        }

        public async Task<int?> WIP_Close(CloseWIPDTO dto)
        {
            var parameters = new
            {
                WIPID = dto.WIPId,
                ClosedBy = dto.ClosedBy
            };
            return await _database.ExecuteUpdateProcedure<int>("WIP_Close", parameters);
        }
        public async Task<int?> WIP_Validation(int WIPId)
        {
            var parameters = new
            {
                WIPID = WIPId,
               
            };
            return await _database.ExecuteUpdateProcedure<int>("WIP_Validation", parameters);
        }



        public async Task<IEnumerable<CreateWIPServiceDTO>> GetWIPServicesByMovementIdAsync(int movementId)
        {
            var parameters = new { MovementId = movementId };

            return await _database.ExecuteGetAllStoredProcedure<CreateWIPServiceDTO>(
                "GetWIPServicesByMovementId",
                parameters
                );

        }

        #region Transfer Movement

        public async Task TransferMaintenanceMovement(int movementId, int workshopId, Guid masterId, string reason)
        {

            var parameters = new
            {
                MovementId = movementId,
                WorkshopId = workshopId,
                masterId = masterId,
                Reason = reason
            };

            await _database.ExecuteNonReturnProcedure("TransferMaintenanceMovement", parameters);
        }

        public async Task<int> UpdateWIPServicesIsExternalAsync(string ids)
        {

            var parameters = new
            {
                Ids = ids
            };

            return await _database.ExecuteUpdateProcedure<int>("UpdateWIPServicesIsExternal", parameters);
        }

        public async Task<int> UpdateWIPServicesIsFixedAsync(string ids)
        {

            var parameters = new { Ids = ids };

            var result = await _database.ExecuteUpdateProcedure<int>(
                "UpdateWIPServicesIsFixed",
                parameters
            );

            return result;
        }
        public async Task<List<VehicleMovement>> GetAllVehicleTransferMovementAsync(int? vehicleId, int? page, int workshopId)
        {

            var parameters = new
            {
                VehicleID = vehicleId,
                PageNumber = page ?? 1,
                WorkshopId = workshopId,
            };

            var result = await _database.ExecuteGetAllStoredProcedure<VehicleMovement>(
                "D_WorkshopVehicleTransferMovement_Filter",
                parameters
                );

            return result?.ToList();
        }
        #endregion

        public async Task<CheckWIPCountDTO> GetByVehicleIdAsync(int vehicleId)
        {
            var parameters = new { VehicleId = vehicleId };
            return await _database.ExecuteGetByIdProcedure<CheckWIPCountDTO>("WIP_GetByVehicleId", parameters);
        }

        public async Task<int> UpdateIssueIdAsync(UpdateIssueIdDTO dto)
        {

            var parameters = new
            {
                IssueId = dto.IssueId,
                WIPId = dto.WIPId,
                Id = dto.Id
            };

            return await _database.ExecuteUpdateProcedure<int>("WIP_UpdateIssueId", parameters);
        }

        public async Task<IEnumerable<int>> WIP_GetOpenWIPs(int? Id, int BranchId)
        {

            var parameters = new
            {
                Id = Id,
                BranchId= BranchId
            };

            return await _database.ExecuteGetAllStoredProcedure<int>("WIP_GetOpenWIPs", parameters);

        }
        public async Task<int> UpdatePartStatusForSingleItem(UpdateSinglePartStatusDTO dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Id", dto.Id);
            parameters.Add("WIPId", dto.WIPId);
            //parameters.Add("ItemId", dto.ItemId);
            //parameters.Add("LocatorId", dto.LocatorId);
            parameters.Add("StatusId", dto.StatusId);

            return await _database.ExecuteUpdateProcedure<int>(
                "WIP_UpdatePartStatusForSingleItem",
                parameters
            );
        }
        public async Task<IEnumerable<WIPDTO>> GetWIPByMovementIds(string movementIds)
        {
            var parameters = new { MovementIds = movementIds };

            return await _database.ExecuteGetAllStoredProcedure<WIPDTO>(
                "sp_GetWIPByMovementIds",
                parameters
            );
        }
        public async Task<int> WIP_UpdatePartWarehouseForSingleItem(UpdateSinglePartWarehouseDTO dto)
        {
            var parameters = new DynamicParameters();
            parameters.Add("Id", dto.Id);
            parameters.Add("WIPId", dto.WIPId);
            //parameters.Add("ItemId", dto.ItemId);
            parameters.Add("WarehouseId", dto.WarehouseId);
            parameters.Add("LocatorId", dto.LocatorId);

            return await _database.ExecuteUpdateProcedure<int>(
                "WIP_UpdatePartWarehouseForSingleItem",
                parameters
            );
        }

        public async Task<int> WIP_Invoice_Insert(CreateWIPInvoiceDTO dto)
        {
            var parameters = new
            {
                WIPId = dto.WIPId,
               
                InvoiceNo = dto.InvoiceNo,
                InvoiceDate = dto.InvoiceDate,
                InvoiceType = dto.InvoiceType,
                AccountType = dto.AccountType,
                TransactionMasterId = dto.TransactionMasterId,
                TransactionCostMasterId = dto.TransactionCostMasterId,
                OldTransactionMasterId=dto.OldTransactionMasterId,
                ReferanceNo = dto.ReferanceNo,
                Total = dto.Total,
                Tax =   dto.Tax,
                Discount = dto.Discount,
                Net = dto.Net,
                CreatedBy = dto.CreatedBy,
            };
            return await _database.ExecuteAddStoredProcedure<int>("WIP_Invoice_Insert", parameters);
        }

        public async Task<IEnumerable<WIPInvoiceDTO?>> WIP_Invoice_GetById(int? id, int? TransactionMasterId)
        {
            var parameters = new { 
                WIPId = id,
                TransactionMasterId = TransactionMasterId 
            };
            return await _database.ExecuteGetAllStoredProcedure<WIPInvoiceDTO>("WIP_Invoice_GetById", parameters);
        }
        public async Task<IEnumerable<WipInvoiceDetailDTO>> WIP_InvoiceDetails_GetByHeaderId(int headerId)
        {
            var parameters = new
            {
                HeaderId = headerId
            };

            return await _database.ExecuteGetAllStoredProcedure<WipInvoiceDetailDTO>(
                "WIP_InvoiceDetails_GetByHeaderId",
                parameters
            );
        }

    }
}
