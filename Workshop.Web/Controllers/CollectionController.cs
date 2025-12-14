using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Data;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.ExternalWorkshopExp;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;
using Formatting = Newtonsoft.Json.Formatting;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class CollectionController : BaseController
    {
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly WorkshopApiClient _workshopApiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly IFileService _fileService;
        private readonly IFileValidationService _fileValidationService;
        private readonly string lang;

        public CollectionController(
            VehicleApiClient vehicleApiClient,
            WorkshopApiClient workshopApiClient,
            AccountingApiClient accountingApiClient,
            IFileService fileService,
            IFileValidationService fileValidationService,
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _vehicleApiClient = vehicleApiClient;
            _workshopApiClient = workshopApiClient;
            _accountingApiClient = accountingApiClient;
            _fileService = fileService;
            _fileValidationService = fileValidationService;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.Collection.View)]
        public async Task<IActionResult> Index([FromQuery] ExternalWorkshopExpFilterDTO filter)
        {
            try
            {
                

                filter ??= new ExternalWorkshopExpFilterDTO();

                var _External_Workshop = await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId);
                ViewBag.ExternalWorkshopList = _External_Workshop.Select(r => new SelectListItem { Text = /*GetCurrentBilanguage(r.SecondaryName, r.PrimaryName)*/ lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();

                ViewBag.VehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);

                //ToDo: Caching
                //if (cache.Get(string.Format(CacheKeys.VehiclesDDL, language)) != null)
                //{
                //    oFilter.VehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL, language));
                //}
                //else
                //{
                //    oFilter.VehicleNams = VehicleApi.GetVehiclesDDL(language, CompanyId);
                //    cache.Set(string.Format(CacheKeys.VehiclesDDL, language), oFilter.VehicleNams, DateTimeOffset.Now.AddDays(10));
                //}

                List<MExternalWorkshopExpDTO> externalWorkshops = new List<MExternalWorkshopExpDTO>();
                filter.CompanyId = CompanyId;
                externalWorkshops = (await _workshopApiClient.GetExternalWorkshopExpAsync(filter))?.ToList();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_MaintenanceList", externalWorkshops);
                }

                return View(externalWorkshops);
            }
            catch (Exception ex)
            {
                //var error = new ApplicationLogModel()
                //{
                //    LayerName = typeof(VehicleApi).Name,
                //    FunctionName = string.Format("{0} : {1}", new StackTrace().GetFrame(1).GetMethod().Name, new StackTrace().GetFrame(0).GetMethod().Name),
                //    Parameters = new JavaScriptSerializer().Serialize(""),
                //    ExceptionError = ex.Message + " Stack Trace: " + ex.StackTrace
                //};

                //ToDo: Logging
                //WorkshopAPI.InsertApplicationLog(error);
                throw ex;
            }

        }

        [CustomAuthorize(Permissions.Collection.Create)]
        public async Task<IActionResult> Create()
        {
            MExternalWorkshopExpDTO data = new MExternalWorkshopExpDTO();
            try
            {
                
                


                var _External_Workshop = await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId);

                ExcelMappingFilterDTO filter = new ExcelMappingFilterDTO() { CompanyId = CompanyId, BranchId = BranchId };
                filter.CompanyId = CompanyId;
                var Excel_Mappin = (await _workshopApiClient.GetExcelMappingAsync(filter))?.Select(a => a.WorkshopId)?.ToList();
                Excel_Mappin ??= new List<int>();

                ViewBag.ExternalWorkshopList = _External_Workshop.Where(a => a.Id.HasValue && Excel_Mappin.Contains(a.Id.Value)).Select(r => new SelectListItem { Text = /*GetCurrentBilanguage(r.PrimaryName, r.SecondaryName)*/ lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();
                data.InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetAll(CompanyId, BranchId, 1, 2);

                return View(data);
            }
            catch (Exception ex)
            {
                return View(data);
            }

        }

        [HttpPost]
        [CustomAuthorize(Permissions.Collection.Create)]
        public async Task<IActionResult> Create(MExternalWorkshopExpDTO CreateExternal_Workshop)
        {
            CreateExternal_Workshop.Excel_Date = DateTime.Now;
            TempData result = new TempData();
            int workOrderId = 0;
            try
            {

                CreateExternal_Workshop.DExternalWorkshopExp = JsonConvert.DeserializeObject<List<DExternalWorkshopExpDTO>>(CreateExternal_Workshop.External_Workshop_ExpjsonList);
                CreateExternal_Workshop.CompanyId = CompanyId;
                CreateExternal_Workshop.BranchId = BranchId;
                CreateExternal_Workshop.CreatedBy = UserId;
                List<string> resultsuccess = new List<string>();
                List<string> resultfault = new List<string>();


                var NotAddedList = new List<DExternalWorkshopExpDTO>();
                var DExternalWorkshopExp = CreateExternal_Workshop.DExternalWorkshopExp;

                var VehicleDefinitions = new VehicleDefinitions();
                var ExternalWorkshop = await _workshopApiClient.GetWorkshopByIdAsync((int)CreateExternal_Workshop.ExternalWorkshopId);

                var Supplier = await _accountingApiClient.Supplier_Find(ExternalWorkshop.SupplierId.Value);
                string Invoices = string.Join(",", CreateExternal_Workshop.DExternalWorkshopExp?.Select(x => x.Invoice_No));
                bool isExist = await _accountingApiClient.AccountSalesMaster_IsValidSupplierInvoiceNo(Supplier.Id, Invoices);
                if (isExist)
                {
                    result.IsSuccess = false;

                    //ToDo: Localize
                    result.Message = $"InvoiceNo {Invoices} AlreadyExist";
                    return Json(result);
                }
                var items = new List<Item>();
                items = await _accountingApiClient.GetItemsByCategoryNo(-1, lang);
                VehicleDefinitions.CompanyId = CompanyId;
                var Vehicles = await _vehicleApiClient.GetVehiclesPlatesByCompanyId(CompanyId);
                decimal Vat;
                var InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetById(CreateExternal_Workshop.InvoiceTypeId);
                var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, lang);
                int? accountId = null;
                var accountTable = new AccountTable();
                var oVehicle = new VehicleDefinitions();
                try
                {
                    var VatList = await _accountingApiClient.GetTaxClassificationById(ExternalWorkshop.VatClassificationId.Value);
                    Vat = VatList.TaxRate;

                }
                catch (Exception)
                {
                    Vat = 0;
                }

                foreach (var item in CreateExternal_Workshop.DExternalWorkshopExp)
                {
                    var workOrder = new MWorkOrderDTO();
                    var MovementInvoice = new Core.DTOs.WorkshopMovement.MovementInvoice();
                    AccountSales oAccountSales = new AccountSales();
                    oAccountSales.AccountSalesDetails = new List<AccountSalesDetails>();
                    oAccountSales.AccountSalesMaster = new AccountSalesMaster();
                    var oAccountSalesDetails = new AccountSalesDetails();
                    try
                    {
                        //6464 - K D S 6464 - K D S
                        workOrder.VehicleId = oVehicle.Id;
                        workOrder.WorkOrderType = 2;
                        workOrder.GregorianDamageDate = item.Invoice_Date;
                        workOrder.IsFix = true;
                        workOrder.Description = item.Description;
                        workOrder.WorkOrderTitle = item.Invoice_No;
                        //workOrder.JobCardStatus = 4;
                        workOrder.WorkOrderStatus = 4;
                        workOrder.InvoicingStatus = 2;
                        workOrder.CreatedBy = UserId;
                        workOrder.CompanyId = CompanyId;
                        workOrder.BranchId = BranchId;
                        workOrderId = _workshopApiClient.InsertMWorkOrderAsync(workOrder).Id;
                        item.WorkOrderId = workOrderId;
                        item.VehicleId = workOrder.VehicleId;
                        MovementInvoice.WorkOrderId = workOrderId;
                        MovementInvoice.InvoiceNo = item.Invoice_No;
                        MovementInvoice.MovementId = 0;
                        MovementInvoice.TotalInvoice = item.Total ?? 0;
                        MovementInvoice.DeductibleAmount = 0;
                        MovementInvoice.ConsumptionValueOfSpareParts = 0;
                        MovementInvoice.Invoice_Date = item.Invoice_Date;
                        MovementInvoice.ExternalWorkshopId = CreateExternal_Workshop.ExternalWorkshopId ?? 0;
                        if (item.Service_Type.ToUpper() == "ITEMS")
                        {
                            MovementInvoice.Vat = (item.Quantity * item.Price * (Vat / 100)) ?? 0;
                            //check vat inclouded
                            MovementInvoice.PartsCost = item.Quantity * item.Price ?? 0;
                            MovementInvoice.LaborCost = 0;
                        }
                        else if (item.Service_Type.Replace(" ", "").ToUpper() == "PARTSANDSERVICES")
                        {
                            MovementInvoice.Vat = Math.Round((item.Total - (item.Total / (1 + (Vat / 100)))) ?? 0, 2);

                            MovementInvoice.PartsCost = (item.Total ?? 0) - MovementInvoice.Vat;
                            MovementInvoice.LaborCost = 0;
                        }
                        else
                        {
                            MovementInvoice.PartsCost = 0;
                            MovementInvoice.LaborCost = (item.Quantity * item.Price) ?? 0;
                        }

                        if (!await _workshopApiClient.WorkshopInvoiceInsertAsync(MovementInvoice))
                        {
                            throw new Exception();
                        }
                        else
                        {
                            // Insert Invoice

                            oVehicle = Vehicles.Where(a => a.PlateNumber.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper() == item.License_Plate_No.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper()).FirstOrDefault();
                            if (MovementInvoice.PartsCost > 0)
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
                                    Description = item.Description == null ? "Maintenance Parts " + "( " + oVehicle.PlateNumber + " ) " + " صيانة  قطع غيار" : item.Description,
                                    Quantity = item.Quantity == null || item.Quantity == 0 ? 1 : (int)item.Quantity,
                                    UnitQuantity = item.Quantity == null || item.Quantity == 0 ? 1 : (int)item.Quantity,
                                    Price = MovementInvoice.PartsCost,
                                    Total = MovementInvoice.PartsCost,
                                    TaxValue = MovementInvoice.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100),
                                    TaxClassificationId = items.Where(a => a.ItemNumber == -1).FirstOrDefault().TaxClassificationNo,
                                    Final = MovementInvoice.PartsCost + (MovementInvoice.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100)),
                                    CostsCentersNo = oVehicle.CostCenter,
                                    Reference = oVehicle.PlateNumber.Replace(" ", "").Replace("-", ""),
                                    Customer_DimensionsId = accountTable.IsCustomer_Dimensions ? oVehicle.Customer_DimensionsId : null,
                                    Vendor_DimensionsId = accountTable.IsVendor_Dimensions ? oVehicle.Vendor_DimensionsId : null,
                                    LOB_DimensionsId = accountTable.IsLOB_Dimensions ? oVehicle.LOB_DimensionsId : null,
                                    Regions_DimensionsId = accountTable.IsRegions_Dimensions ? oVehicle.Regions_DimensionsId : null,
                                    Locations_DimensionsId = accountTable.IsLocations_Dimensions ? oVehicle.Locations_DimensionsId : null,
                                    Item_DimensionsId = accountTable.IsItem_Dimensions ? oVehicle.Item_DimensionsId : null,
                                    Worker_DimensionsId = accountTable.IsWorker_Dimensions ? oVehicle.Worker_DimensionsId : null,
                                    FixedAsset_DimensionsId = accountTable.IsFixedAsset_Dimensions ? oVehicle.FixedAsset_DimensionsId : null,
                                    Department_DimensionsId = accountTable.IsDepartment_Dimensions ? oVehicle.Department_DimensionsId : null,
                                    Contract_CC_DimensionsId = accountTable.IsContract_CC_Dimensions ? oVehicle.Contract_CC_DimensionsId : null,
                                    City_DimensionsId = accountTable.IsCity_Dimensions ? oVehicle.City_DimensionsId : null,
                                    D1_DimensionsId = accountTable.IsD1_Dimensions ? oVehicle.D1_DimensionsId : null,
                                    D2_DimensionsId = accountTable.IsD2_Dimensions ? oVehicle.D2_DimensionsId : null,
                                    D3_DimensionsId = accountTable.IsD3_Dimensions ? oVehicle.D3_DimensionsId : null,
                                    D4_DimensionsId = accountTable.IsD4_Dimensions ? oVehicle.D4_DimensionsId : null,
                                };
                                oAccountSales.AccountSalesDetails.Add(oAccountSalesDetails);
                            }

                            if (MovementInvoice.LaborCost > 0)
                            {
                                accountId = items.Where(a => a.ItemNumber == -2).FirstOrDefault()?.ItemSalesAccountId;
                                accountId = accountId == null ? InvoiceType.AccountId : accountId;
                                accountTable = new AccountTable();
                                accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                                oAccountSalesDetails = new AccountSalesDetails()
                                {
                                    ItemNumber = items.Where(a => a.ItemNumber == -2).FirstOrDefault().ItemId,
                                    UnitId = items.Where(a => a.ItemNumber == -2).FirstOrDefault().UnitId,
                                    Discount = 0,
                                    Description = item.Description == null ? "Maintenance Labor " + "( " + oVehicle.PlateNumber + " ) " + " صيانة عمالة" : item.Description,
                                    Quantity = item.Quantity == null || item.Quantity == 0 ? 1 : (int)item.Quantity,
                                    UnitQuantity = item.Quantity == null || item.Quantity == 0 ? 1 : (int)item.Quantity,
                                    Price = MovementInvoice.LaborCost,
                                    Total = MovementInvoice.LaborCost,
                                    TaxValue = MovementInvoice.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100),
                                    TaxClassificationId = items.Where(a => a.ItemNumber == -2).FirstOrDefault().TaxClassificationNo,
                                    Final = MovementInvoice.LaborCost + (MovementInvoice.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100)),
                                    CostsCentersNo = oVehicle.CostCenter,
                                    Reference = oVehicle.PlateNumber.Replace(" ", "").Replace("-", ""),
                                    Customer_DimensionsId = accountTable.IsCustomer_Dimensions ? oVehicle.Customer_DimensionsId : null,
                                    Vendor_DimensionsId = accountTable.IsVendor_Dimensions ? oVehicle.Vendor_DimensionsId : null,
                                    LOB_DimensionsId = accountTable.IsLOB_Dimensions ? oVehicle.LOB_DimensionsId : null,
                                    Regions_DimensionsId = accountTable.IsRegions_Dimensions ? oVehicle.Regions_DimensionsId : null,
                                    Locations_DimensionsId = accountTable.IsLocations_Dimensions ? oVehicle.Locations_DimensionsId : null,
                                    Item_DimensionsId = accountTable.IsItem_Dimensions ? oVehicle.Item_DimensionsId : null,
                                    Worker_DimensionsId = accountTable.IsWorker_Dimensions ? oVehicle.Worker_DimensionsId : null,
                                    FixedAsset_DimensionsId = accountTable.IsFixedAsset_Dimensions ? oVehicle.FixedAsset_DimensionsId : null,
                                    Department_DimensionsId = accountTable.IsDepartment_Dimensions ? oVehicle.Department_DimensionsId : null,
                                    Contract_CC_DimensionsId = accountTable.IsContract_CC_Dimensions ? oVehicle.Contract_CC_DimensionsId : null,
                                    City_DimensionsId = accountTable.IsCity_Dimensions ? oVehicle.City_DimensionsId : null,
                                    D1_DimensionsId = accountTable.IsD1_Dimensions ? oVehicle.D1_DimensionsId : null,
                                    D2_DimensionsId = accountTable.IsD2_Dimensions ? oVehicle.D2_DimensionsId : null,
                                    D3_DimensionsId = accountTable.IsD3_Dimensions ? oVehicle.D3_DimensionsId : null,
                                    D4_DimensionsId = accountTable.IsD4_Dimensions ? oVehicle.D4_DimensionsId : null,
                                };
                                oAccountSales.AccountSalesDetails.Add(oAccountSalesDetails);
                            }
                            // End Invoice
                            oAccountSales.AccountSalesMaster = new AccountSalesMaster()
                            {

                                Total = oAccountSales.AccountSalesDetails.Sum(x => x.Total),
                                Net = oAccountSales.AccountSalesDetails.Sum(x => x.Final),
                                Final = oAccountSales.AccountSalesDetails.Sum(x => x.Total),
                                Tax = oAccountSales.AccountSalesDetails.Sum(x => x.TaxValue),
                                AccSalesTypeNo = 8,
                                AccSalesDate = item.Invoice_Date,
                                InvoiceType = 2,
                                TypeSalesPurchasesID = CreateExternal_Workshop.InvoiceTypeId,
                                Notes = "Maintenance",
                                SupplierInvoiceNo = item.Invoice_No,
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
                                CustomerAccountNo = AccountList.Where(x => x.ID == Supplier.AccountNoPayableId).FirstOrDefault().AccountNo,
                            };
                            oAccountSales.AccountSalesMaster.UserId = UserId.ToString();
                            oAccountSales.AccountSalesMaster.CurrencyID = 1;//((CompanyInfo)Session["CompanyInfo"]).CurrencyIDH;
                            oAccountSales.AccountSalesMaster.AccSalesBranch = BranchId;
                            oAccountSales.AccountSalesMaster.PaymentTerms = Supplier.oLDBPaymentType > 0 ? Supplier.oLDBPaymentType : 0;
                            oAccountSales.CompanyId = CompanyId;
                            oAccountSales.BranchId = BranchId;
                            oAccountSales.AccountSalesMaster.InventoryAccountId = InvoiceType.AccountId;
                            oAccountSales.CompanyType = 1;// ((CompanyInfo)Session["CompanyInfo"]).CompanyType;
                            await _accountingApiClient.AccountSalesMaster_Insert(oAccountSales);
                        }

                        resultsuccess.Add(item.ID.ToString());

                    }
                    catch (Exception)
                    {
                        //3495
                        if (workOrderId > 0)
                        {
                            await _workshopApiClient.DeleteMWorkOrderAsync(workOrderId);
                        }
                        NotAddedList.Add(item);
                        resultfault.Add(item.ID.ToString());
                    }
                }

                foreach (var item in resultfault)
                {
                    var id = Convert.ToInt32(item);
                    var ItemTobeRemoved = CreateExternal_Workshop.DExternalWorkshopExp.Where(a => a.ID == id).FirstOrDefault();
                    CreateExternal_Workshop.DExternalWorkshopExp.Remove(ItemTobeRemoved);
                }
                if (resultsuccess.Count() > 0)
                {
                    CreateExternalWorkshopExpDTO createExternalWorkshopExpDTO = new CreateExternalWorkshopExpDTO();
                    createExternalWorkshopExpDTO.MappMExternalWorkshopExpDTOToCreateExternalWorkshopExpDTO(CreateExternal_Workshop);

                    var Insert_External_Workshop = await _workshopApiClient.InsertExternalWorkshopExpAsync(createExternalWorkshopExpDTO);
                }
                //var _External_Workshop = await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId);
                //ViewBag.ExternalWorkshopList = _External_Workshop.Select(r => new SelectListItem { Text = /*GetCurrentBilanguage(r.PrimaryName, r.SecondaryName)*/language == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();

                result.DataList.Add(JsonConvert.SerializeObject(resultsuccess, Formatting.Indented));
                result.DataList.Add(JsonConvert.SerializeObject(resultfault, Formatting.Indented));
                result.Data = NotAddedList;
                result.IsSuccess = true;
                return Json(result);


            }
            catch (Exception)
            {
                if (workOrderId != 0)
                {
                    await _workshopApiClient.DeleteMWorkOrderAsync(workOrderId);
                }
                result.IsSuccess = false;
                return Json(result);

            }

        }

        [HttpPost]
        public async Task<IActionResult> CheckExcel(MExternalWorkshopExpDTO CreateExternal_Workshop)
        {
            try
            {
                var resultData = new List<Error>();
                Error res = null;
                
                var dExternalWorkshopExps = JsonConvert.DeserializeObject<List<DExternalWorkshopExpDTO>>(CreateExternal_Workshop.External_Workshop_ExpjsonList);
                var Vehicles = await _vehicleApiClient.GetVehiclesPlatesByCompanyId(CompanyId);
                List<ExternalWorkshopExpReportDTO> externalWorkshopExpReports = new List<ExternalWorkshopExpReportDTO>();

                var Invoice_NoDBList = await _workshopApiClient.GetExternalWorkshopExpReportAsync(new ExternalWorkshopExpReportFilterDTO { CompanyId = CompanyId });
                List<string> Invoice_NoExcelList = new List<string>();
                decimal Vat;
                try
                {
                    var VatList = (await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang)).FirstOrDefault();
                    Vat = VatList.TaxRate;
                }
                catch (Exception)
                {
                    Vat = 0;
                }

                var workOrder = new MWorkOrderDTO();
                var movementInvoice = new MovementInvoice();
                for (int i = 0; i < dExternalWorkshopExps.Count; i++)
                {
                    res = new Error
                    {
                        Id = i
                    };

                    try
                    {
                        try
                        {
                            int count = 0;
                            var x = Invoice_NoDBList.Where(a => a.Invoice_No.Replace(" ", "").ToUpper() == dExternalWorkshopExps[i].Invoice_No.Replace(" ", "").ToUpper()).Count();
                            if (x > 0)
                            {
                                count++;
                            }
                            if (Invoice_NoExcelList.Count == 0)
                            {
                                Invoice_NoExcelList.Add(dExternalWorkshopExps[i].Invoice_No);
                            }
                            else
                            {
                                var y = Invoice_NoDBList.Where(a => a.Invoice_No.Replace(" ", "").ToUpper() == dExternalWorkshopExps[i].Invoice_No.Replace(" ", "").ToUpper()).Count();
                                if (y > 0)
                                {
                                    count++;
                                }
                                Invoice_NoExcelList.Add(dExternalWorkshopExps[i].Invoice_No);
                            }
                            if (count > 0)
                            {
                                throw new Exception();
                            }
                            //Damage.VehicleId = Vehicles.Where(a => a.PlateNumber.Replace(" ", "").ToUpper() == dExternalWorkshopExps[i].License_plate_No.Replace(" ", "").ToUpper()).FirstOrDefault().Id;
                        }
                        catch (Exception)
                        {
                            res.Errors.Add(GetCurrentBilanguage("رقم الفاتورة موجود بالفعل", "The Invoice Number is Already Exist"));
                        }
                        try
                        {
                            workOrder.VehicleId = Vehicles.Where(a => a.PlateNumber.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper() == dExternalWorkshopExps[i].License_Plate_No.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper()).FirstOrDefault().Id;
                        }
                        catch (Exception)
                        {
                            res.Errors.Add(GetCurrentBilanguage("رقم اللوحة لا ينتمي إلى أي مركبة", "The Plate Number Does Not Belong to Any Vehicle"));
                        }
                        try
                        {
                            workOrder.GregorianDamageDate = dExternalWorkshopExps[i].Invoice_Date;
                        }
                        catch (Exception)
                        {
                            res.Errors.Add(GetCurrentBilanguage("تنسيق التاريخ غير صحيح، الرجاء إدخال تاريخ مثل هذا (dd-MM-yyyy)", "The Date Format is wrong, Please Enter date like this (dd-MM-yyyy)"));
                        }
                        try
                        {
                            workOrder.Description = dExternalWorkshopExps[i].Description;
                        }
                        catch (Exception)
                        {
                            res.Errors.Add(GetCurrentBilanguage("يرجى التحقق من الوصف", "Please Check Description"));
                        }
                        try
                        {
                            workOrder.WorkOrderTitle = dExternalWorkshopExps[i].Invoice_No;
                        }
                        catch (Exception)
                        {
                            res.Errors.Add(GetCurrentBilanguage("يرجى التحقق من رقم الفاتورة", "Please Check Invoice No"));
                        }
                        try
                        {
                            movementInvoice.TotalInvoice = dExternalWorkshopExps[i].Total ?? 0;
                        }
                        catch (Exception)
                        {
                            res.Errors.Add(GetCurrentBilanguage("يرجى التحقق من الحقل الإجمالي", "Please Check Total Field"));
                        }
                    }
                    catch (Exception ex)
                    {
                        res.Errors.Add(ex.Message);
                    }
                    resultData.Add(res);
                }

                return Json(resultData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null, // This preserves the original case
                    DictionaryKeyPolicy = null
                });
            }
            catch (Exception e)
            {
                return RedirectToAction("Index");
            }
        }

        [CustomAuthorize(Permissions.Collection.Edit)]
        public async Task<IActionResult> Edit(int? id)
        {
            MExternalWorkshopExpDTO data = new MExternalWorkshopExpDTO();
            
            

            var _External_Workshop = await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId);

            if (id.HasValue)
            {
                var ew = await _workshopApiClient.GetExternalWorkshopExpByIdAsync(id.Value);

                data.ExternalWorkshopId = ew.ExternalWorkshopId;
                data.Excel_Date = ew.Excel_Date;
                data.DExternalWorkshopExp = (await _workshopApiClient.GetExternalWorkshopExpDetailsByIdAsync(id.Value))?.ToList();
            }
            else
            {
                data.DExternalWorkshopExp = new List<DExternalWorkshopExpDTO>();
            }

            ViewBag.ExternalWorkshopList = _External_Workshop.Select(r => new SelectListItem { Text = /*GetCurrentBilanguage(r.PrimaryName, r.SecondaryName)*/ lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();

            return View(data);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.Collection.Edit)]
        public async Task<IActionResult> Edit(DExternalWorkshopExpDTO CreateExternal_Workshop)
        {
            TempData result = new TempData();
            int workOrderId = 0;
            try
            {

                
                
                //  D_External_Workshop_Exp d_External_Workshop_Exp = JsonConvert.DeserializeObject<D_External_Workshop_Exp>("");

                List<string> resultsuccess = new List<string>();
                List<string> resultfault = new List<string>();

                var NotAddedList = new List<DExternalWorkshopExpDTO>();
                var VehicleDefinitions = new VehicleDefinitions();
                VehicleDefinitions.CompanyId = CompanyId;
                var Vehicles = await _vehicleApiClient.GetVehiclesPlatesByCompanyId(CompanyId);
                var workOrder = new MWorkOrderDTO();
                try
                {
                    workOrder.VehicleId = Vehicles.Where(a => a.PlateNumber.Replace("-", "").Replace("/", "").Replace(" ", "").ToUpper() == CreateExternal_Workshop.License_Plate_No.Replace("-", "").Replace("/", "").Replace(" ", "").ToUpper()).FirstOrDefault().Id;
                    //Damage.VehicleId = Vehicles.Where(a => a.PlateNumber == CreateExternal_Workshop.License_plate_No).FirstOrDefault().Id;
                    workOrder.WorkOrderType = 2;
                    workOrder.GregorianDamageDate = CreateExternal_Workshop.Invoice_Date;
                    workOrder.IsFix = true;
                    workOrder.Description = CreateExternal_Workshop.Description;
                    workOrder.WorkOrderTitle = CreateExternal_Workshop.Invoice_No;
                    //workOrder.JobCardStatus = 4;
                    workOrder.WorkOrderStatus = 4;
                    workOrder.InvoicingStatus = 2;
                    workOrder.ModifyBy = UserId;
                    workOrder.CompanyId = CompanyId;
                    workOrder.BranchId = BranchId;
                    workOrder.Id = CreateExternal_Workshop.WorkOrderId;
                    await _workshopApiClient.UpdateMWorkOrderAsync(workOrder);
                    CreateExternal_Workshop.VehicleId = workOrder.VehicleId;
                }
                catch (Exception)
                {
                    workOrderId = workOrder.Id;
                    await _workshopApiClient.DeleteMWorkOrderAsync(workOrderId);
                }
                List<DExternalWorkshopExpDTO> d_External_Workshop_Exp = new List<DExternalWorkshopExpDTO>();
                d_External_Workshop_Exp.Add(CreateExternal_Workshop);
                var Insert_External_Workshop = await _workshopApiClient.UpdateExternalWorkshopExpDetailsAsync(d_External_Workshop_Exp);
                //var Insert_External_Workshop = WorkshopAPI.External_Workshop_Exp_Details_Update(d_External_Workshop_Exp);
                var _External_Workshop = _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId);

                result.IsSuccess = true;
                return Json(result);


            }
            catch (Exception)
            {
                if (workOrderId != 0)
                {
                    await _workshopApiClient.DeleteMWorkOrderAsync(workOrderId);
                }
                result.IsSuccess = false;
                return Json(result);

            }

        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(int workshopId, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {

                try
                {

                    decimal Vat;
                    try
                    {
                        var VatList = (await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang)).FirstOrDefault();
                        Vat = VatList.TaxRate;

                    }
                    catch (Exception)
                    {
                        Vat = 0;
                    }
                    var mappingColumn = await _workshopApiClient.GetExcelMappingDetailsByIdAsync(null, workshopId);
                    var mExcelMapping = new MExcelMappingDTO();
                    mExcelMapping.WorkshopId = workshopId;
                    mExcelMapping.CompanyId = CompanyId;

                    var mapingData = (await _workshopApiClient.GetExcelMappingAsync(new ExcelMappingFilterDTO() { CompanyId = CompanyId, BranchId = BranchId }))?.FirstOrDefault();
                    using (var workbook = new XLWorkbook(file.OpenReadStream()))
                    {
                        var worksheet = workbook.Worksheet(1);
                        var rows = worksheet.RowsUsed().Skip(mapingData.Started_Row - 1);
                        MExternalWorkshopExpDTO MExternalWorkshopExpDTO = new MExternalWorkshopExpDTO();
                        List<DExternalWorkshopExpDTO> jsonData = new List<DExternalWorkshopExpDTO>();

                        //var vehicleMapping = Mapping_Column.FirstOrDefault(a => a.Mapping_ColumnDB == "VEHICLEID");
                        //var Col_VehicleId_Index = vehicleMapping == null ? 0 : vehicleMapping.Mapping_Column_Index;

                        int Col_VehicleId_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "VEHICLEID")?.Mapping_Column_Index ?? 0;
                        int Col_Invoice_No_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "INVOICE_NO")?.Mapping_Column_Index ?? 0;
                        int Col_License_plate_No_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "LICENSE_PLATE_NO")?.Mapping_Column_Index ?? 0;
                        int Col_Invoice_Date_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "INVOICE_DATE")?.Mapping_Column_Index ?? 0;
                        int Col_Business_Line_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "BUSINESS_LINE")?.Mapping_Column_Index ?? 0;
                        int Col_MILAGE_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "MILAGE")?.Mapping_Column_Index ?? 0;
                        int Col_City_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "CITY")?.Mapping_Column_Index ?? 0;
                        int Col_Quantity_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "QUANTITY")?.Mapping_Column_Index ?? 0;
                        int Col_Price_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "PRICE")?.Mapping_Column_Index ?? 0;
                        int Col_Description_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "DESCRIPTION")?.Mapping_Column_Index ?? 0;
                        int Col_Maker_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "MAKER")?.Mapping_Column_Index ?? 0;
                        int Col_Vin_No_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "VIN_NO")?.Mapping_Column_Index ?? 0;
                        int Col_Model_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "MODEL")?.Mapping_Column_Index ?? 0;
                        int Col_Year_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "YEAR")?.Mapping_Column_Index ?? 0;
                        int Col_SubTotal_BeforVat_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "SUBTOTAL_BEFORVAT")?.Mapping_Column_Index ?? 0;
                        int Col_Vat_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "VAT")?.Mapping_Column_Index ?? 0;
                        int Col_Total_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "TOTAL")?.Mapping_Column_Index ?? 0;
                        int Col_Service_Type_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "SERVICE_TYPE")?.Mapping_Column_Index ?? 0;
                        var Service_Type = mappingColumn.Where(a => a.Mapping_ColumnDB.ToUpper() == "SERVICE_TYPE").ToList();
                        int Col_JobCardId_Index = mappingColumn.FirstOrDefault(a => a.Mapping_ColumnDB.ToUpper() == "JOBCARDID")?.Mapping_Column_Index ?? 0;


                        string Invoice_No = "";
                        foreach (var row in rows)
                        {
                            var dataRow = new DExternalWorkshopExpDTO();

                            if (!string.IsNullOrEmpty(row.Cell(Col_Invoice_No_Index).Value.ToString()))
                            {
                                Invoice_No = Col_Invoice_No_Index != 0 ? row.Cell(Col_Invoice_No_Index).Value.ToString() : "";
                                var _Invoice_Date = Col_Invoice_Date_Index != 0 ? row.Cell(Col_Invoice_Date_Index).Value.ToString() : DateTime.Now.ToString();
                                dataRow.Invoice_No = Invoice_No;
                                dataRow.Invoice_Date = Convert.ToDateTime(_Invoice_Date);
                                dataRow.License_Plate_No = Col_License_plate_No_Index != 0 ? row.Cell(Col_License_plate_No_Index).Value.ToString() : "";
                                dataRow.Business_Line = Col_Business_Line_Index != 0 ? row.Cell(Col_Business_Line_Index).Value.ToString() : "";
                                dataRow.MILAGE = Col_MILAGE_Index != 0 ? Convert.ToInt32(string.IsNullOrEmpty(row.Cell(Col_MILAGE_Index).Value.ToString()) == true ? "0" : row.Cell(Col_MILAGE_Index).Value.ToString()) : 0;
                                dataRow.City = Col_City_Index != 0 ? row.Cell(Col_City_Index).Value.ToString() : "";
                                dataRow.VehicleId = 0;
                                dataRow.Maker = Col_Maker_Index != 0 ? row.Cell(Col_Maker_Index).Value.ToString() : "";
                                dataRow.Model = Col_Model_Index != 0 ? row.Cell(Col_Model_Index).Value.ToString() : "";
                                dataRow.Year = Col_Year_Index != 0 ? row.Cell(Col_Year_Index).Value.ToString() : "";
                                dataRow.SubTotal_BeforVat = Col_SubTotal_BeforVat_Index != 0 ? Convert.ToDecimal(string.IsNullOrEmpty(row.Cell(Col_SubTotal_BeforVat_Index).Value.ToString()) == true ? "0" : row.Cell(Col_SubTotal_BeforVat_Index).Value.ToString()) : 0;
                                dataRow.Vat = Col_Vat_Index != 0 ? Convert.ToDecimal(string.IsNullOrEmpty(row.Cell(Col_Vat_Index).Value.ToString()) == true ? "0" : row.Cell(Col_Vat_Index).Value.ToString()) : 0;
                                dataRow.Total = Col_Total_Index != 0 ? Convert.ToDecimal(string.IsNullOrEmpty(row.Cell(Col_Total_Index).Value.ToString()) == true ? "0" : row.Cell(Col_Total_Index).Value.ToString()) : 0;
                                dataRow.Service_Type = Service_Type.Count() == 1 ? row.Cell(Service_Type.FirstOrDefault().Mapping_Column_Index ?? 1).Value.ToString() : "PARTS AND SERVICES";
                                try
                                {
                                    dataRow.Quantity = Col_Quantity_Index != 0 ? Convert.ToInt32(string.IsNullOrEmpty(row.Cell(Col_Quantity_Index).Value.ToString()) == true ? "0" : row.Cell(Col_Quantity_Index).Value.ToString()) : 0;

                                }
                                catch
                                {

                                    dataRow.Quantity = 0;
                                }
                                dataRow.Description = Col_Description_Index != 0 ? row.Cell(Col_Description_Index).Value.ToString() : "";
                                dataRow.Price = Col_Price_Index != 0 ? Convert.ToDecimal(string.IsNullOrEmpty(row.Cell(Col_Price_Index).Value.ToString()) == true ? "0" : row.Cell(Col_Price_Index).Value.ToString()) : 0;
                                jsonData.Add(dataRow);
                            }
                            else
                            {
                                var _jsonData = jsonData.Where(a => a.Invoice_No == Invoice_No).FirstOrDefault();
                                dataRow.Invoice_No = _jsonData.Invoice_No;
                                dataRow.Invoice_Date = _jsonData.Invoice_Date;
                                dataRow.License_Plate_No = _jsonData.License_Plate_No;
                                dataRow.Business_Line = _jsonData.Business_Line;
                                dataRow.MILAGE = _jsonData.MILAGE;
                                dataRow.City = _jsonData.City;
                                dataRow.VehicleId = 0;
                                dataRow.Maker = _jsonData.Maker;
                                dataRow.Model = _jsonData.Model;
                                dataRow.Year = _jsonData.Year;
                                dataRow.SubTotal_BeforVat = 0;
                                dataRow.Vat = 0;
                                dataRow.Service_Type = Col_Service_Type_Index != 0 ? (string.IsNullOrEmpty(row.Cell(Col_Service_Type_Index).Value.ToString()) == false ? row.Cell(Col_Service_Type_Index).Value.ToString() : "") : "";
                                dataRow.Quantity = Col_Quantity_Index != 0 ? Convert.ToInt32(string.IsNullOrEmpty(row.Cell(Col_Quantity_Index).Value.ToString()) == true ? "0" : row.Cell(Col_Quantity_Index).Value.ToString()) : 0;
                                dataRow.Description = row.Cell("F").Value.ToString();
                                dataRow.Price = Col_Price_Index != 0 ? Convert.ToDecimal(string.IsNullOrEmpty(row.Cell(Col_Price_Index).Value.ToString()) == true ? "0" : row.Cell(Col_Price_Index).Value.ToString()) : 0;
                                var Vatableservice = Service_Type.Where(a => a.Is_Vat_Included == true && a.Addition_Cullman_Name.ToUpper() == dataRow.Service_Type.ToUpper());
                                if (Vatableservice.Count() == 1)
                                {
                                    dataRow.Total = (dataRow.Quantity * dataRow.Price * Vat / 100) + (dataRow.Quantity * dataRow.Price);
                                }
                                else
                                {
                                    dataRow.Total = dataRow.Quantity * dataRow.Price;
                                }

                                jsonData.Add(dataRow);


                            }


                        }
                        return Json(new { success = true, data = jsonData }, new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = null, // This preserves the original case
                            DictionaryKeyPolicy = null
                        });
                    }


                }
                catch (Exception)
                {
                    return Json(new { success = false, Message = "Ther Is An Issue With Uploaded File." });
                }
            }
            else
            {
                return Json(new { success = false, Message = "Ther Is No Uploaded File." });
            }
        }

        private string GetCurrentBilanguage(string ar, string en)
        {
            if (lang == "en")
            {
                return en;
            }
            else
            {
                return ar;
            }
        }
    }
}
