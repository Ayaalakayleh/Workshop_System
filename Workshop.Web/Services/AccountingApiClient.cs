using Newtonsoft.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Models;
using System.Text;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using DocumentFormat.OpenXml.Drawing.Diagrams;

namespace Workshop.Web.Services
{
    public class AccountingApiClient : BaseApiClient
    {
        public AccountingApiClient(
        HttpClient httpClient,
            IConfiguration config,
            IApiAuthStrategy apiAuthStrategy
            ) : base(
                httpClient,
                new ApiSettings
                {
                    ApiUser = config.GetValue<string>("ApiSettings:AccountingApiUser"),
                    ApiPassword = config.GetValue<string>("ApiSettings:AccountingApiPassword"),

                }, apiAuthStrategy)
        { }

        public async Task<List<TaxClassification>> GetTaxClassificationListByCompanyIdAndBranchId(int companyId, int branchId, string lang = "en")
        {

            string url = $"/TaxClassification/TaxClassificationGet?CompanyId={companyId}&BranchId={branchId}&language={lang}";
            return await SendRequest<List<TaxClassification>>(url, HttpMethod.Get);
        }

        public async Task<List<AccountTable>> ChartOfAccountAcceptTransByCompanyIdAndBranchId(int companyId, int branchId, string lang = "en")
        {

            string url = $"/ChartOfAccount/ChartOfAccountAcceptTrans?CompanyId={companyId}&BranchId={branchId}&language={lang}";
            return await SendRequest<List<AccountTable>>(url, HttpMethod.Get);
        }

        public async Task<List<CustomerInformation>> M_GetSuppliers(int companyId, int branchId, int isCompanyCenterialized = 1, string language = "en")
        {

            string url = $"/Customers/M_GetSuppliers?CompanyId={companyId}&BranchId={branchId}&IsCompanyCenterialized={isCompanyCenterialized}&Language={language}";
            return await SendRequest<List<CustomerInformation>>(url, HttpMethod.Get);
        }

        public async Task<List<TypeSalesPurchases>> TypeSalesPurchases_GetAll(int companyId, int branchId, int isCompanyCenterialized, int? invoiceType = null)
        {

            string url = $"/TypeSalesPurchases/TypeSalesPurchases_GetAll?CompanyId={companyId}&BranchId={branchId}&IsCompanyCenterialized={isCompanyCenterialized}&InvoiceType={invoiceType}";
            return await SendRequest<List<TypeSalesPurchases>>(url, HttpMethod.Get);
        }

        public async Task<CustomerInformation> Supplier_Find(int id)
        {

            string url = $"/Customers/Customer_Find?Id={id}";
            return await SendRequest<CustomerInformation>(url, HttpMethod.Get);
        }

        public async Task<bool> AccountSalesMaster_IsValidSupplierInvoiceNo(Int64 supplierId, string invoiceNo)
        {
            try
            {

                string url = $"/SalesInvoice/AccountSalesMaster_IsValidSupplierInvoiceNo?SupplierId={supplierId}&SupplierInvoiceNo={invoiceNo}";
                return await SendRequest<bool>(url, HttpMethod.Get);
            }
            catch (Exception e) {
                return new bool();
            }
        }

        public async Task<List<string>> AccountSalesMaster_IsValidSupplierInvoicesNo(ValidSupplierInvoicesNo validSupplierInvoicesNo)
        {
            try
            {
                List<string> result = new List<string>();

                string url = $"/SalesInvoice/AccountSalesMaster_IsValidSupplierInvoicesNo";
                var request = await SendRequest<string>(url, HttpMethod.Post, validSupplierInvoicesNo);
                return JsonConvert.DeserializeObject<List<string>>(request);
            }
            catch (Exception e) {
                return new List<string>();
            }
        }
        public async Task<List<Item>> GetItemsByCategoryNo(int categoryno, string lang = "en")
        {

            string url = $"/Items/Get_Items_ByCategoryNumber?categoryno={categoryno}&Language={lang}";
            return await SendRequest<List<Item>>(url, HttpMethod.Get);
        }

        public async Task<TypeSalesPurchases> TypeSalesPurchases_GetById(int id)
        {

            string url = $"/TypeSalesPurchases/TypeSalesPurchases_GetById?Id={id}";
            return await SendRequest<TypeSalesPurchases>(url, HttpMethod.Get);
        }

        public async Task<TaxClassification> GetTaxClassificationById(int id)
        {

            string url = $"/TaxClassification/M_TaxClassificationTable_Find?Id={id}";
            return await SendRequest<TaxClassification>(url, HttpMethod.Get);
        }

        public async Task<AccountSalesMaster> AccountSalesMaster_Insert(AccountSales data)
        {

            string url = $"/SalesInvoice/AccountSalesMaster_Insert";
            return await SendRequest<AccountSalesMaster>(url, HttpMethod.Post, data);
        }

        public async Task<List<CustomerInformation>> Customer_GetAll(int companyId, int branchId, int isCompanyCenterialized, string lang ="en")
        {

            string url = $"/Customers/Customers_GetAll?CompanyId={companyId}&BranchId={branchId}&IsCompanyCenterialized={isCompanyCenterialized}&Language={lang}";
            return await SendRequest<List<CustomerInformation>>(url, HttpMethod.Get);
        }

        public async Task<CustomerInformation> Customer_GetById(int Id)
        {

            string url = $"/Customers/Customer_Find?Id={Id}";
            return await SendRequest<CustomerInformation>(url, HttpMethod.Get);
        }

        //public async Task<TransactionMaster> SaveTransaction(TransactionMaster transactions)
        //{
        //    string url = $"/SalesInvoice/SaveTransaction";
        //    return await SendRequest<TransactionMaster>(url, HttpMethod.Post, transactions);
        //}

        public async Task<List<TransTypeTable>> TransactionType(int companyId, int branchId, string lang)
        {
            string url = $"/Transaction/TransType?CompanyId={companyId}&BranchId={branchId}&lang={lang}";
            return await SendRequest<List<TransTypeTable>>(url, HttpMethod.Get);
        }

        //=====================================================================================================
        public async Task<TransactionMaster?> SaveTransaction(VehicleDefinitions vehicle,List<AccountTable> accountDTO,AccountDefinitionDTO accountDefinitionDTO,int companyId,int branchId,int userId,
            int tranTypeNo,
            decimal? totalInternal=0,
            decimal? totalExternal=0,
            DateTime? transactionDate = null,
            string? notes = null,int? CurrencyId=null,string InternalType="")
        {
            try
            {            
                var WIPAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.WIPAccountId);
                var CostAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.InternalCostPartId);
                var ExternalCostAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.ExternalCostPartId);
                var MaintenanceAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.MaintenanceAccountId);
                var AccessoriesAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.AccessoriesAccountId);
                var AccidentAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.AccidentAccountId);
                var MaintenanceProjectsAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.MaintenanceProjectsAccountId);
                var InternalPartAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.InternalRevenuePartId);
                var InternalLabourAccount = accountDTO.FirstOrDefault(a => a.ID == accountDefinitionDTO.InternalRevenueLabourId);
                var transaction = new TransactionMaster
                {
                    TranTypeNo = tranTypeNo,
                    CurrencyID = (int)CurrencyId,
                    TranDate = transactionDate ?? DateTime.Now,
                    CreateBy = userId,
                    Notes = notes ?? "",
                    CompanyId = companyId,
                    BranchId = branchId,
                    oLTransactionDetails = new List<TransactionDetails>()
                };
                if (totalInternal >0)
                {
                    transaction.oLTransactionDetails.Add(new TransactionDetails
                    {
                        AccountNo = CostAccount.AccountNo,
                        KeyId = Guid.NewGuid().ToString(),
                        DAmount = (decimal)totalInternal,
                        CAmount = 0m,
                        Notes = notes ?? "",
                        Customer_DimensionsId = CostAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                        Vendor_DimensionsId = CostAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                        LOB_DimensionsId = CostAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                        Regions_DimensionsId = CostAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                        Locations_DimensionsId = CostAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                        Item_DimensionsId = CostAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                        Worker_DimensionsId = CostAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                        FixedAsset_DimensionsId = CostAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                        Department_DimensionsId = CostAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                        Contract_CC_DimensionsId = CostAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                        City_DimensionsId = CostAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                        D1_DimensionsId = CostAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                        D2_DimensionsId = CostAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                        D3_DimensionsId = CostAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                        D4_DimensionsId = CostAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                    });

                }
                if (totalExternal > 0)

                {
                    transaction.oLTransactionDetails.Add(new TransactionDetails
                    {
                        AccountNo = ExternalCostAccount.AccountNo,
                        KeyId = Guid.NewGuid().ToString(),
                        DAmount = (decimal)totalExternal,
                        CAmount = 0m,
                        Notes = notes ?? "",
                        Customer_DimensionsId = ExternalCostAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                        Vendor_DimensionsId = ExternalCostAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                        LOB_DimensionsId = ExternalCostAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                        Regions_DimensionsId = ExternalCostAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                        Locations_DimensionsId = ExternalCostAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                        Item_DimensionsId = ExternalCostAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                        Worker_DimensionsId = ExternalCostAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                        FixedAsset_DimensionsId = ExternalCostAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                        Department_DimensionsId = ExternalCostAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                        Contract_CC_DimensionsId = ExternalCostAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                        City_DimensionsId = ExternalCostAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                        D1_DimensionsId = ExternalCostAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                        D2_DimensionsId = ExternalCostAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                        D3_DimensionsId = ExternalCostAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                        D4_DimensionsId = ExternalCostAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                    });

                }
                // Debit entry

                // Credit entry
                transaction.oLTransactionDetails.Add(new TransactionDetails
                {
                    AccountNo = WIPAccount.AccountNo,
                    KeyId = Guid.NewGuid().ToString(),
                    DAmount = 0m,
                    CAmount = (decimal)totalInternal + (decimal)totalExternal,
                    Notes = notes ?? "",
                    Customer_DimensionsId = WIPAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                    Vendor_DimensionsId = WIPAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                    LOB_DimensionsId = WIPAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                    Regions_DimensionsId = WIPAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                    Locations_DimensionsId = WIPAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                    Item_DimensionsId = WIPAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                    Worker_DimensionsId = WIPAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                    FixedAsset_DimensionsId = WIPAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                    Department_DimensionsId = WIPAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                    Contract_CC_DimensionsId = WIPAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                    City_DimensionsId = WIPAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                    D1_DimensionsId = WIPAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                    D2_DimensionsId = WIPAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                    D3_DimensionsId = WIPAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                    D4_DimensionsId = WIPAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                });

                // Debit entry
                if (totalInternal > 0)
                {
                    if (InternalType =="1")
                    {
                        transaction.oLTransactionDetails.Add(new TransactionDetails
                        {
                            AccountNo = MaintenanceAccount.AccountNo,
                            KeyId = Guid.NewGuid().ToString(),
                            DAmount = (decimal)totalInternal,
                            CAmount = 0m,
                            Notes = notes ?? "",
                            Customer_DimensionsId = MaintenanceAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                            Vendor_DimensionsId = MaintenanceAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                            LOB_DimensionsId = MaintenanceAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                            Regions_DimensionsId = MaintenanceAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                            Locations_DimensionsId = MaintenanceAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                            Item_DimensionsId = MaintenanceAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                            Worker_DimensionsId = MaintenanceAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                            FixedAsset_DimensionsId = MaintenanceAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                            Department_DimensionsId = MaintenanceAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                            Contract_CC_DimensionsId = MaintenanceAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                            City_DimensionsId = MaintenanceAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                            D1_DimensionsId = MaintenanceAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                            D2_DimensionsId = MaintenanceAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                            D3_DimensionsId = MaintenanceAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                            D4_DimensionsId = MaintenanceAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                        });

                    }
                    else if (InternalType == "2")
                        {
                        transaction.oLTransactionDetails.Add(new TransactionDetails
                        {
                            AccountNo = AccessoriesAccount.AccountNo,
                            KeyId = Guid.NewGuid().ToString(),
                            DAmount = (decimal)totalInternal,
                            CAmount = 0m,
                            Notes = notes ?? "",
                            Customer_DimensionsId = AccessoriesAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                            Vendor_DimensionsId = AccessoriesAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                            LOB_DimensionsId = AccessoriesAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                            Regions_DimensionsId = AccessoriesAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                            Locations_DimensionsId = AccessoriesAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                            Item_DimensionsId = AccessoriesAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                            Worker_DimensionsId = AccessoriesAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                            FixedAsset_DimensionsId = AccessoriesAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                            Department_DimensionsId = AccessoriesAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                            Contract_CC_DimensionsId = AccessoriesAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                            City_DimensionsId = AccessoriesAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                            D1_DimensionsId = AccessoriesAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                            D2_DimensionsId = AccessoriesAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                            D3_DimensionsId = AccessoriesAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                            D4_DimensionsId = AccessoriesAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                        });

                    }
                    else if (InternalType == "3")
                    {
                        transaction.oLTransactionDetails.Add(new TransactionDetails
                        {
                            AccountNo = AccessoriesAccount.AccountNo,
                            KeyId = Guid.NewGuid().ToString(),
                            DAmount = (decimal)totalInternal,
                            CAmount = 0m,
                            Notes = notes ?? "",
                            Customer_DimensionsId = AccessoriesAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                            Vendor_DimensionsId = AccessoriesAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                            LOB_DimensionsId = AccessoriesAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                            Regions_DimensionsId = AccessoriesAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                            Locations_DimensionsId = AccessoriesAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                            Item_DimensionsId = AccessoriesAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                            Worker_DimensionsId = AccessoriesAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                            FixedAsset_DimensionsId = AccessoriesAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                            Department_DimensionsId = AccessoriesAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                            Contract_CC_DimensionsId = AccessoriesAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                            City_DimensionsId = AccessoriesAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                            D1_DimensionsId = AccessoriesAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                            D2_DimensionsId = AccessoriesAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                            D3_DimensionsId = AccessoriesAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                            D4_DimensionsId = AccessoriesAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                        });

                    }
                    else if (InternalType == "4")
                    {
                        transaction.oLTransactionDetails.Add(new TransactionDetails
                        {
                            AccountNo = MaintenanceProjectsAccount.AccountNo,
                            KeyId = Guid.NewGuid().ToString(),
                            DAmount = (decimal)totalInternal,
                            CAmount = 0m,
                            Notes = notes ?? "",
                            Customer_DimensionsId = MaintenanceProjectsAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                            Vendor_DimensionsId = MaintenanceProjectsAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                            LOB_DimensionsId = MaintenanceProjectsAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                            Regions_DimensionsId = MaintenanceProjectsAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                            Locations_DimensionsId = MaintenanceProjectsAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                            Item_DimensionsId = MaintenanceProjectsAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                            Worker_DimensionsId = MaintenanceProjectsAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                            FixedAsset_DimensionsId = MaintenanceProjectsAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                            Department_DimensionsId = MaintenanceProjectsAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                            Contract_CC_DimensionsId = MaintenanceProjectsAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                            City_DimensionsId = MaintenanceProjectsAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                            D1_DimensionsId = MaintenanceProjectsAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                            D2_DimensionsId = MaintenanceProjectsAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                            D3_DimensionsId = MaintenanceProjectsAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                            D4_DimensionsId = MaintenanceProjectsAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                        });

                    }

                    // Credit entry
                    transaction.oLTransactionDetails.Add(new TransactionDetails
                    {
                        AccountNo = InternalPartAccount.AccountNo,
                        KeyId = Guid.NewGuid().ToString(),
                        DAmount = 0m,
                        CAmount =(decimal) totalInternal,
                        Notes = notes ?? "",
                        Customer_DimensionsId = InternalPartAccount.IsCustomer_Dimensions ? vehicle.Customer_DimensionsId : null,
                        Vendor_DimensionsId = InternalPartAccount.IsVendor_Dimensions ? vehicle.Vendor_DimensionsId : null,
                        LOB_DimensionsId = InternalPartAccount.IsLOB_Dimensions ? vehicle.LOB_DimensionsId : null,
                        Regions_DimensionsId = InternalPartAccount.IsRegions_Dimensions ? vehicle.Regions_DimensionsId : null,
                        Locations_DimensionsId = InternalPartAccount.IsLocations_Dimensions ? vehicle.Locations_DimensionsId : null,
                        Item_DimensionsId = InternalPartAccount.IsItem_Dimensions ? vehicle.Item_DimensionsId : null,
                        Worker_DimensionsId = InternalPartAccount.IsWorker_Dimensions ? vehicle.Worker_DimensionsId : null,
                        FixedAsset_DimensionsId = InternalPartAccount.IsFixedAsset_Dimensions ? vehicle.FixedAsset_DimensionsId : null,
                        Department_DimensionsId = InternalPartAccount.IsDepartment_Dimensions ? vehicle.Department_DimensionsId : null,
                        Contract_CC_DimensionsId = InternalPartAccount.IsContract_CC_Dimensions ? vehicle.Contract_CC_DimensionsId : null,
                        City_DimensionsId = InternalPartAccount.IsCity_Dimensions ? vehicle.City_DimensionsId : null,
                        D1_DimensionsId = InternalPartAccount.IsD1_Dimensions ? vehicle.D1_DimensionsId : null,
                        D2_DimensionsId = InternalPartAccount.IsD2_Dimensions ? vehicle.D2_DimensionsId : null,
                        D3_DimensionsId = InternalPartAccount.IsD3_Dimensions ? vehicle.D3_DimensionsId : null,
                        D4_DimensionsId = InternalPartAccount.IsD4_Dimensions ? vehicle.D4_DimensionsId : null,
                    });
                }



                var saveTeansURL = $"/Transaction/SaveTransaction";
                var request = await SendRequest<TransactionMaster>(saveTeansURL, HttpMethod.Post, transaction);

                return request ?? new TransactionMaster();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error saving financial transaction: {Message}", ex.Message);
                return null;
            }
        }
        public async Task<TransactionMaster?> ReverseTrans(TransactionMaster transactionMaster)
        {
            try
            {

                var saveTeansURL = $"/Transaction/ReverseTransaction";
                var request = await SendRequest<TransactionMaster>(saveTeansURL, HttpMethod.Post, transactionMaster);

                return request ?? new TransactionMaster();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error saving financial transaction: {Message}", ex.Message);
                return null;
            }
        }
        public async Task<List<DBPaymentType>> PaymentTerms_Get(int companyId, int branchId)
        {
            string url = $"/PaymentTerms/PaymentTerms_Get?CompanyId={companyId}&BranchId={branchId}";
            return await SendRequest<List<DBPaymentType>>(url, HttpMethod.Get);
        }

        //Saves a financial transaction with one master and two detail records(debit and credit entries)
        public async Task<TransactionMaster?> SaveIssueTransaction(
             int tranTypeNo,
             decimal amount,
             string debitAccountNo,
             int companyId,
             int branchId,
             int userId,
             string creditAccountNo,
             DateTime? transactionDate = null,
             string? notes = null, int? CurrencyId = null,
             string? imagePath = null)
        {
            try
            {
                var transaction = new TransactionMaster
                {
                    TranTypeNo = tranTypeNo,
                    CurrencyID = (int)CurrencyId,
                    TranDate = transactionDate ?? DateTime.Now,
                    CreateBy = userId,
                    Notes = notes ?? "",
                    CompanyId = companyId,
                    BranchId = branchId,
                    ImageName = imagePath ?? "",
                    oLTransactionDetails = new List<TransactionDetails>()
                };

                // Debit entry
                transaction.oLTransactionDetails.Add(new TransactionDetails
                {
                    AccountNo = debitAccountNo,
                    KeyId = Guid.NewGuid().ToString(),
                    DAmount = amount,
                    CAmount = 0m,
                    Notes = notes ?? ""
                });

                // Credit entry
                transaction.oLTransactionDetails.Add(new TransactionDetails
                {
                    AccountNo = creditAccountNo,
                    KeyId = Guid.NewGuid().ToString(),
                    DAmount = 0m,
                    CAmount = amount,
                    Notes = notes ?? ""
                });

                //var request = await CreateAuthorizedRequestAsync(
                //    HttpMethod.Post,
                //    "Transaction/SaveTransaction");

                //request.Content = new StringContent(
                //    JsonSerializer.Serialize(transaction, _jsonOptions),
                //    Encoding.UTF8,
                //    "application/json");

                //using var response = await _httpClient.SendAsync(request);
                //var responseContent = await response.Content.ReadAsStringAsync();


                var saveTeansURL = $"/Transaction/SaveTransaction";
                var request = await SendRequest<TransactionMaster>(saveTeansURL, HttpMethod.Post, transaction);

                
                return request ?? new TransactionMaster();

                //return envelope.ResponseDetails; // <-- your FinancialTransactionMaster

            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error saving financial transaction: {Message}", ex.Message);
                return null;
            }
        }


        public async Task<TransactionMaster?> ReverseTransactionAsync(int transactionMasterId, int tranTypeNo, int companyId, int branchId, int userId)
        {
            try
            {
                var reverseTrans = new TransactionMaster
                {
                    ID = transactionMasterId,
                    TranTypeNo = tranTypeNo,
                    TranTypeNoReverse = tranTypeNo,
                    TranDate = DateTime.Now,
                    CreateBy = userId,
                    CompanyId = companyId,
                    BranchId = branchId,
                    IsCompanyCenterialized = 0,
                    VoucherType = 1                 // قيد
                };
                string url = $"/Transaction/ReverseTransaction";
                return await SendRequest<TransactionMaster>(url, HttpMethod.Post, reverseTrans);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error reversing financial transaction: {Message}", ex.Message);
                return null;
            }
        }

    }
}
