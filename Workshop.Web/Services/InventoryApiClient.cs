using Azure;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.Text;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Web.Models;

namespace Workshop.Web.Services
{
    public class InventoryApiClient : BaseApiClient
    {
        public InventoryApiClient(
           HttpClient httpClient,
           IConfiguration config,
           IApiAuthStrategy apiAuthStrategy
           ) : base(
               httpClient,
               new ApiSettings
               {
                   ApiUser = config.GetValue<string>("ApiSettings:InventoryApiUser"),
                   ApiPassword = config.GetValue<string>("ApiSettings:InventoryApiPassword"),

               }, apiAuthStrategy
           )
        {
        }

        public async Task<List<ItemDTO>> GetAllItemsAsync(int fK_GroupId, int fK_CategoryId, int fK_SubCategoryId)
        {
            string url = $"Item/DDL";
            var requestBody = new
            {
                fK_GroupId= fK_GroupId,
                fK_CategoryId= fK_CategoryId,
                fK_SubCategoryId= fK_SubCategoryId
             };
            
            return await SendRequest<List<ItemDTO>>(url, HttpMethod.Post, requestBody, requireAuth: false);

        }
        public async Task<List<ItemDTO>> GetItemsWithStockAndLocation(int fK_GroupId, int fK_CategoryId, int fK_SubCategoryId)
        {
            string url = $"api/InventoryTransaction/GetItemsWithStockAndLocation";
            var requestBody = new
            {
                groupId = fK_GroupId,
                categoryId = fK_CategoryId,
                subCategoryId = fK_SubCategoryId,
                itemId=0,
                warehouseId= 0,
                locatorId= 0,
                companyId= 0,
                branchId= 0,
                statusCsv= "",
                pageNumber= 1,
                pageSize= 60
            };
            
            return await SendRequest<List<ItemDTO>>(url, HttpMethod.Post, requestBody, requireAuth: false);

        }

        public async Task<List<GroupDTO>> GetAllGroupsAsync()
        {
            string url = $"api/Group/GetAllDDL";

            return await SendRequest<List<GroupDTO>>(url, HttpMethod.Post, null, requireAuth: false);

        }

        public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
        {
            string url = $"api/Category/GetAllDDL";

            return await SendRequest<List<CategoryDTO>>(url, HttpMethod.Post, null, requireAuth: false);

        }

        public async Task<List<SubCategoryDTO>> GetAllSubCategoriesAsync()
        {
            string url = $"api/SubCategory/GetAllDDL";

            return await SendRequest<List<SubCategoryDTO>>(url, HttpMethod.Post, null, requireAuth: false);

        }

        public async Task<ItemDTO> GetItemByIdAsync(int id)
        {
            string url = $"Item/{id}";

            return await SendRequest<ItemDTO>(url, HttpMethod.Get, null, requireAuth: false);

        }

        public async Task<List<UnitDTO>> GetAllUnitDDL()
        {
            string url = $"api/Unit/GetAllDDL";
            return await SendRequest<List<UnitDTO>>(url, HttpMethod.Post, null, requireAuth: false);

        }
       
        public async Task<List<WarehouseDTO>> GetAllWarehousesDDL(bool? islocator, int transTypeId)
        {
            string url = $"Warehouse/GetAllDDL?islocator={islocator}&transactionTypeId={transTypeId}";
            return await SendRequest<List<WarehouseDTO>>(url, HttpMethod.Get, null, requireAuth: false);

        }

        public async Task<List<AvailableLocatorQtyDTO>> GetAvailableLocatorsDDL(FilterLocatorDTO dto)
        {
            string url = $"api/InventoryTransaction/available-locators-qty";
            return await SendRequest<List<AvailableLocatorQtyDTO>>(url, HttpMethod.Post, dto, requireAuth: false);

        }

        public async Task<int> CreateInventoryTransaction(CreateInventoryTransactionDTO dto)
        {
            string url = $"api/InventoryTransaction/Create";
            return await SendRequest<int>(url, HttpMethod.Post, dto, requireAuth: false);

        }

        public async Task<GetWarehouseDTO> GetWarehouseByIdAsync( int Id)
        {
            string url = $"Warehouse/{Id}";
            return await SendRequest<GetWarehouseDTO>(url, HttpMethod.Get, null, requireAuth: false);

        }

        public async Task<string> GRNAdd(CreateInventoryTransactionDTO grnObj) //Aya WS Service
        {
            using var form = new MultipartFormDataContent();

            // Add file
            if (grnObj.AttachmentFile != null)
            {
                var streamContent = new StreamContent(grnObj.AttachmentFile.OpenReadStream());
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(grnObj.AttachmentFile.ContentType);
                form.Add(streamContent, "AttachmentFile", grnObj.AttachmentFile.FileName);
            }

            
            if (grnObj.Details != null && grnObj.Details.Any())
            {
                grnObj.Details = grnObj.Details
                .GroupBy(d => new { d.FK_ItemId, d.FK_UnitId, d.KeyId })
                .Select(g => g.First())
                .ToList(); 
                for (int i = 0; i < grnObj.Details.Count; i++)
                {
                    var detail = grnObj.Details[i];

                    if (detail.FK_ItemId.HasValue)
                        form.Add(new StringContent(detail.FK_ItemId.Value.ToString()), $"Details[{i}].FK_ItemId");

                    if (detail.FK_UnitId.HasValue)
                        form.Add(new StringContent(detail.FK_UnitId.Value.ToString()), $"Details[{i}].FK_UnitId");

                    if (!string.IsNullOrEmpty(detail.KeyId))
                        form.Add(new StringContent(detail.KeyId), $"Details[{i}].KeyId");

                    if (detail.Quantity.HasValue)
                        form.Add(new StringContent(detail.Quantity.Value.ToString()), $"Details[{i}].Quantity");

                    if (detail.UnitQuantity.HasValue)
                        form.Add(new StringContent(detail.UnitQuantity.Value.ToString()), $"Details[{i}].UnitQuantity");

                    if (detail.Price.HasValue)
                        form.Add(new StringContent(detail.Price.Value.ToString()), $"Details[{i}].Price");

                    if (detail.Total.HasValue)
                        form.Add(new StringContent(detail.Total.Value.ToString()), $"Details[{i}].Total");

                    if (detail.FK_LocatorId.HasValue)
                        form.Add(new StringContent(detail.FK_LocatorId.Value.ToString()), $"Details[{i}].FK_LocatorId");

                    if (!string.IsNullOrEmpty(detail.Description))
                        form.Add(new StringContent(detail.Description), $"Details[{i}].Description");

                    if (!string.IsNullOrEmpty(detail.Serial))
                        form.Add(new StringContent(detail.Serial), $"Details[{i}].Serial");

                    if (!string.IsNullOrEmpty(detail.Batch))
                        form.Add(new StringContent(detail.Batch), $"Details[{i}].Batch");

                    if (detail.ExpiryDate.HasValue)
                        form.Add(new StringContent(detail.ExpiryDate.Value.ToString("o")), $"Details[{i}].ExpiryDate"); // ISO 8601 format
                }
            }

            //3-9
            // Add other simple properties
            form.Add(new StringContent(grnObj.TransactionDate.ToString("o")), "TransactionDate"); // ISO 8601

            if (grnObj.TransactionReferenceNo.HasValue)
                form.Add(new StringContent(grnObj.TransactionReferenceNo.Value.ToString()), "TransactionReferenceNo");

            if (grnObj.FK_TransactionReferenceTypeId.HasValue)
                form.Add(new StringContent(grnObj.FK_TransactionReferenceTypeId.Value.ToString()), "FK_TransactionReferenceTypeId");

            if (grnObj.FK_WarehouseId.HasValue)
                form.Add(new StringContent(grnObj.FK_WarehouseId.Value.ToString()), "FK_WarehouseId");

            form.Add(new StringContent(grnObj.FK_TransactionTypeId.ToString()), "FK_TransactionTypeId");

            form.Add(new StringContent(grnObj.FK_TransactionStatusId.ToString()), "FK_TransactionStatusId");

            form.Add(new StringContent(grnObj.Description ?? ""), "Description");

            if (grnObj.CompanyId.HasValue)
                form.Add(new StringContent(grnObj.CreatedBy.Value.ToString()), "CreatedBy");

            if (grnObj.CompanyId.HasValue)
                form.Add(new StringContent(grnObj.CompanyId.Value.ToString()), "CompanyId");

            if (grnObj.BranchId.HasValue)
                form.Add(new StringContent(grnObj.BranchId.Value.ToString()), "BranchId");

            if (grnObj.FK_FromWarehouseId.HasValue)
                form.Add(new StringContent(grnObj.FK_FromWarehouseId.Value.ToString()), "FK_FromWarehouseId");

            if (grnObj.FK_ToWarehouseId.HasValue)
                form.Add(new StringContent(grnObj.FK_ToWarehouseId.Value.ToString()), "FK_ToWarehouseId");

            if (grnObj.Fk_FinancialTransactionMasterId.HasValue)
                form.Add(new StringContent(grnObj.Fk_FinancialTransactionMasterId.Value.ToString()), "Fk_FinancialTransactionMasterId");

            if (grnObj.FinancialTransactionNo.HasValue)
                form.Add(new StringContent(grnObj.FinancialTransactionNo.Value.ToString()), "FinancialTransactionNo");

            if (grnObj.FinancialTransactionTypeNo.HasValue)
                form.Add(new StringContent(grnObj.FinancialTransactionTypeNo.Value.ToString()), "FinancialTransactionTypeNo");

            if (grnObj.Fk_InvoiceType.HasValue)
                form.Add(new StringContent(grnObj.Fk_InvoiceType.Value.ToString()), "Fk_InvoiceType");

            if (grnObj.StockType.HasValue)
                form.Add(new StringContent(grnObj.StockType.Value.ToString()), "StockType");

            form.Add(new StringContent(grnObj.AttachmentPath ?? ""), "AttachmentPath");



            // Post to API
            var response = await _httpClient.PostAsync("api/InventoryTransaction/Create", form);

            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }

        //public async Task<string> GetAllGRNByIdHead(long invObj)
        //{
        //    //var content = new StringContent(invObj.ToString(), Encoding.UTF8, "application/json");
        //    string url = $"api/InventoryTransaction/GetAllByIdHead";
        //    return await SendRequest<string>(url, HttpMethod.Post, invObj, requireAuth: false);
        //}

        public async Task<string> GetAllGRNByIdHead(long invObj)
        {
            var content = new StringContent(invObj.ToString(), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/InventoryTransaction/GetAllByIdHead", content);
            var responseData = await response.Content.ReadAsStringAsync();
            return responseData;
        }

        public async Task<InventoryAccountDefinitionsDTO> GetInventoryAccountDefinitions()
        {
            string url = $"AccountDefinitions";
            return await SendRequest<InventoryAccountDefinitionsDTO>(url, HttpMethod.Get, null, requireAuth: false);
        }

        public async Task<int> InventoryTransactionDelete(DeleteInventoryTransactionDTO dto)
        {
            string url = $"api/InventoryTransaction/Delete";
            return await SendRequest<int>(url, HttpMethod.Patch, dto, requireAuth: false);
        }

        
    }
}
