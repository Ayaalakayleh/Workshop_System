using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Infrastructure;
using Workshop.Web.Controllers;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Controllers
{
    [SessionTimeout]

    public class ExternalWorkshopInvoiceController : BaseController
    {
        private readonly WorkshopApiClient _workshopApiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _erpAPIClient;
        private readonly IFileService _fileService;
        private readonly IFileValidationService _fileValidationService;
        private readonly ILogger<TechnicianDashboardController> _logger;

        private string lang;//LanguageController.GetCurrentLanguage();
        private const string SessionKeyMainClockingModel = "ExternalInvoiceBranchModel_v1";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public ExternalWorkshopInvoiceController(
            WorkshopApiClient workshopApiClient,
            AccountingApiClient accountingApiClient,
            VehicleApiClient vehicleApiClient,
            ERPApiClient erpAPIClient,
            IFileService fileService,
            IFileValidationService fileValidationService,
            ILogger<TechnicianDashboardController> logger,
            IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _workshopApiClient = workshopApiClient;
            _accountingApiClient = accountingApiClient;
            _vehicleApiClient = vehicleApiClient;
            _erpAPIClient = erpAPIClient;
            _fileService = fileService;
            _fileValidationService = fileValidationService;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.ExternalWorkshopInvoice.View)]
        public async Task<IActionResult> Index()
        {

            ExternalWorkshopInvoiceDTO externalWorkshopInvoice = new ExternalWorkshopInvoiceDTO();

            ViewBag.WorkshopDefinitions = await GetWorkshopDefinitions();
            ViewBag.InvoiceTypes = await GetInvoiceTypes();

            return View(externalWorkshopInvoice);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.ExternalWorkshopInvoice.Create)]
        public async Task<IActionResult> Edit(ExternalWorkshopInvoiceDTO externalWorkshopInvoice)
        {

            var workshopDetails = await _workshopApiClient.GetWorkshopByIdAsync(externalWorkshopInvoice.ExternalWorkshopId);

            if (workshopDetails.VatClassificationId.HasValue)
            {
                externalWorkshopInvoice.VatRate = (await _accountingApiClient.GetTaxClassificationById(workshopDetails.VatClassificationId.Value)).TaxRate;
            }

            externalWorkshopInvoice.ExternalWorkshopInvoiceDetails = await _workshopApiClient.GetExternalWorkshopInvoiceDetailsAsync(new ExternalWorkshopInvoiceDetailsFilterDTO { WorkshopId = workshopDetails.Id, FromDate = externalWorkshopInvoice.GregorianFromDate, ToDate = externalWorkshopInvoice.GregorianToDate, ExternalWorkshopId = externalWorkshopInvoice.ExternalWorkshopId });

            var branches = await _erpAPIClient.GetActiveBranchesByCompanyId(CompanyId);
            foreach (var item in externalWorkshopInvoice?.ExternalWorkshopInvoiceDetails)
            {
                item.BranchName = branches.Where(b => b.BranchID == item.BranchId).Select(b=> lang == "en" ? b.BranchPrimaryName : b.BranchSecondaryName).FirstOrDefault();
            }

            return PartialView("_ExternalWorkshopInvoice", externalWorkshopInvoice);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.ExternalWorkshopInvoice.Create)]
        public async Task<IActionResult> Save(ExternalWorkshopInvoiceDTO externalWorkshopInvoice, List<IFormFile> files)
        {
            var result = new TempData();
            int BranchId = externalWorkshopInvoice.BranchId;// int.Parse(Session["branch"].ToString());
            try
            {
                MovementInvoice invoice = new MovementInvoice();
                AccountSales oAccountSales = new AccountSales();
                oAccountSales.AccountSalesDetails = new List<AccountSalesDetails>();
                oAccountSales.AccountSalesMaster = new AccountSalesMaster();
                var oAccountSalesDetails = new AccountSalesDetails();
                var ExternalWorkshop = await _workshopApiClient.GetWorkshopByIdAsync(externalWorkshopInvoice.ExternalWorkshopId);
                var Supplier = new CustomerInformation();
                if (ExternalWorkshop != null && ExternalWorkshop.SupplierId != null)
                    Supplier = await _accountingApiClient.Supplier_Find(ExternalWorkshop.SupplierId.Value);
                var VehicleDetails = new VehicleDefinitions();
                var items = new List<Item>();
                items = await _accountingApiClient.GetItemsByCategoryNo(-1, "en"/*LanguageController.GetCurrentLanguage()*/);
                var InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetById(externalWorkshopInvoice.InvoiceTypeId);
                var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, lang);
                int? accountId = null;
                var accountTable = new AccountTable();

                //Crashed here
                string Invoices = string.Join(",", externalWorkshopInvoice.ExternalWorkshopInvoiceDetails.Select(x => x.InvoiceNo));
                bool isExist = await _accountingApiClient.AccountSalesMaster_IsValidSupplierInvoiceNo(Supplier.Id, Invoices);
                if (!isExist)
                {
                    result.IsSuccess = false;
                    result.Message = "InvoiceNo " + Invoices + " AlreadyExist";
                    return Json(result);
                }
                var ValidSupplierInvoicesNo = new ValidSupplierInvoicesNo()
                {
                    SupplierId = Supplier.Id,
                    Invoices = Invoices,
                };
                List<string> data = await _accountingApiClient.AccountSalesMaster_IsValidSupplierInvoicesNo(ValidSupplierInvoicesNo);
                if (data.Count > 0)
                {
                    result.IsSuccess = false;
                    result.Message = "InvoiceNo" + " " + string.Join(", ", data) + " " + "AlreadyExist";
                    return Json(result);
                }
                for (int i = 0; i < externalWorkshopInvoice.ExternalWorkshopInvoiceDetails.Count; i++)
                {
                    var item = externalWorkshopInvoice.ExternalWorkshopInvoiceDetails[i];
                    var file = files[i];

                    var validationResult = _fileValidationService.CheckFileTypeAndSize(file);
                    if (validationResult.IsSuccess)
                    {

                        if (file != null && file.Length > 0)
                        {
                            var (filePath, fileName) = await _fileService.SaveFileAsync(file, "ExternalWorkshopInvoice");

                            invoice.FileName = fileName;
                            invoice.FilePath = filePath;
                            invoice.MovementId = item.MovementId;
                            invoice.WorkOrderId = item.WorkOrderId;
                            await _workshopApiClient.DExternalWorkshopInvoiceInsertAsync(invoice);
                            invoice = new MovementInvoice();
                            invoice.MovementId = item.MovementId;
                            invoice.MasterId = item.MasterId;
                            invoice.ExternalWorkshopId = Convert.ToInt32(item.ExternalWorkshopId);
                            invoice.InvoiceNo = item.InvoiceNo;
                            invoice.TotalInvoice = Convert.ToDecimal(item.TotalInvoice);
                            invoice.WorkOrderId = item.WorkOrderId;
                            invoice.DeductibleAmount = 0;
                            invoice.ConsumptionValueOfSpareParts = 0;
                            invoice.Vat = item.Vat;
                            invoice.PartsCost = item.PartsCost;
                            invoice.LaborCost = item.LaborCost;
                            invoice.Invoice_Date = externalWorkshopInvoice.GregorianToDate;

                            await _workshopApiClient.WorkshopInvoiceInsertAsync(invoice);

                        }
                        oAccountSales.AccountSalesMaster = new AccountSalesMaster()
                        {

                            Total = invoice.LaborCost + invoice.PartsCost,
                            Net = invoice.TotalInvoice,
                            Final = invoice.LaborCost + invoice.PartsCost,
                            Tax = invoice.Vat,
                            AccSalesTypeNo = 8,
                            AccSalesDate = externalWorkshopInvoice.GregorianToDate,
                            InvoiceType = 2,
                            TypeSalesPurchasesID = externalWorkshopInvoice.InvoiceTypeId,
                            Notes = "Maintenance",
                            SupplierInvoiceNo = invoice.InvoiceNo,
                            CustomerId = (int)Supplier.Id,
                            Customer_DimensionsId = Supplier.Customer_DimensionsId,
                            Vendor_DimensionsId = Supplier.Vendor_DimensionsId,
                            LOB_DimensionsId = Supplier.LOB_DimensionsId,
                            Regions_DimensionsId = Supplier.Regions_DimensionsId,
                            Locations_DimensionsId = Supplier.Locations_DimensionsId,
                            Item_DimensionsId = Supplier.Item_DimensionsId,
                            Worker_DimensionsId = Supplier.Worker_DimensionsId,
                            FixedAsset_DimensionsId = Supplier.FixedAsset_DimensionsId,
                            Department_DimensionsId = Supplier.Department_DimensionsId,
                            Contract_CC_DimensionsId = Supplier.Contract_CC_DimensionsId,
                            City_DimensionsId = Supplier.City_DimensionsId,
                            D1_DimensionsId = Supplier.D1_DimensionsId,
                            D2_DimensionsId = Supplier.D2_DimensionsId,
                            D3_DimensionsId = Supplier.D3_DimensionsId,
                            D4_DimensionsId = Supplier.D4_DimensionsId,

                        };
                        if (AccountList != null && Supplier != null)
                        {
                            oAccountSales.AccountSalesMaster.CustomerAccountNo = AccountList.Where(x => x.ID == Supplier.AccountNoPayableId)?.FirstOrDefault()?.AccountNo;
                        }
                        VehicleDetails = await _vehicleApiClient.GetVehicleDetails(item.VehicleId, lang);

                        if (item.PartsCost > 0)
                        {
                            accountId = items.Where(a => a.ItemNumber == -1).FirstOrDefault()?.ItemSalesAccountId;
                            accountId = accountId == null ? InvoiceType.AccountId : accountId;
                            accountTable = new AccountTable();
                            accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                            oAccountSalesDetails = new AccountSalesDetails()
                            {
                                ItemNumber = items.Where(a => a.ItemNumber == -1).FirstOrDefault().ItemId,
                                UnitId = items.Where(a => a.ItemNumber == -1).FirstOrDefault().UnitId,
                                Discount = 0,
                                Description = "Maintenance Parts " + "( " + VehicleDetails.PlateNumber + " ) " + " صيانة قطع غيار",
                                Quantity = 1,
                                UnitQuantity = 1,
                                Price = item.PartsCost,
                                Total = item.PartsCost,
                                TaxValue = item.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100),
                                TaxClassificationId = items.Where(a => a.ItemNumber == -1).FirstOrDefault().TaxClassificationNo,
                                Final = item.PartsCost + (item.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100)),
                                CostsCentersNo = VehicleDetails.CostCenter,
                                Reference = VehicleDetails.PlateNumber,
                                Customer_DimensionsId = accountTable.IsCustomer_Dimensions ? VehicleDetails.Customer_DimensionsId : null,
                                Vendor_DimensionsId = accountTable.IsVendor_Dimensions ? VehicleDetails.Vendor_DimensionsId : null,
                                LOB_DimensionsId = accountTable.IsLOB_Dimensions ? VehicleDetails.LOB_DimensionsId : null,
                                Regions_DimensionsId = accountTable.IsRegions_Dimensions ? VehicleDetails.Regions_DimensionsId : null,
                                Locations_DimensionsId = accountTable.IsLocations_Dimensions ? VehicleDetails.Locations_DimensionsId : null,
                                Item_DimensionsId = accountTable.IsItem_Dimensions ? VehicleDetails.Item_DimensionsId : null,
                                Worker_DimensionsId = accountTable.IsWorker_Dimensions ? VehicleDetails.Worker_DimensionsId : null,
                                FixedAsset_DimensionsId = accountTable.IsFixedAsset_Dimensions ? VehicleDetails.FixedAsset_DimensionsId : null,
                                Department_DimensionsId = accountTable.IsDepartment_Dimensions ? VehicleDetails.Department_DimensionsId : null,
                                Contract_CC_DimensionsId = accountTable.IsContract_CC_Dimensions ? VehicleDetails.Contract_CC_DimensionsId : null,
                                City_DimensionsId = accountTable.IsCity_Dimensions ? VehicleDetails.City_DimensionsId : null,
                                D1_DimensionsId = accountTable.IsD1_Dimensions ? VehicleDetails.D1_DimensionsId : null,
                                D2_DimensionsId = accountTable.IsD2_Dimensions ? VehicleDetails.D2_DimensionsId : null,
                                D3_DimensionsId = accountTable.IsD3_Dimensions ? VehicleDetails.D3_DimensionsId : null,
                                D4_DimensionsId = accountTable.IsD4_Dimensions ? VehicleDetails.D4_DimensionsId : null,
                            };
                            oAccountSales.AccountSalesDetails.Add(oAccountSalesDetails);

                        }
                        if (item.LaborCost > 0)
                        {
                            accountId = items.Where(a => a.ItemNumber == -2).FirstOrDefault()?.ItemSalesAccountId;
                            accountId = accountId == null ? InvoiceType.AccountId : accountId;
                            accountTable = new AccountTable();
                            accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                            oAccountSalesDetails = new AccountSalesDetails()
                            {
                                ItemNumber = items.Where(a => a.ItemNumber == (item.Vat == 0 ? -2 : -1)).FirstOrDefault().ItemId,
                                UnitId = items.Where(a => a.ItemNumber == (item.Vat == 0 ? -2 : -1)).FirstOrDefault().UnitId,
                                Discount = 0,
                                Description = "Maintenance Labor " + "( " + VehicleDetails.PlateNumber + " ) " + " صيانة عمالة",
                                Quantity = 1,
                                UnitQuantity = 1,
                                Price = item.LaborCost,
                                Total = item.LaborCost,
                                TaxValue = item.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100),
                                TaxClassificationId = items.Where(a => a.ItemNumber == -2).FirstOrDefault().TaxClassificationNo,
                                Final = item.LaborCost + (item.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100)),
                                CostsCentersNo = VehicleDetails.CostCenter,
                                Reference = VehicleDetails.PlateNumber,
                                Customer_DimensionsId = accountTable.IsCustomer_Dimensions ? VehicleDetails.Customer_DimensionsId : null,
                                Vendor_DimensionsId = accountTable.IsVendor_Dimensions ? VehicleDetails.Vendor_DimensionsId : null,
                                LOB_DimensionsId = accountTable.IsLOB_Dimensions ? VehicleDetails.LOB_DimensionsId : null,
                                Regions_DimensionsId = accountTable.IsRegions_Dimensions ? VehicleDetails.Regions_DimensionsId : null,
                                Locations_DimensionsId = accountTable.IsLocations_Dimensions ? VehicleDetails.Locations_DimensionsId : null,
                                Item_DimensionsId = accountTable.IsItem_Dimensions ? VehicleDetails.Item_DimensionsId : null,
                                Worker_DimensionsId = accountTable.IsWorker_Dimensions ? VehicleDetails.Worker_DimensionsId : null,
                                FixedAsset_DimensionsId = accountTable.IsFixedAsset_Dimensions ? VehicleDetails.FixedAsset_DimensionsId : null,
                                Department_DimensionsId = accountTable.IsDepartment_Dimensions ? VehicleDetails.Department_DimensionsId : null,
                                Contract_CC_DimensionsId = accountTable.IsContract_CC_Dimensions ? VehicleDetails.Contract_CC_DimensionsId : null,
                                City_DimensionsId = accountTable.IsCity_Dimensions ? VehicleDetails.City_DimensionsId : null,
                                D1_DimensionsId = accountTable.IsD1_Dimensions ? VehicleDetails.D1_DimensionsId : null,
                                D2_DimensionsId = accountTable.IsD2_Dimensions ? VehicleDetails.D2_DimensionsId : null,
                                D3_DimensionsId = accountTable.IsD3_Dimensions ? VehicleDetails.D3_DimensionsId : null,
                                D4_DimensionsId = accountTable.IsD4_Dimensions ? VehicleDetails.D4_DimensionsId : null,
                            };
                            oAccountSales.AccountSalesDetails.Add(oAccountSalesDetails);

                        }
                        oAccountSales.AccountSalesMaster.UserId = UserId.ToString();//SessionManager.GetSessionUserInfo.UserID.ToString();
                        oAccountSales.AccountSalesMaster.CurrencyID = 1;//((CompanyInfo)Session["CompanyInfo"]).CurrencyIDH;
                        oAccountSales.AccountSalesMaster.AccSalesBranch = BranchId;
                        oAccountSales.AccountSalesMaster.PaymentTerms = Supplier.oLDBPaymentType > 0 ? Supplier.oLDBPaymentType : 0;
                        oAccountSales.CompanyId = CompanyId;
                        oAccountSales.BranchId = BranchId;
                        oAccountSales.AccountSalesMaster.InventoryAccountId = InvoiceType.AccountId;
                        oAccountSales.CompanyType = 1;
                        await _accountingApiClient.AccountSalesMaster_Insert(oAccountSales);

                    }

                }


                result.IsSuccess = true;
                return Json(result);
            }
            catch (Exception ex)
            {
                result.Message = "ErrorHappend";
                return Json(result);
            }
        }

        public async Task<IActionResult> GetDate(int Month)
        {
            int Year = DateTime.Now.Year;
            if (Month == 12 && DateTime.Now.Month == 1)
            {
                Year -= 1;
            }
            List<WorkShopDefinitionDTO> olworkshopDefinition = new List<WorkShopDefinitionDTO>();

            DateTime StartDate = new DateTime(Year, Month, 1);
            DateTime EndDate = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));
            DateTime FromDate = new DateTime(Year, Month, 1);
            DateTime ToDate = new DateTime(Year, Month, DateTime.DaysInMonth(Year, Month));

            olworkshopDefinition = await _workshopApiClient.GetExternalWorkshopInvoiceWorkshopDetailsAsync(new WorkShopFilterDTO { FromDate = FromDate, ToDate = ToDate, Language = lang });

            string startDate = StartDate.ToString("yyyy-MM-dd");
            string endDate = EndDate.ToString("yyyy-MM-dd");
            string fromDate = FromDate.ToString("dd-MM-yyyy");
            string toDate = ToDate.ToString("dd-MM-yyyy");

            return Json(new { success = true, startDate, endDate, fromDate, toDate, olworkshopDefinition });
        }

        #region ViewBag Data
        private async Task<List<SelectListItem>> GetWorkshopDefinitions()
        {
            var workshops = await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId);

            if (workshops == null || !workshops.Any())
            {
                return new List<SelectListItem>();
            }

            return workshops?.Select(w => new SelectListItem
            {
                Value = w.Id.ToString(),
                Text = lang == "en" ? w.PrimaryName : w.SecondaryName
            })?.ToList();
        }

        private async Task<List<SelectListItem>> GetInvoiceTypes()
        {
            var invoiceTypes = await _accountingApiClient.TypeSalesPurchases_GetAll(CompanyId, BranchId, 1, 2);
            if (invoiceTypes == null || !invoiceTypes.Any())
            {
                return new List<SelectListItem>();
            }
            return invoiceTypes?.Select(it => new SelectListItem
            {
                Value = it.Id.ToString(),
                Text = lang == "en" ? it.PrimaryName : it.SecondaryName
            })?.ToList();
        }
        [HttpGet]
        public async Task<List<SelectListItem>> GetBranchs()
        {
            return GetBranchsModel();
        }

        private List<SelectListItem> GetBranchsModel()
        {
            try
            {
                if (HttpContext == null)
                {
                    _logger.LogWarning("GetBranchsModel called outside of an HTTP context; returning new BranchModel.");
                    return new List<SelectListItem>();
                }

                var session = HttpContext.Session;
                if (session == null)
                {
                    _logger.LogWarning("Session is not available on HttpContext; returning new BranchModel.");
                    return new List<SelectListItem>();
                }

                var json = session.GetString(SessionKeyMainClockingModel);
                if (string.IsNullOrEmpty(json)) return new List<SelectListItem>();
                var model = JsonSerializer.Deserialize<List<SelectListItem>>(json, _jsonOptions);
                return model ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize Branch model from session, creating new one.");
                return new List<SelectListItem>();
            }
        }

        private void SaveBranchsModel(List<SelectListItem> model)
        {
            try
            {
                if (model == null) return;

                if (HttpContext == null)
                {
                    _logger.LogWarning("SaveBranchsModel called outside of an HTTP context; skipping save.");
                    return;
                }

                var session = HttpContext.Session;
                if (session == null)
                {
                    _logger.LogWarning("Session is not available on HttpContext; skipping save.");
                    return;
                }

                var json = JsonSerializer.Serialize(model, _jsonOptions);
                session.SetString(SessionKeyMainClockingModel, json);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to serialize Branch model to session.");
            }
        }

        #endregion




    }
}