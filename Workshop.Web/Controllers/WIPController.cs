
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Domain.Enum;
using Workshop.Infrastructure;
using Workshop.Resources;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Drawing;
using System.Drawing.Imaging;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class WIPController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly InventoryApiClient _inventoryApiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly ReportsServiceApiClient _reportsServiceApiClient;
        private readonly WorkflowEmailService _workflowEmailService;
        private readonly IFileService _fileService;
        private readonly IFileValidationService _fileValidationService;
        private readonly ILogger<WIPController> _logger;
        private readonly IStringLocalizer<Common> _common;
        //private readonly MsegatHelper _msegat;

        public readonly string lang;
        public WIPController(
            AccountingApiClient accountingApiClient,
            VehicleApiClient vehicleApiClient,
            WorkshopApiClient apiClient,
            InventoryApiClient inventoryApiClient,
            ERPApiClient erpApiClient,
            ReportsServiceApiClient repo,
            WorkflowEmailService workflowEmailService,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IFileService fileService,
            IFileValidationService fileValidationService,
            IStringLocalizer<Common> common,
            ILogger<WIPController> logger
            , IMemoryCache cache) : base(cache, configuration, env)
        {
            _accountingApiClient = accountingApiClient;
            _vehicleApiClient = vehicleApiClient;
            _apiClient = apiClient;
            _inventoryApiClient = inventoryApiClient;
            _reportsServiceApiClient = repo;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            _workflowEmailService = workflowEmailService;
            _fileService = fileService;
            _fileValidationService = fileValidationService;
            _erpApiClient = erpApiClient;
            _logger = logger;
            _common = common;
            //_msegat = msegat;
        }


        public IActionResult CreateJobCard(int movementId)
        {
            return RedirectToAction("Edit", new { id = (int?)null, movementId = movementId });
        }

        [CustomAuthorize(Permissions.WIP.View)]
        public async Task<IActionResult> Index([FromForm] FilterWIPDTO? oFilterWIPDTO, bool isEmbed=false)
        {

            //ViewBag.isEmbed = isEmbed;
            var isCompanyCenterialized = 1;
            oFilterWIPDTO ??= new FilterWIPDTO();
            oFilterWIPDTO.PageNumber = oFilterWIPDTO.PageNumber ?? 1;
            oFilterWIPDTO.WorkshopId = BranchId;

            var data = await _apiClient.GetAllWIPsAsync(oFilterWIPDTO);
            WIPDTO oWIPDTO = new WIPDTO();
            oWIPDTO.List = data;

            ViewBag.Makes = await GetMakes();

            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            ViewBag.Customers = allCustomers.Select(c => new SelectListItem
            { Value = c.Id.ToString(), Text = c.CustomerName }).ToList();


            var status = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(8, CompanyId); //WIP Status
            ViewBag.Status = status.Select(t => new SelectListItem { Text = lang == "en" ? t.Code + " - " + t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_WIPList", oWIPDTO.List);
            }

            return View(oWIPDTO);
        }


        [HttpPost]
        [CustomAuthorize(Permissions.WIP.Create)]
        public async Task<IActionResult> Add([FromBody] CreateWIPDTO dto)
        {

            int? result;
            dto.WorkshopId = BranchId;
            dto.CreatedBy = UserId;

            result = await _apiClient.AddWIPAsync(dto);

            if (!result.HasValue)
            {
                ModelState.AddModelError(string.Empty, "Failed to create technician.");
                return View(dto);
            }
            return View();
        }


        public async Task<IActionResult> Edit(int? id, int? movementId = 0, bool isEmbed = false)
        {
            try
            {

                var isCompanyCenterialized = 1;
                WIPDTO dto = new WIPDTO();
                VehicleMovement movement = new VehicleMovement();

                if (movementId.HasValue && movementId > 0)
                {
                    movement = await _apiClient.GetVehicleMovementByIdAsync((int)movementId);
                    if (movement != null)
                    {
                        dto.VehicleId = (int)movement.VehicleID;
                        dto.MovementId = (int)movementId;
                    }
                }

                //var priceWfDto = new PriceWorkflowDTO { CompanyId = CompanyId, BranchId = BranchId };
                //ViewBag.PriceWorkflowDefinitions = await _apiClient.GetPriceWorkflowDefinitionAsync(priceWfDto);

                var _WIPID = 0;

                if (id.HasValue && id.Value > 0)
                {
                    _WIPID = id.Value;
                    dto = await _apiClient.GetWIPByIdAsync(id.Value);
                    if (dto == null)
                    {
                        _logger.LogWarning($"WIP with ID {id.Value} not found.");
                        return NotFound();
                    }
                    var externalInvoices = await _apiClient.GetWorkshopInvoiceByWorkOrderId(dto.WorkOrderId ?? 0);
                    if (externalInvoices != null && externalInvoices.Any())
                    {
                        ViewBag.HasExternalInvoices = true;

                        ViewBag.TransferLabourCost = externalInvoices.Sum(x => x.LaborCost);
                        ViewBag.TransferPartsCost = externalInvoices.Sum(x => x.PartsCost);
                        ViewBag.TransferTotalInvoice = externalInvoices.Sum(x => x.TotalAmount);
                        ViewBag.TransferTotalInvoiceWithoutVat = externalInvoices.Sum(x => x.TotalAmount - x.Vat);
                        ViewBag.TransferVatAmount = externalInvoices.Sum(x => x.Vat);

                    }
                    else
                    {
                        ViewBag.HasExternalInvoices = false;
                    }

                    //RegistrationNo
                    var RegistrationNo = await VehicleDocumants(dto.VehicleId, 8);


                    var movementOperators = await GetMovementOperatorsAsync(movementId, dto);

                    ViewBag.DueInDate = movementOperators.DueInDate;
                    ViewBag.ReceivedMeter = movementOperators.ReceivedMeter;
                    ViewBag.CreatingOperator = movementOperators.CreatingOperator;

                    ViewBag.DueOutDate = movementOperators.DueOutDate;
                    ViewBag.BookedOutOperator = movementOperators.BookedOutOperator;

                    ViewBag.InvoicingOperator = movementOperators.InvoicingOperator;


                    // Get vehicle documents - handle nulls
                    var docs = await GetVehicleDocumentDatesAsync(dto.VehicleId);
                    ViewBag.RegDate = docs.RegDate;
                    ViewBag.MOTDate = docs.MOTDate;

                    // Get internal matches
                    ViewBag.InternalMatches = await GetInternalMatchesAsync();

                    // Get account details
                    dto.AccountDetails = await GetAccountDetailsAsync(dto.Id);

                    // Get sales type based on account type
                    var salesTypes = await GetSalesTypesAsync(dto.AccountDetails);
                    ViewBag.SalesType = salesTypes.SalesType;
                    ViewBag.PartialSalesType = salesTypes.PartialSalesType;



                    // Get services
                    ViewBag.Services = await GetWipServicesAsync(id.Value);

                    
                    
                    // Get account details
                    dto.InvoiceDetailsList = await _apiClient.WIPInvoiceGetById(dto.Id, null);


                    // Get items
                    var wipItems = await GetWipItemsAsync(id.Value);
                    ViewBag.Items = wipItems.Items;
                    ViewBag.AllowActions = wipItems.AllowActions;

                }

                ViewBag.ID = _WIPID;

                    // Get makes
                    try
                    {
                        dto = dto ?? new WIPDTO();
                        WorkOrderFilterDTO workOrderFilterDTO = new WorkOrderFilterDTO();
                        workOrderFilterDTO.VehicleID = dto.VehicleId;
                        workOrderFilterDTO.CompanyId = CompanyId;
                        //workOrderFilterDTO.BranchId = BranchId;
                        var allManufacturers = await GetMakes();
                        var allModels = await GetModels();
                        var workOrder = (await _apiClient.GetMWorkOrdersAsync(workOrderFilterDTO));
                        dto.VehicleTab = await _apiClient.WIP_GetVehicleDetailsById(dto.Id) ?? new VehicleTabDTO();
                        dto.WorkOrderId = dto.WorkOrderId ?? workOrder?.FirstOrDefault()?.Id;
                        var VehiclesColors = await _vehicleApiClient.GetAllColors(lang);
                        if (dto.WorkOrderId != null && dto.WorkOrderId > 0)
                        {
                            var workorder = await _apiClient.GetMWorkOrderByID(dto.WorkOrderId ?? 0);

                            if (workorder?.VehicleType == (int)VehicleTypeId.Internal)
                            {
                                var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_Find(dto.VehicleId)) ?? new VehicleDefinitions();
                                dto.VehicleTab.ManufacturerId = vehicleDetails.ManufacturerId;
                                dto.VehicleTab.ModelId = vehicleDetails?.VehicleModelId;
                                dto.VehicleTab.ClassId = vehicleDetails?.VehicleClassId;
                                dto.VehicleTab.PlateNumber = vehicleDetails?.PlateNumber;
                                dto.VehicleTab.ManufacturingYear = vehicleDetails?.ManufacturingYear;
                                dto.VehicleTab.Color = vehicleDetails?.Color;
                                dto.VehicleTab.ColorName = VehiclesColors?.FirstOrDefault(c => c?.Id == vehicleDetails?.Color)?.Name;
                                dto.VehicleTab.ChassisNo = vehicleDetails?.ChassisNo;
                                dto.VehicleTab.ManufacturerPrimaryName = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerPrimaryName).FirstOrDefault();
                                dto.VehicleTab.ManufacturerSecondaryName = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerSecondaryName).FirstOrDefault();
                                dto.VehicleTab.VehicleModelPrimaryName = allModels?.Where(i => i.Id == vehicleDetails?.VehicleModelId).Select(s => s.VehicleModelPrimaryName).FirstOrDefault();
                                dto.VehicleTab.VehicleModelSecondaryName = allModels?.Where(i => i.Id == vehicleDetails?.VehicleModelId).Select(s => s.VehicleModelSecondaryName).FirstOrDefault();
                                var recallResponse = await _apiClient.GetActiveRecallsByChassis(vehicleDetails?.ChassisNo);

                                ViewBag.HasRecall = recallResponse?.HasActiveRecall ?? false;

                            }
                            else
                            {
                                var vehicleDetails = await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById((int)dto.VehicleId);
                                dto.VehicleTab.ManufacturerId = vehicleDetails.ManufacturerId;
                                dto.VehicleTab.ModelId = vehicleDetails.VehicleModelId;
                                dto.VehicleTab.PlateNumber = vehicleDetails.PlateNumber;
                                dto.VehicleTab.ManufacturingYear = vehicleDetails.ManufacturingYear;
                                dto.VehicleTab.Color = vehicleDetails.Color;
                                dto.VehicleTab.ColorName = VehiclesColors?.FirstOrDefault(c => c?.Id == vehicleDetails?.Color)?.Name;
                                dto.VehicleTab.ChassisNo = vehicleDetails.ChassisNo;
                                dto.VehicleTab.ManufacturerPrimaryName = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerPrimaryName).FirstOrDefault();
                                dto.VehicleTab.ManufacturerSecondaryName = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerSecondaryName).FirstOrDefault();
                                dto.VehicleTab.VehicleModelPrimaryName = allModels?.Where(i => i.Id == vehicleDetails?.VehicleModelId).Select(s => s.VehicleModelPrimaryName).FirstOrDefault();
                                dto.VehicleTab.VehicleModelSecondaryName = allModels?.Where(i => i.Id == vehicleDetails?.VehicleModelId).Select(s => s.VehicleModelSecondaryName).FirstOrDefault();
                                var allCustomers = await _vehicleApiClient.Get_CustomerInformation(BranchId, "en", null);

                                if (vehicleDetails?.CompanyId is int companyId && companyId > 0)
                                {
                                    var companyCustomer = allCustomers?.FirstOrDefault(c => c.Id == vehicleDetails?.CompanyId);

                                    dto.CompanyName = lang=="en" ? companyCustomer?.CustomerPrimaryName : companyCustomer.CustomerSecondaryname; 
                                }

                        }
                    }
                        ViewBag.Makes = await GetMakesList();
                        ViewBag.Models = await GetModelsList(dto.VehicleTab.ManufacturerId ?? 0);
                        ViewBag.Classes = await GetClasses();
                        ViewBag.Colors = await GetColors();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching makes");
                        ViewBag.Makes = new List<SelectListItem>();
                    }

                ViewBag.GeneralRequest = Convert.ToBoolean(_configuration["GeneralRequest"] ?? "false");

                // Get account types enum
                try
                {
                    ViewBag.AccountTypes = Enum.GetValues(typeof(AccountTypeEnum)).Cast<AccountTypeEnum>()
                        .Select(e => new SelectListItem
                        {
                            Value = ((int)e).ToString(),
                            Text = lang == "en" ? e.ToString() : (e == AccountTypeEnum.Internal ? "داخلي" : e == AccountTypeEnum.External ? "خارجي" : "")

                        }).ToList() ?? new List<SelectListItem>();
                }
                catch
                {
                    ViewBag.AccountTypes = new List<SelectListItem>();
                }
                // Get WIP service
                
                var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(15, CompanyId);
                ViewBag.VehcileServices = VehcileServices?.Select(t => new SelectListItem
                {
                    Text = lang == "en" ?  t.PrimaryName : t.SecondaryName,
                    Value = t.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();
                // Get WIP status
                try
                {
                    var status = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(8, CompanyId);
                    ViewBag.Status = status?.Select(t => new SelectListItem
                    {
                        Text = lang == "en" ? t.Code + " - " + t.PrimaryName : t.SecondaryName,
                        Value = t.Id.ToString()
                    }).ToList() ?? new List<SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching WIP status");
                    ViewBag.Status = new List<SelectListItem>();
                }



                // Get units
                ViewBag.Units = await GetUnitsSelectListAsync();

                // Get warehouses
                ViewBag.Warehouses = await GetWarehousesSelectListAsync();

                // Get VAT classification
                try
                {
                    var VatList = await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId);
                    ViewBag.VatClassificationList = VatList?.Select(t => new SelectListItem
                    {
                        Text = t.Name,
                        Value = t.TaxClassificationNo.ToString()
                    }).ToList() ?? new List<SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching VAT classification");
                    ViewBag.VatClassificationList = new List<SelectListItem>();
                }

                // Get currency list
                try
                {
                    var CurrencyList = await _erpApiClient.GetCurrecy(CompanyId, BranchId, lang);
                    ViewBag.CurrencyList = CurrencyList?.Select(c => new SelectListItem
                    {
                        Value = c.CurrencyID.ToString(),
                        Text = lang == "en" ? c.CurrencyCode + " - " + c.CurrencyPrimaryName : c.CurrencyCode + " - " + c.CurrencySecondlyName
                    }).ToList() ?? new List<SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching currency list");
                    ViewBag.CurrencyList = new List<SelectListItem>();
                }

                // Get payment terms
                try
                {
                    var termsList = await _accountingApiClient.PaymentTerms_Get(CompanyId, BranchId);
                    ViewBag.Terms = termsList?.Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = lang == "en" ? t.PrimaryName : t.SecondaryName
                    }).ToList() ?? new List<SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching payment terms");
                    ViewBag.Terms = new List<SelectListItem>();
                }

                // Get technicians
                try
                {
                    var technicians = await _apiClient.GetTechniciansDDL(BranchId);
                    ViewBag.Technicians = technicians?.Select(t => new SelectListItem
                    {
                        Value = t.Id.ToString(),
                        Text = lang == "en" ? t.PrimaryName : t.SecondaryName
                    }).ToList() ?? new List<SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching technicians");
                    ViewBag.Technicians = new List<SelectListItem>();
                }

                // Check if waiting for invoice
                if (dto.Status == (int)WIPStatusEnum.G)
                {
                    ViewBag.IsWaitingInvoiced = true;
                }
                
                if(dto.Status == (int)WIPStatusEnum.C)
                {
                    ViewBag.IsCompleated = true;
                }

                int? selectedCustomerId = null;

                // Get customer agreements for vehicle
                try
                {
                    if (dto.VehicleId > 0)
                    {

                        var activeAgreement = await _vehicleApiClient.GetActiveAgreementId(dto.VehicleId);

                        selectedCustomerId = activeAgreement?.CustomerId;
                        var status = (activeAgreement?.AgreementId > 0) ? _common["Open"] : _common["NoAgreement"];


                        ViewBag.AgreementStatus = status;
                        if (activeAgreement.AgreementId != null && activeAgreement.AgreementId > 0)
                        {
                            ViewBag.AgreementEndDate = activeAgreement.GregorianReturnDate?.ToString("dd-MMM-yyyy");
                            dto.AgreementId = (int)activeAgreement.AgreementId;
                        }

                        if (dto.AgreementId != null)
                        {
                            var agreement = await _vehicleApiClient.Get_AgreementCustomerAndCompanyName((int)dto.AgreementId, lang);
                            dto.CompanyName = agreement.CompanyName;
                        }

                        var isReplacement = false;
                        if(dto.AgreementId != null)
                        {
                            var generalInfo = await _vehicleApiClient.GetGeneralInfo((int)dto.AgreementId);
                            var x =  generalInfo.ReservationId;
                            if(generalInfo?.ReservationId != null)
                            {
                                var agreementVehicle = await _vehicleApiClient.GetReservationRentalDetails((int)generalInfo.ReservationId, lang);
                                var vehicleInReservation = agreementVehicle.VehicleDefinitionId;
                                if(dto.VehicleId != vehicleInReservation)
                                {
                                    isReplacement = true;
                                }
                            }
                        }
                        ViewBag.IsReplacement = isReplacement;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching customer agreements for vehicle {VehicleId}", dto.VehicleId);
                }

                // Get customers
                try
                {
                    var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                    ViewBag.Customers = allCustomers?.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.CustomerName
                        //Selected = selectedCustomerId.HasValue && c.Id == selectedCustomerId.Value
                    }).ToList() ?? new List<SelectListItem>();

                    //if (selectedCustomerId.HasValue)
                    //    dto.AccountDetails.CustomerId = selectedCustomerId.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching customers");
                    ViewBag.Customers = new List<SelectListItem>();
                }


                var ourwarehouses = await GetOurWarehouse();
                ViewBag.OurWarehouses = ourwarehouses;

                if (dto.WipDate == default(DateTime) || dto.WipDate == DateTime.MinValue || dto.WipDate == null)
                {
                    dto.WipDate = DateTime.Today;
                }

                ViewBag.CreationDate = dto.WipDate?.ToString("yyyy-MM-dd");

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WIPController.Edit for id {Id} and movementId {MovementId}", id, movementId);

                // Return a user-friendly error view
                TempData["ErrorMessage"] = "An error occurred while loading the WIP details. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.WIP.Create)]
        public async Task<IActionResult> Edit_Post(UpdateWIPDTO dto)
        {
            try
            {

                int? result;

                dto.WorkshopId = BranchId;
                dto.ModifyBy = UserId;


                //dto.FK_WarehouseId = await GetOurWarehouse();
                dto.ItemsList = !string.IsNullOrEmpty(dto.Items)
                 ? System.Text.Json.JsonSerializer.Deserialize<IEnumerable<BaseItemDTO>>(dto.Items)
                 : new List<BaseItemDTO>();
                if (dto.Id > 0 && dto.ItemsList != null)
                {
                    foreach (var item in dto.ItemsList)
                    {
                        if (item.WIPId == 0)
                            item.WIPId = dto.Id;

                        item.Discount = item.Discount;
                    }
                }

                var defs = await _apiClient.GetPriceWorkflowDefinitionAsync(
                    new PriceWorkflowDTO { CompanyId = CompanyId, BranchId = BranchId });

                foreach (var item in dto.ItemsList)
                {
                    //d.KeyId == "SalePrice" &&
                    var saleMatch = defs
                        .Where(d => item.SalePrice >= d.Price)
                        .OrderByDescending(d => d.Price)
                        .FirstOrDefault();

                    //d.KeyId == "CostPrice" &&
                    var costMatch = defs
                        .Where(d => item.CostPrice >= d.Price)
                        .OrderByDescending(d => d.Price)
                        .FirstOrDefault();

                    if (saleMatch == null && costMatch == null)
                    {
                        item.RequiresPriceApproval = false;
                        item.PriceWorkflowEnumId = null;
                    }
                    else
                    {
                        var chosen = (saleMatch == null) ? costMatch
                                   : (costMatch == null) ? saleMatch
                                   : (saleMatch.Price >= costMatch.Price ? saleMatch : costMatch);

                        item.RequiresPriceApproval = true;
                        item.PriceWorkflowEnumId = chosen.WorkflowID;
                    }
                }

                bool needApproval = dto.ItemsList.Any(x => x.RequiresPriceApproval == true);

                dto.ServicesList = !string.IsNullOrEmpty(dto.Services)
                    ? System.Text.Json.JsonSerializer.Deserialize<IEnumerable<CreateWIPServiceDTO>>(dto.Services)
                    : new List<CreateWIPServiceDTO>();

                var success = 0;

                if (dto.Id == 0)
                {
                    var newWip = new CreateWIPDTO
                    {
                        WorkshopId = BranchId,
                        CreatedBy = UserId,
                        VehicleId = dto.VehicleId,
                        MovementId = dto.MovementId,
                        AgreementId = dto.AgreementId,
                        Status = dto.Status,
                        Note = dto.Note,
                        WipDate = dto.WipDate,
                        ItemsList = dto.ItemsList,
                        ServicesList = dto.ServicesList,
                    };

                    success = await _apiClient.AddWIPAsync(newWip) ?? 0;
                }
                else
                {
                    success = await _apiClient.UpdateWIPAsync(dto) ?? 0;
                }

                if (success > 0)
                {
                    dto.AccountDetails.WIPId = success;
                    dto.VehicleTab.WIPId = success;
                    dto.Options.WIPId = success;

                    var InsertAccount = await _apiClient.InsertWIPAccount(dto.AccountDetails);
                    var InsertVehicleDetails = await _apiClient.InsertWIPVehicleDetails(dto.VehicleTab);
                    var optionsUpdate = await UpdateWIPOptions(dto.Options);
                    if (dto.Status == (int)WIPStatusEnum.C)
                    {
                        await GetReturnParts(dto.Id);
                    }


                    if (needApproval)
                    {
                        var pendingLines = await _apiClient.WipPriceWorkflow_GetPendingLines(success);

                        if (pendingLines != null && pendingLines.Any())
                        {

                            foreach (var line in pendingLines)
                            {
                                // 1) create master id for this line
                                var masterId = Guid.NewGuid();

                                // 2) create workflow instance in ERP 
                                var done = await _erpApiClient.InsertWorkflowInstance(line.PriceWorkflowEnumId.Value,masterId,CompanyId, UserId, GroupId);

                                if (!done)
                                {
                                    //  Error 
                                    continue;
                                }

                                // 3) mark WIP item as pending in workshop DB
                                await _apiClient.WipPriceWorkflow_Apply(new ApplyWipPriceWorkflowResult
                                {
                                    WipItemId = line.Id,
                                    MasterId = masterId,
                                    WorkflowEnumId = line.PriceWorkflowEnumId,
                                    Created = true,
                                    UserId = UserId
                                });

                                // 4) notifications (optional)
                                //await SendWorkflowEmailAndNotification(masterId, Action: 0, CreatedBy: UserId, wipItemId: line.Id);
                            }
                        }
                    }
                    return Json(new { success = true, wipId = success });
                }
                return Json(new { success = false });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }


        [HttpPost]
        public async Task<IActionResult> VehicleList([FromBody] VehicleAdvancedFilter filter)
        {
            filter ??= new VehicleAdvancedFilter();
            var colVehicleDefinitions = new List<VehicleDefinitions>();


            filter.CompanyId = CompanyId;

            if (filter.VehicleTypeId == 1) // internal
            {
                colVehicleDefinitions = await _vehicleApiClient.GetWorkshopVehicles(filter);
            }
            else if (filter.VehicleTypeId == 2) // external
            {
                colVehicleDefinitions = await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicles(filter.PageNumber, filter.ManufacturerId == 0 ? default(int?) : filter.ManufacturerId, filter.PlateNumber, filter.VehicleModelId == 0 ? default(int?) : filter.VehicleModelId);
            }
            return PartialView("_VehicleSelectList", colVehicleDefinitions);
        }
        public async Task<IActionResult> GetAccountNumber(int Id)////Change int to bigint for any id in the system 
        {

            var TypeList = await _accountingApiClient.TypeSalesPurchases_GetAll(CompanyId, BranchId, 1, 1);
            var selectedType = TypeList.FirstOrDefault(t => t.Id == Id);
            var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, "en");
            var accountInfo = AccountList.FirstOrDefault(c => c.ID == selectedType.AccountId);
            return Json(accountInfo);
        }

        private async Task<List<SelectListItem>> GetMakesList()
        {

            var makes = await _vehicleApiClient.GetAllManufacturers(lang);

            return makes.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = lang == "en" ? m.ManufacturerPrimaryName : m.ManufacturerSecondaryName
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetModelsList(int manufacturerId = 0)
        {

            var models = await _vehicleApiClient.GetAllVehicleModel(manufacturerId, lang);

            return models.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = lang == "en" ? m.VehicleModelPrimaryName : m.VehicleModelSecondaryName
            }).ToList();////plase replace m with name
        }


        private async Task<List<Manufacturers>> GetMakes()
        {
            var makes = await _vehicleApiClient.GetAllManufacturers(lang);
            return makes;
        }

        private async Task<List<VehicleModel>> GetModels(int manufacturerId = 0)
        {
            var models = await _vehicleApiClient.GetAllVehicleModel(manufacturerId, lang);
            return models;
        }

        private async Task<List<SelectListItem>> GetClasses()
        {

            var models = await _vehicleApiClient.GetAllVehicleClass(lang);

            return models.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = lang == "en" ? m.VehicleClassPrimaryName : m.VehicleClassSecondaryName
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetColors()
        {

            var models = await _vehicleApiClient.GetAllColors(lang);

            return models.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = lang == "en" ? m.Name : m.Name
            }).ToList();
        }

        public async Task<object> GetVehicleByManufacturerId(int manufacturerId)
        {
            var vehicles = await _vehicleApiClient.GetAllVehicleModel(manufacturerId, lang);

            return vehicles;
        }

        public async Task<JsonResult> GetRTSDDL([FromQuery] RTSWithTimeDTO dto)
        {
            //IEnumerable<RTSCodeDTO> RTSCodeDDL = await _apiClient.GetAllRTSCodesDDLAsync();
            dto.CompanyId = CompanyId;
            IEnumerable<RTSCodeDTO> RTSCodeDDL = await _apiClient.GetAllServicesWithTimeAsync(dto);
            return Json(RTSCodeDDL);
        }

        public async Task<JsonResult> GetMenuDDL()
        {
            IEnumerable<MenuDTO> MenuDDL = await _apiClient.GetAllMenus();
            return Json(MenuDDL);
        }

        public async Task<JsonResult> GetPlateNumber([FromQuery] int vehicleId)
        {
            var vehicleDetails = await _vehicleApiClient.GetVehicleDetails(vehicleId, lang);
            var number = vehicleDetails.PlateNumber;
            return Json(number);
        }

        [HttpPost]
        public async Task<JsonResult> GetAllItems(int fK_GroupId, int fK_CategoryId, int fK_SubCategoryId, int wipId)
        {
            var items = await _inventoryApiClient.GetItemsWithStockAndLocation(fK_GroupId, fK_CategoryId, fK_SubCategoryId, wipId);

            var allCategories = await _inventoryApiClient.GetAllCategoriesAsync();
            var allUnits = await _inventoryApiClient.GetAllUnitDDL();

            var myWarehouseIds = new HashSet<int>(await GetOurWarehouse());

            var catById = allCategories.ToDictionary(c => c.Id);
            var unitById = allUnits.ToDictionary(u => u.Id);

            //var defs = await _apiClient.GetPriceWorkflowDefinitionAsync(new PriceWorkflowDTO { CompanyId = CompanyId, BranchId = BranchId });

            //(int? wfId, bool requires) Resolve(decimal salePrice, string keyId = "SalePrice")
            //{
            //    var matches = defs.Where(d => d.KeyId == keyId && salePrice >= d.Price).OrderByDescending(d => d.Price).FirstOrDefault();

            //    return matches is null ? (null, false) : (matches.WorkflowID, true);
            //}
            var defs = await _apiClient.GetPriceWorkflowDefinitionAsync(new PriceWorkflowDTO { CompanyId = CompanyId, BranchId = BranchId });

            (int? wfId, bool requires) ResolveAny(ItemDTO item)
            {
                var saleMatch = defs
                    .Where(d => d.KeyId == "SalePrice" && item.SalePrice >= d.Price)
                    .OrderByDescending(d => d.Price)
                    .FirstOrDefault();

                var costValue = item.AvgCost; 
                var costMatch = defs
                    .Where(d => d.KeyId == "CostPrice" && costValue >= d.Price)
                    .OrderByDescending(d => d.Price)
                    .FirstOrDefault();

         

                if (saleMatch is null && costMatch is null)
                    return (null, false);

                var chosen = (saleMatch is null) ? costMatch
                           : (costMatch is null) ? saleMatch
                           : (saleMatch.Price >= costMatch.Price ? saleMatch : costMatch);

                return (chosen.WorkflowID, true);
            }

            object Map(ItemDTO item)
            {
                var resolved = ResolveAny(item);
                catById.TryGetValue(item.FK_CategoryId, out var cat);
                unitById.TryGetValue(item.FK_UnitId, out var unit);

                return new
                { 
                    id = item.Id,
                    code = item.Code,
                    primaryName = item.PrimaryName,
                    secondaryName = item.SecondaryName,
                    price = item.Price,
                    salePrice = item.SalePrice,
                    costPrice = item.AvgCost,
                    fK_UnitId = item.FK_UnitId,
                    fK_CategoryId = item.FK_CategoryId,
                    fK_SubCategoryId = item.FK_SubCategoryId,
                    subCategoryPrimaryName = item.SubCategoryPrimaryName,
                    subCategorySecondaryName = item.SubCategorySecondaryName,
                    locatorId = item.LocatorId,
                    locatorCode = item.LocatorCode,
                    avgCost = item.AvgCost,
                    availableQty = item.AvailableQty,
                    warehouseId = item.WarehouseId,
                    warehouse = lang == "en" ? item.WarehousePrimaryName : item.WarehouseSecondaryName,

                    categoryPrimaryName = cat?.primaryName,
                    categorySecondaryName = cat?.secondaryName,

                    isDecimalUnit = unit?.IsDecimal ?? false,
                    unitPrimaryName = unit?.primaryName,
                    unitSecondaryName = unit?.secondaryName,

                    requiresPriceApproval = resolved.requires,
                    priceWorkflowEnumId = resolved.wfId
                };
            }

            var ours = items
                .Where(i => myWarehouseIds.Contains(i.WarehouseId))
                .Select(Map)
                .ToList();

            var others = items
                .Where(i => !myWarehouseIds.Contains(i.WarehouseId) && i.AvailableQty > 0)
                .Select(Map)
                .ToList();

            return Json(new { ours, others });
        }

        public async Task<JsonResult> GetAlternativeItems (int ItemId, bool includeIndirectAlternatives)
        {
            var items = await _inventoryApiClient.GetAlternativeItems(ItemId, includeIndirectAlternatives);
            if (items == null) items = new List<ItemDTO>(); 
            return Json(new { items });
        }

        public async Task<JsonResult> GeneralRequest(int WIPId, string RequestDescription)
        {
            GeneralRequest oGeneralRequest = new GeneralRequest();
            oGeneralRequest.WIPId = WIPId;
            oGeneralRequest.CreatedBy = UserId;
            oGeneralRequest.RequestDescription = RequestDescription;
            int? result;

            result = await _apiClient.GeneralRequest(oGeneralRequest);
            if (result.HasValue)
            {
                return Json(new { success = result.HasValue });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        public async Task<JsonResult> GetCustomerById(int Id)
        {


            var Details = await _accountingApiClient.Customer_GetById(Id);

            var terms = Details.oLDBPaymentType;
            var vat = Details.SalesTaxGroupId;


            return Json(Details);
        }

        public async Task<decimal?> GetVatValueById(int VatId)
        {
            var vatDetails = await _accountingApiClient.GetTaxClassificationById(VatId);
            return vatDetails?.TaxRate;
        }

        public async Task<JsonResult> GetMatchas(int Id)
        {
            var Details = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(Id, CompanyId);
            return Json(Details);
        }

        public async Task<JsonResult> MappingItems(int itemId)
        {
            var _item = await _inventoryApiClient.GetItemByIdAsync(itemId);
        
            return Json(_item);
        }

        public async Task<IActionResult> GetSalesType([FromQuery] int accountId)
        {
            var result = await GetSalesTypeListAsync(accountId, CompanyId, lang);
            return Json(result);

        }
        private async Task<List<SelectListItem>> GetSalesTypeListAsync(int accountType, int CompanyId, string lang)
        {
            int headerId = accountType == (int)AccountTypeEnum.Internal ? 9 : 10;
            var matches = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(headerId, CompanyId);

            return matches.Select(sc => new SelectListItem //// add defult value if null
            {
                Value = sc.Id.ToString(),
                Text = lang == "en" ? sc.PrimaryName : sc.SecondaryName
            }).ToList(); /// حlease use completed name "sc"
        }

        [HttpPost]
        public async Task<JsonResult> WIPSChedule([FromBody] WIPSChedule oWIPSChedule)
        {
            var scheduleList = await _apiClient.WIPSCheduleInsert(oWIPSChedule);
            if (scheduleList != null)
            {
                UpdateService updateService = new UpdateService()
                {
                    WIPId = oWIPSChedule.WIPId,
                    RTSId = oWIPSChedule.RTSId,
                    KeyId = oWIPSChedule.KeyId,
                    Status = (int)LabourLineEnum.Booked
                };

                var updateResult = await UpdateServiceStatus(updateService);

                return Json(new
                {
                    success = true,
                    RTSId = updateService.RTSId,
                    KeyId = updateService.KeyId,
                    Status = updateService.Status
                });
            }
            return Json(new { success = false });
        }

        public async Task<JsonResult> ScheduleGetById(int RTSId, int WIPId, int KeyId)
        {
            WIPSChedule model = null;
            model = await _apiClient.WIP_SChedule_Get(RTSId, WIPId, KeyId);/// please fix the worning 
            return Json(model);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateServiceStatus([FromBody] UpdateService dto)
        {
            var result = await _apiClient.UpdateServiceStatus(dto);
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> UpdatePartStatus([FromBody] UpdatePartStatus dto)
        {
            var result = await _apiClient.UpdatePartStatus(dto);
            return Json(result);
        }

        public async Task<IActionResult> GetReturnParts(int Id)
        {
            var result = await _apiClient.GetReturnParts(Id);
            return Json(result);
        }

        public async Task<JsonResult> GetVehicleDetailsById(int Id, int WIPId)
        {
            WIPDTO dto = new WIPDTO();
            VehicleTabDTO oVehicleTabDTO = new VehicleTabDTO();

            //dto = await _apiClient.GetWIPByIdAsync(Id);
            var vehicleDetails = await _vehicleApiClient.GetVehicleDetails(Id, lang);

            if (WIPId > 0)
            {
                oVehicleTabDTO = await _apiClient.WIP_GetVehicleDetailsById(WIPId);
            }
            else
            {
                oVehicleTabDTO = new VehicleTabDTO();
            }

            VehicleTabDTO oVehicleTab = new VehicleTabDTO
            {
                VehicleId = dto.VehicleId,
                PlateNumber = vehicleDetails.PlateNumber,
                ChassisNo = vehicleDetails.ChassisNo,
                ManufacturerPrimaryName = vehicleDetails.RefManufacturers.ManufacturerPrimaryName,
                ManufacturerSecondaryName = vehicleDetails.RefManufacturers.ManufacturerSecondaryName,
                VehicleModelPrimaryName = vehicleDetails.RefVehicleModels.VehicleModelPrimaryName,
                VehicleModelSecondaryName = vehicleDetails.RefVehicleModels.VehicleModelSecondaryName,
                VehicleClassPrimaryName = vehicleDetails.RefVehicleClasses.VehicleClassPrimaryName,
                VehicleClassSecondaryName = vehicleDetails.RefVehicleClasses.VehicleClassSecondaryName,
                ManufacturingYear = vehicleDetails.ManufacturingYear,
                Color = vehicleDetails.Color,

                VehAdvisorNotes = oVehicleTabDTO?.VehAdvisorNotes ?? string.Empty,
                VehConcerns = oVehicleTabDTO?.VehConcerns ?? string.Empty,
                VehServiceDesc = oVehicleTabDTO?.VehServiceDesc ?? string.Empty,
                OdometerPrevious = oVehicleTabDTO?.OdometerPrevious ?? 0,
                OdometerCurrentIN = oVehicleTabDTO?.OdometerCurrentIN ?? 0,
                OdometerCurrentOUT = oVehicleTabDTO?.OdometerCurrentOUT ?? 0

            };

            return Json(oVehicleTab);
        }

        public async Task<JsonResult> GetWIPOptionsById(int Id)
        {
            WIPOptionsDTO oWIPOptionsDTO = await _apiClient.WIP_GetOptionsById(Id);
            return Json(oWIPOptionsDTO);
        }
        public async Task<int?> UpdateWIPOptions(WIPOptionsDTO oWIPOptionsDTO)
        {
            //WIPDTO dto = new WIPDTO();
            //dto = await _apiClient.GetWIPByIdAsync(oWIPOptionsDTO.Id);

            return await _apiClient.UpdateWIPOptions(oWIPOptionsDTO);
        }

        public async Task<IActionResult> GetWIPServiceHistory(int VehicleId, int WIPId)
        {
            try
            {
                var result = await _apiClient.GetWIPServiceHistory(VehicleId);
                var labourHistory = await _apiClient.WIPServiceHistoryDetails_GetLabours(VehicleId);
                var partsHistory = await _apiClient.WIPServiceHistoryDetails_GetParts(VehicleId);
                
                var productIds = partsHistory
                    .Where(p => p.Product.HasValue)
                    .Select(p => p.Product.Value)
                    .Distinct()
                    .ToList();


                var productLookup = new Dictionary<int, (string Name, string Code)>();

                foreach (var productId in productIds)
                {
                    var item = await _inventoryApiClient.GetItemByIdAsync(productId);
                    if (item != null)
                    {
                        productLookup[productId] = (
                            Name: lang == "en" ? item.PrimaryName : item.SecondaryName,
                            Code: item.Code
                            );

                    }
                }

                foreach (var part in partsHistory)
                {
                    if (part.Product.HasValue &&
                        productLookup.TryGetValue(part.Product.Value, out var productData))
                    {
                        part.ProductName = productData.Name;
                        part.Code = productData.Code;   
                    }
                }

                foreach (var item in result)
                {
                    item.HistoryLabours = labourHistory.Where(x => x.FK_WIPId == item.WIPId);
                    item.HistoryParts = partsHistory.Where(x => x.FK_WIPId == item.WIPId);
                    var branchInfo = await _erpApiClient.GetBranchById(item.BranchId);
                    item.Branch = lang == "en" ? branchInfo.BranchPrimaryName : branchInfo.BranchSecondaryName;
                }
                
                return PartialView("_History", result ?? new List<M_WIPServiceHistoryDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading WIP service history for vehicle {VehicleId}", VehicleId);
                return PartialView("_History", new List<M_WIPServiceHistoryDTO>());
            }
        }


        public async Task<IActionResult> GetWIPDetails(int VehicleId)
        {
            try
            {
                var result = await _apiClient.GetWIPServiceHistory(VehicleId);
                var labourHistory = await _apiClient.WIPServiceHistoryDetails_GetLabours(VehicleId);
                var partsHistory = await _apiClient.WIPServiceHistoryDetails_GetParts(VehicleId);
                foreach (var item in result)
                {
                    item.HistoryLabours = labourHistory.Where(record => record.FK_WIPId == item.WIPId);
                    item.HistoryParts = partsHistory.Where(record => record.FK_WIPId == item.WIPId);
                }
                return PartialView("_History", result ?? new List<M_WIPServiceHistoryDTO>());
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error loading WIP service history for vehicle {VehicleId}", VehicleId);
                return PartialView("_History", new List<M_WIPServiceHistoryDTO>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> GetLabourRate([FromBody] LabourRateFilterDTO filter)
        {
            WIPDTO dto = new WIPDTO();
            dto = await _apiClient.GetWIPByIdAsync((int)filter.WIPId);
            var vehicleDetails = await _vehicleApiClient.VehicleDefinitions_Find(dto.VehicleId);
            filter.Make = vehicleDetails.ManufacturerId;
            if (filter.TechnicianId != null)
            {
                var tech = await _apiClient.GetTechnicianByIdAsync((int)filter.TechnicianId);
                filter.Skills = tech.FK_SkillId;
            }
            var rate = await _apiClient.GetLabourRate(filter);
            return Json(rate);
        }

        #region Transfer
        [HttpGet]
        public async Task<IActionResult> TransferMoveIn(int movementId, int WIP_Id)
        {
            try
            {
                VehicleMovement movement = new VehicleMovement();
                //ToDo: Caching
                //if (cache.Get(string.Format(CacheKeys.ExternalWorkshop)) != null)
                //{
                //    movement.workshops = (List<POCO.WorkshopDefinition>)cache.Get(string.Format(CacheKeys.ExternalWorkshop));
                //}
                //else
                //{
                //    movement.workshops = await _apiClient.Workshop_GetInternalBycompany(CompanyId);
                //    cache.Set(string.Format(CacheKeys.ExternalWorkshop), movement.workshops, DateTimeOffset.Now.AddDays(10));
                //}

                var workshops = await _apiClient.WorkshopGetAllAsync(CompanyId);
                foreach (var item in workshops)
                {
                    item.Name = lang == "en" ? item.PrimaryName : item.SecondaryName;
                }
                ViewBag.Workshops = workshops;

                ViewBag.fuelLevels = await _vehicleApiClient.GetFuleLevel();
                movement.MovementId = movementId;

                var move = await _apiClient.GetVehicleMovementByIdAsync(movementId);
                movement.GregorianMovementDate = move.GregorianMovementDate;
                movement.ExitMeter = move.ReceivedMeter + 1;

                movement.RefVehicledefinitions = new VehicleDefinitions();
                movement.RefVehicledefinitions.ColVehicleSubStatus = await _vehicleApiClient.GetAllSubStatus(CompanyId, lang);

                var services = await _apiClient.WIP_GetServicesById(WIP_Id);

                ViewBag.Services = services.Where(x => x.Status == (int)LabourLineEnum.WaitingForLabour).Select(s => new
                {
                    Value = s.tableId.Value.ToString(),
                    Text = $"{s.Code} - {s.Description} - {s.KeyId}"
                }).ToList();

                return PartialView("_TransferMovement", movement);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<IActionResult> TransferMaintenanceMovemet(
            VehicleMovement movement,
            [FromForm] string SelectedServicesIds
            )
        {

            var resultJson = new TempData();

            try
            {
                var move = await _apiClient.GetVehicleMovementByIdAsync(movement.MovementId.Value);
                int AreaId = 1;//((CompanyBranch)Session["branchInfo"]).AreaId;
                var VehicleMovementStatus = await _apiClient.CheckVehicleMovementStatusAsync(move.VehicleID.Value);
                if (movement.GregorianMovementDate.Value.Date.Add(movement.ExitTime.Value) < VehicleMovementStatus.lastmovemnetDate)
                {
                    resultJson.IsSuccess = false;
                    resultJson.Message = "Cannot make out before last movement in " + VehicleMovementStatus.lastmovemnetDate;
                    return Json(resultJson);
                }
                var mian = await _apiClient.GetDMaintenanceCardsByMovementIdAsync(movement.MovementId.Value);

                movement.CompanyId = CompanyId;
                movement.CreatedBy = UserId;
                // movement.MoveOutWorkshopId = movement.MoveInWorkshopId; need some attention here :)
                movement.MoveOutWorkshopId = movement.MoveInWorkshopId;
                movement.MovementOut = true;
                movement.WorkshopId = BranchId;
                movement.Status = 4;
                movement.MasterId = move.MasterId;
                movement.WorkOrderId = move.WorkOrderId;
                movement.LastVehicleStatus = move.LastVehicleStatus;
                movement.IsExternal = move.IsExternal;
                movement.VehicleID = move.VehicleID;
                movement.IsPettyCash = false;
                var movements = await _apiClient.InsertVehicleMovementAsync(movement);
                await _apiClient.TransferMaintenanceMovement(movements.MovementId.Value, (int)movement.MoveInWorkshopId, movement.MasterId.Value, movement.Reason);

                var workshop = await _apiClient.GetWorkshopByIdAsync((int)movement.MoveInWorkshopId);
                int updated = await _apiClient.UpdateWIPServicesIsExternalAsync(SelectedServicesIds, (int)movement.MoveInWorkshopId);
                //What will happen with the new service logic???
                if (mian != null && mian.Count > 0)
                {
                    await _apiClient.UpdateWorkOrderStatusAsync(movement.WorkOrderId.Value, 3);
                }

                resultJson.IsSuccess = true;
                resultJson.Type = "success";
                return Json(resultJson);

            }
            catch (Exception)
            {
                resultJson.IsSuccess = false;
                resultJson.Type = "error";
                resultJson.Message = "Error Happend";
                return Json(resultJson);
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransferMovements()

        {

            try
            {
                VehicleMovement ovehicleMovement = new VehicleMovement();
                ovehicleMovement.workshops = new List<WorkShopDefinitionDTO>();
                ovehicleMovement.ColMovements = new List<VehicleMovement>();

                ovehicleMovement.ColMovements = await _apiClient.GetAllVehicleTransferMovementAsync(null, 1, BranchId);

                ovehicleMovement.vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
                ovehicleMovement.ExternalVehicleNams = await _vehicleApiClient.GetExteralVehicleName(lang);
                ovehicleMovement.ColBranches = await _erpApiClient.GetActiveBranchesByCompanyId(CompanyId);
                ovehicleMovement.workshops = (await _apiClient.WorkshopGetAllAsync(CompanyId, null, null, lang))?.ToList();

                //ToDo: Caching
                //if (cache.Get(string.Format(CacheKeys.VehiclesDDL, language)) != null)
                //{
                //    ovehicleMovement.vehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL, language));
                //}
                //else
                //{
                //    ovehicleMovement.vehicleNams = VehicleApi.GetVehiclesDDL(language, CompanyId);
                //    cache.Set(string.Format(CacheKeys.VehiclesDDL, language), ovehicleMovement.vehicleNams, DateTimeOffset.Now.AddDays(10));
                //}


                //ToDo: Caching
                //if (cache.Get(string.Format(CacheKeys.ExternalWorkshop)) != null)
                //{
                //    ovehicleMovement.workshops = (List<POCO.WorkshopDefinition>)cache.Get(string.Format(CacheKeys.ExternalWorkshop));
                //}
                //else
                //{
                //    ovehicleMovement.workshops = WorkshopAPI.Workshop_GetInternalBycompany(CompanyId, language);
                //    cache.Set(string.Format(CacheKeys.ExternalWorkshop), ovehicleMovement.workshops, DateTimeOffset.Now.AddDays(10));
                //}

                return View("TransferMovements", ovehicleMovement);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<IActionResult> TransferMovementIn(int Id)
        {
            VehicleMovement movement = new VehicleMovement();

            movement = await _apiClient.GetVehicleMovementByIdAsync(Id);
            //movement.ColMaintenanceCard = new List<MaintenanceCard>();
            //movement.ColMaintenanceCard = await _apiClient.GetMaintenanceCardByMasterId(movement.MasterId);
            movement.fuelLevels = await _vehicleApiClient.GetFuleLevel();
            var workshopDetails = await _apiClient.GetWorkshopByIdAsync(movement.MoveInWorkshopId ?? 0);

            movement.VatRate = (await _accountingApiClient.GetTaxClassificationById(workshopDetails.VatClassificationId ?? 0))?.TaxRate;
            movement.InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetAll(CompanyId, BranchId, 1, 2);
            movement.WIPServices = (await _apiClient.GetWIPServicesByMovementIdAsync(movement.MovementInId.Value))?.Where(s => s.IsExternal && s.Status == (int)LabourLineEnum.Tranfer 
                && s.ExternalWorkshopId == movement.MoveInWorkshopId).ToList();
            movement.WIPServices ??= (new List<CreateWIPServiceDTO>());

            // Add AccountDefinition to ViewBag for Petty Cash functionality
            try
            {
                var accountDefinition = await _apiClient.GetAccountDefinitionGetAsync(CompanyId);
                ViewBag.AccountDefinition = accountDefinition;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching account definition for company {CompanyId}", CompanyId);
                ViewBag.AccountDefinition = null;
            }
            //var services = await _apiClient.WIP_GetServicesById(WIP_Id);

            //DamageFilter damageFilter = new DamageFilter();
            //damageFilter.VehicleID = movement.VehicleID;
            //damageFilter.CompanyId = CompanyId;
            //damageFilter.language = language;
            //movement.Dameges = await _apiClient.GetDamages(damageFilter);

            return PartialView("_TransferMovementIn", movement);
        }

        [HttpPost]
        public async Task<IActionResult> TransferMoveIn(
        [FromForm] VehicleMovement movement,
        [FromForm] List<Models.WipServiceFixDto> Services,
        [FromForm] IFormFile file2)
        {
            var resultJson = new TempData();

            resultJson.Notification = new List<Notification>();
            var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            int? accountId = null;
            var accountTable = new AccountTable();
            var ColTaxClassification = await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            var zeroTaxClassification = ColTaxClassification.Where(a => a.TaxRate == 0).FirstOrDefault();

            try
            {
                var ExternalWorkshop = await _apiClient.GetWorkshopByIdAsync((int)movement.MoveOutWorkshopId);/// Change the ID to bigint
                if (!string.IsNullOrEmpty(movement.InvoceNo) && movement.TotalWorkOrder != null && movement.TotalWorkOrder > 0)
                {
                    bool IsValid = await _accountingApiClient.AccountSalesMaster_IsValidSupplierInvoiceNo((int)ExternalWorkshop.SupplierId, movement.InvoceNo);/// Change the ID to bigint
                    if (!IsValid)
                    {
                        resultJson.IsSuccess = false;
                        resultJson.Type = "error";
                        resultJson.Message = "InvoiceNo" + " " + movement.InvoceNo + " " + "Already Exist";
                        return Json(resultJson);
                    }
                }

                var VehicleMovementStatus = await _apiClient.CheckVehicleMovementStatusAsync(movement.VehicleID.Value);
                if (movement.GregorianMovementDate.Value.Date.Add(movement.ReceivedTime.Value) < VehicleMovementStatus.lastmovemnetDate)
                {
                    resultJson.IsSuccess = false;
                    resultJson.Type = "error";
                    resultJson.Message = "Cannot make In before last movement in" + " " + VehicleMovementStatus.lastmovemnetDate;
                    return Json(resultJson);
                }

                movement.CompanyId = CompanyId;
                movement.CreatedBy = UserId;
                movement.MovementIN = true;
                movement.WorkshopId = BranchId;
                movement.Status = 1;
                movement.IsExternal = true;
                movement.IsPettyCash = false;
                //Check
                //Movement.DamageId = Movement.ColMaintenanceCard[0].DamageId;
                var movements = await _apiClient.InsertVehicleMovementAsync(movement);
                if (Services != null && Services.Any())
                {
                    await _apiClient.UpdateWIPServicesExternalAndFixStatus(Services);
                }

                MovementInvoice invoice = new MovementInvoice();
             
                // files uploaded in the request
                var allFiles = Request.Form.Files;

                var guid = Guid.NewGuid().ToString();

                var invoiceFiles = allFiles
                    .Where(f => string.Equals(f.Name, "ExternalWorkshopInvoice", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (invoiceFiles.Any())
                {
                    for (int i = 0; i < invoiceFiles.Count; i++)
                    {
                        var file = invoiceFiles[i];

                        var validationResult = _fileValidationService.CheckFileTypeAndSize(file);
                        if (!validationResult.IsSuccess)
                        {
                            await _apiClient.UpdateWorkOrderInvoicingStatusAsync((int)movement.WorkOrderId);
                            continue;
                        }

                        var (savedRelativePath, savedFileName) =
                            await _fileService.SaveFileAsync(file, Path.Combine("ExternalWorkshopInvoice"));

                        invoice.FileName = savedFileName; 
                        invoice.FilePath = savedRelativePath;
                        invoice.MovementId = movements.MovementId.Value;
                        invoice.Invoice_Date = DateTime.Now;

                        await _apiClient.DExternalWorkshopInvoiceInsertAsync(invoice);
                    }
                }
                else
                {
                    await _apiClient.UpdateWorkOrderInvoicingStatusAsync((int)movement.WorkOrderId);
                }


                if (!string.IsNullOrEmpty(movement.InvoceNo) && movement.TotalWorkOrder != null && movement.TotalWorkOrder > 0)
                {
                    invoice.MovementId = movements.MovementId.Value;
                    invoice.MasterId = movement.MasterId.Value;
                    invoice.ExternalWorkshopId = Convert.ToInt32(movement.MoveOutWorkshopId);
                    invoice.InvoiceNo = movement.InvoceNo;
                    invoice.TotalInvoice = Convert.ToDecimal(movement.TotalWorkOrder);
                    invoice.WorkOrderId = Convert.ToInt32(movement.WorkOrderId);
                    invoice.DeductibleAmount = 0;
                    invoice.ConsumptionValueOfSpareParts =0;
                    invoice.Vat = movement.Vat ?? 0;// من لفيو اذا مش تاكسبل ابعتها صفر 
                    invoice.PartsCost = movement.PartsCost.Value;
                    invoice.LaborCost = movement.LaborCost.Value;

                    await _apiClient.WorkshopInvoiceInsertAsync(invoice);
                    AccountSales oAccountSales = new AccountSales();
                    oAccountSales.AccountSalesDetails = new List<AccountSalesDetails>();
                    oAccountSales.AccountSalesMaster = new AccountSalesMaster();

                    var oAccountSalesDetails = new AccountSalesDetails();
                    var Supplier = await _accountingApiClient.Supplier_Find(ExternalWorkshop.SupplierId.Value);
                    var VehicleDetails = new VehicleDefinitions();
                    var items = new List<Workshop.Core.DTOs.AccountingDTOs.Item>();
                    items = await _accountingApiClient.GetItemsByCategoryNo(-1, lang);
                    var InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetById((int)movement.InvoiceTypeId);

                    ///Create function to fill the object without doublicate code
                    oAccountSales.AccountSalesMaster = new AccountSalesMaster()
                    {

                        Total = invoice.LaborCost + invoice.PartsCost,
                        Net = invoice.TotalInvoice,
                        Final = invoice.LaborCost + invoice.PartsCost,
                        Tax = invoice.Vat,
                        AccSalesTypeNo = 8,
                        AccSalesDate = DateTime.Now,
                        InvoiceType = 2,
                        TypeSalesPurchasesID = (int)movement.InvoiceTypeId,
                        Notes = "Maintenance" + " WIP : " , // Get WIP No.
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
                        CustomerAccountNo = AccountList.Where(x => x.ID == Supplier.AccountNoPayableId).FirstOrDefault().AccountNo,
                    };

                    VehicleDetails = await _vehicleApiClient.GetVehicleDetails(movement.VehicleID.Value, lang);

                    if (invoice.PartsCost > 0)
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
                            Description = "Maintenance Parts " + "( " + VehicleDetails.PlateNumber + " ) " + " صيانة قطع غيار ",
                            Quantity = 1,
                            UnitQuantity = 1,
                            Price = invoice.PartsCost,
                            Total = invoice.PartsCost,
                            TaxValue = movement.Vat == 0 ? 0 : invoice.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100),///اذا فوق صفر ياخدها صفر
                            TaxClassificationId = movement.Vat == 0 ? zeroTaxClassification.TaxClassificationNo : items.Where(a => a.ItemNumber == -1).FirstOrDefault().TaxClassificationNo,
                            Final = invoice.PartsCost + (invoice.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100)),
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
                    if (invoice.LaborCost > 0)
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
                            Description = "Maintenance Labor " + "( " + VehicleDetails.PlateNumber + " ) " + " صيانة عمالة",
                            Quantity = 1,
                            UnitQuantity = 1,
                            Price = invoice.LaborCost,
                            Total = invoice.LaborCost,
                            TaxValue = movement.Vat == 0 ? 0 : invoice.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100), //??
                            TaxClassificationId = movement.Vat == 0 ? zeroTaxClassification.TaxClassificationNo : items.Where(a => a.ItemNumber == -2).FirstOrDefault().TaxClassificationNo,
                            Final = invoice.LaborCost + (invoice.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100)),
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

                    oAccountSales.AccountSalesMaster.UserId = UserId.ToString();
                    //ToDo Important
                    oAccountSales.AccountSalesMaster.CurrencyID = Supplier.CurrencyId;//((CompanyInfo)Session["CompanyInfo"]).CurrencyIDH;
                    oAccountSales.AccountSalesMaster.AccSalesBranch = BranchId;
                    oAccountSales.AccountSalesMaster.PaymentTerms = Supplier.oLDBPaymentType > 0 ? Supplier.oLDBPaymentType : 0;
                    oAccountSales.CompanyId = CompanyId;
                    oAccountSales.BranchId = BranchId;
                    oAccountSales.AccountSalesMaster.InventoryAccountId = InvoiceType.AccountId;
                    //ToDo Important
                    oAccountSales.CompanyType = 1; // ((CompanyInfo)Session["CompanyInfo"]).CompanyType;
                    await _accountingApiClient.AccountSalesMaster_Insert(oAccountSales);
                }

                //Posible to return to this logic
                //await _apiClient.UpdateDMaintenanceCardAsync(Movement.Card);
                //foreach (var item in Movement.ColMaintenanceCard)
                //{
                //    await _apiClient.FixDamage(Convert.ToInt32(item.WorkOrderId), item.status);
                //}
                //bool isUpated = await _apiClient.VehicleMovement_Status(Movement);

                //Overriddn
                //await _apiClient.UpdateDMaintenanceCardAsync(movement.Card);

                //foreach (var item in movement.ColMaintenanceCard)
                //{
                //    await _apiClient.FixWorkOrderAsync(item.WorkOrderId.Value, item.status.Value);
                //}
                //await _apiClient.UpdateVehicleMovementStatusAync(movement.MoveInWorkshopId.Value, movement.MasterId.Value);


                //Mark fixed services as fixed/not fixed (WIP_Service) this will be on IsFixed column
                resultJson.IsSuccess = true;
                resultJson.Type = "success";
                return Json(resultJson);


            }
            catch
            {
                resultJson.IsSuccess = false;
                resultJson.Type = "error";
                return Json(resultJson);
                ///save the error message in the Database
            }
        }

        [HttpPost]
        public async Task<JsonResult> CloseWIP(UpdateWIPDTO dto)
        {
            try
            {
                dto.ClosedBy = UserId;


                var isValid = await _apiClient.WIP_Validation(dto.Id); //0=valid
                var ExternalInvoice = new AccountSalesMaster();
                if (isValid == 0)
                {
                    dto.ItemsList = !string.IsNullOrEmpty(dto.Items)
                     ? System.Text.Json.JsonSerializer.Deserialize<IEnumerable<BaseItemDTO>>(dto.Items)
                     : new List<BaseItemDTO>();

                    dto.ServicesList = !string.IsNullOrEmpty(dto.Services)
                        ? System.Text.Json.JsonSerializer.Deserialize<IEnumerable<CreateWIPServiceDTO>>(dto.Services)
                        : new List<CreateWIPServiceDTO>();
                    var Internalinvoice = await _SaveInvoice(dto);
                    if (Internalinvoice.ID > 0)
                    {// Insert External Invoice
                        if (dto.AccountDetails.AccountType == AccountTypeEnum.Internal || dto.AccountDetails.PartialAccountType == AccountTypeEnum.Internal)
                        {
                            CreateWIPInvoiceDTO wIPInvoiceDTO = new CreateWIPInvoiceDTO
                            {
                                WIPId = dto.Id,
                                InvoiceNo = (int)Internalinvoice.TranNo,
                                InvoiceDate = Internalinvoice.TranDate,
                                TransactionMasterId = (int)Internalinvoice.ID,
                                Total = dto.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.Internal).Sum(x => x.CostPrice * (decimal)x.UsedQuantity),
                                Tax = 0,
                                Net = dto.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.Internal).Sum(x => x.CostPrice * (decimal)x.UsedQuantity),
                                InvoiceType = (int)AccountTypeEnum.Internal,
                                AccountType = (int)AccountTypeEnum.Internal,
                                CreatedBy = UserId
                            };
                            await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);
                            await _apiClient.WIP_Close(dto.Id, (int)dto.ClosedBy);

                        }

                    }
                    else if (Internalinvoice.TranNo == -1)
                    {
                        CreateWIPInvoiceDTO wIPInvoiceDTO = new CreateWIPInvoiceDTO
                        {
                            WIPId = dto.Id,
                            InvoiceNo = 0, // max +1
                            InvoiceDate = DateTime.Now,
                            Total = 0,
                            Tax = 0,
                            Net = 0,
                            InvoiceType = (int)AccountTypeEnum.Internal,
                            AccountType = (int)AccountTypeEnum.Internal,
                            CreatedBy = UserId
                        };
                        await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);
                        await _apiClient.WIP_Close(dto.Id, (int)dto.ClosedBy);
                        return Json(new { success = true });

                    }
                    if (dto.AccountDetails.AccountType == AccountTypeEnum.External || dto.AccountDetails.PartialAccountType == AccountTypeEnum.External)
                    {
                        ExternalInvoice = await SaveInvoice(dto);
                        if (ExternalInvoice.ID > 0 )
                        {// Insert External Invoice
                            if (dto.AccountDetails.AccountType == AccountTypeEnum.External || dto.AccountDetails.PartialAccountType == AccountTypeEnum.External)
                            {
                                CreateWIPInvoiceDTO wIPInvoiceDTO = new CreateWIPInvoiceDTO
                                {
                                    WIPId = dto.Id,
                                    InvoiceNo = (int)ExternalInvoice.AccSalesNo,
                                    InvoiceDate = ExternalInvoice.AccSalesDate,
                                    TransactionMasterId = (int)ExternalInvoice.MasterId,
                                    Total = ExternalInvoice.Total,
                                    Tax = ExternalInvoice.Tax,
                                    Discount = ExternalInvoice.Discount,
                                    Net = ExternalInvoice.Net,
                                    InvoiceType = (int)AccountTypeEnum.Internal,
                                    AccountType = (int)AccountTypeEnum.External,
                                    TransactionCostMasterId = (int)Internalinvoice.ID,
                                    CreatedBy = UserId

                                };
                                await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);

                            }
                            await _apiClient.WIP_Close(dto.Id, (int)dto.ClosedBy);

                        }
                        else 
                        {
                            return Json(new { success = false });
                        }
                    }

                        return Json(new { success = true });
                }
                else
                {
                    switch (isValid)
                    {
                        case -100:
                            return Json(new { success = false, message = "USER CONTEXT MISSING" });
                        case -101:
                            return Json(new { success = false, message = "WIP NOT FOUND" });
                        case -102:
                            return Json(new { success = false, message = lang == "en" ? "ALREADY CLOSED" : "أمر العمل مغلق مسبقاً" });
                        case -103:
                            return Json(new { success = false, message = lang == "en" ? "SERVICE NOT COMPLETED" : "الخدمة غير مكتملة" });
                        case -104:
                            return Json(new { success = false, message = lang == "en" ? "SERVICE TIME MISSING" : "وقت الخدمة غير مُدخل" });
                        case -105:
                            return Json(new { success = false, message = lang == "en" ? "PARTS RETURN PENDING" : "إرجاع القطع قيد الانتظار" });
                        case -106:
                            return Json(new { success = false, message = lang == "en" ? "PARTIAL INVOICE INCOMPLETE" : "الفاتورة الجزئية غير مكتملة" });
                        case -107:
                            return Json(new { success = false, message = lang == "en" ? "UPDATE AFFECTED UNEXPECTED ROWS" : "تم تحديث عدد صفوف غير متوقع" });
                        case -999:
                        default:
                            return Json(new { success = false, message = lang == "en" ? "An unknown error occurred." : "حدث خطأ غير معروف" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        public async Task<TransactionMaster> _SaveInvoice(UpdateWIPDTO oWIPDTO)
        {

            // Vechicle Details Internal
            var  saveTransaction = new TransactionMaster();
            string InternalType = "";
            var AccountTable = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId);
            var account = await _apiClient.GetAccountDefinitionGetAsync(CompanyId);
            decimal? totalInternal = oWIPDTO.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.Internal).Sum(x => x.CostPrice * (decimal)x.UsedQuantity);
            decimal? totalExternal = oWIPDTO.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.External).Sum(x => x.CostPrice * (decimal)x.UsedQuantity);
            var VehicleDetails = await _vehicleApiClient.GetVehicleDetails(oWIPDTO.VehicleId, lang);

            if (oWIPDTO.AccountDetails.AccountType == AccountTypeEnum.Internal || oWIPDTO.AccountDetails.PartialAccountType == AccountTypeEnum.Internal)
            {
                var InternalList = await _apiClient.GetLookupDetailByIdAsync((oWIPDTO.AccountDetails.SalesType > 0 && oWIPDTO.AccountDetails.AccountType == AccountTypeEnum.Internal) ? (int)oWIPDTO.AccountDetails.SalesType : (int)oWIPDTO.AccountDetails.PartialSalesType, 9, CompanyId);

                InternalType = InternalList.Code;
            }

            if (totalExternal > 0 || totalInternal > 0)
            {
                saveTransaction = await _accountingApiClient.SaveTransaction(VehicleDetails, AccountTable, account, CompanyId, BranchId, UserId, account.JournalId, totalInternal, totalExternal, DateTime.Now, "Close WIP No : " + oWIPDTO.Id, CurrencyId, InternalType);

            }
            else if(oWIPDTO.AccountDetails.AccountType== AccountTypeEnum.Internal)
            {
                saveTransaction = new TransactionMaster()
                {
                    TranNo = -1
                };
            }
            return saveTransaction;

        }

        //===========================================
        //
        //===========================================
        [HttpPost]
        public async Task<AccountSalesMaster> SaveInvoice(UpdateWIPDTO oWIPDTO)
        {

            // Vechicle Details Internal


            var result = new TempData();
            var customerId =  oWIPDTO.AccountDetails.CustomerId ?? oWIPDTO.AccountDetails.PartialCustomerId;
            var vat = oWIPDTO.AccountDetails.Vat ?? oWIPDTO.AccountDetails.PartialVat;

            var account = await _apiClient.GetAccountDefinitionGetAsync(CompanyId);
            var invoice = account.InvoiceTypeId; //to be invoice type only
            var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            var Customer = await _accountingApiClient.Customer_GetById(customerId.Value);
            int? accountId = null;
            var accountTable = new AccountTable();
            var oAccountSalesDetails = new AccountSalesDetails();
            AccountSales oAccountSales = new AccountSales();
            var VehicleDetails = await _vehicleApiClient.GetVehicleDetails(oWIPDTO.VehicleId, lang);
            var taxClass =await _accountingApiClient.GetTaxClassificationById(vat ?? 0);
            //oWIPDTO.AccountDetails.TaxClassificationId ?? oWIPDTO.AccountDetails.PartialVat
            var items = new List<Workshop.Core.DTOs.AccountingDTOs.Item>();
            items = await _accountingApiClient.GetItemsByCategoryNo(-1, lang);
            var InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetById(account.InvoiceTypeId);
            try
            {
                var language = lang;
                string guid = Guid.NewGuid().ToString();
                oAccountSales.AccountSalesMaster = new AccountSalesMaster();
                oAccountSales.AccountSalesDetails = new List<AccountSalesDetails>();

                // --- Services Labour -------------------
                foreach (var item in oWIPDTO.ServicesList.Where(x => x.AccountType == (int)AccountTypeEnum.External).ToList())
                {
                    if (item.Total > 0)
                    {
                        accountId = items.Where(a => a.ItemNumber == -6).FirstOrDefault()?.ItemSalesAccountId;
                        if (accountId == null)
                        {
                            throw new InvalidOperationException(lang == "en"
                                ? "Labour account is incomplete in accounting"
                                : "حساب العمالة غير مكتمل في المحاسبة");
                        }
                        //accountId = accountId == null ? InvoiceType.AccountId : accountId;
                        accountTable = new AccountTable();
                        accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                        oAccountSalesDetails = new AccountSalesDetails()
                        {
                            ItemNumber = items.Where(a => a.ItemNumber == -6).FirstOrDefault().ItemId,
                            UnitId = items.Where(a => a.ItemNumber == -6).FirstOrDefault().UnitId,
                            Discount = 0,
                            Description = item.LongDescription,
                            Quantity = item.StandardHours,
                            UnitQuantity = item.StandardHours,
                            Price = item.Rate - ((decimal)item.Discount/ item.StandardHours),
                            Total = item.Rate * item.StandardHours - (decimal)item.Discount,
                            TaxValue = (item.Rate * item.StandardHours - (decimal)item.Discount) * (taxClass.TaxRate / 100),
                            TaxClassificationId = taxClass.TaxClassificationNo,
                            Final = ((item.Rate * item.StandardHours) - (decimal)item.Discount) + (((item.Rate * item.StandardHours) - (decimal)item.Discount) * taxClass.TaxRate / 100),
                            CostsCentersNo = 0,
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

                }
                //----- Items -------------------------
                foreach (var item in oWIPDTO.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.External))
                {
                    var mapping = await _inventoryApiClient.GetItemByIdAsync(item.ItemId);

                    accountId = items.Where(a => a.ItemNumber == -5).FirstOrDefault()?.ItemSalesAccountId;
                    if (accountId == null)
                    {
                        throw new InvalidOperationException(lang == "en"
                            ? "Item account is incomplete in accounting"
                            : "حساب القطع غير مكتمل في المحاسبة");
                    }
                    //accountId = accountId == null ? InvoiceType.AccountId : accountId;
                    accountTable = new AccountTable();
                    accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                    oAccountSalesDetails = new AccountSalesDetails()
                    {
                        ItemNumber = items.Where(a => a.ItemNumber == -5).FirstOrDefault().ItemId,
                        UnitId = items.Where(a => a.ItemNumber == -5).FirstOrDefault().UnitId,
                        Discount = 0,
                        Description = lang=="en"?mapping.PrimaryName:mapping.SecondaryName,
                        Quantity = (decimal)item.UsedQuantity,
                        UnitQuantity = (decimal)item.UsedQuantity,
                        Price = item.Price,
                        Total = (item.Price*(decimal)item.UsedQuantity)   - (decimal)item.Discount,
                        TaxValue = ((item.Price * (decimal)item.UsedQuantity) - (decimal)item.Discount) * (taxClass.TaxRate / 100),
                        TaxClassificationId = taxClass.TaxClassificationNo,
                        Final = ((item.Price * (decimal)item.UsedQuantity) - (decimal)item.Discount) + (((item.Price * (decimal)item.UsedQuantity) - (decimal)item.Discount) * taxClass.TaxRate / 100),
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

                AccountSalesMaster AccountSalesMaster = new AccountSalesMaster()
                {

                    Total = oAccountSales.AccountSalesDetails.Sum(a => a.Total),
                    Net = oAccountSales.AccountSalesDetails.Sum(a => a.Total),
                    Discount = (decimal)oWIPDTO.ServicesList.Sum(x=>x.Discount)+(decimal)oWIPDTO.ItemsList.Sum(x=>x.Discount),
                    Final = oAccountSales.AccountSalesDetails.Sum(a => a.Total),
                    Tax = oAccountSales.AccountSalesDetails.Sum(a => a.TaxValue),
                    AccSalesTypeNo = 6,
                    AccSalesDate = DateTime.Now,
                    InvoiceType = 1,
                    TypeSalesPurchasesID = invoice,
                    Notes = "WIP : " + oWIPDTO.Id,
                    CustomerId = (int)Customer.Id,
                    Customer_DimensionsId = Customer.Customer_DimensionsId,
                    Vendor_DimensionsId = Customer.Vendor_DimensionsId,
                    LOB_DimensionsId = Customer.LOB_DimensionsId,
                    Regions_DimensionsId = Customer.Regions_DimensionsId,
                    Locations_DimensionsId = Customer.Locations_DimensionsId,
                    Item_DimensionsId = Customer.Item_DimensionsId,
                    Worker_DimensionsId = Customer.Worker_DimensionsId,
                    FixedAsset_DimensionsId = Customer.FixedAsset_DimensionsId,
                    Department_DimensionsId = Customer.Department_DimensionsId,
                    Contract_CC_DimensionsId = Customer.Contract_CC_DimensionsId,
                    City_DimensionsId = Customer.City_DimensionsId,
                    D1_DimensionsId = Customer.D1_DimensionsId,
                    D2_DimensionsId = Customer.D2_DimensionsId,
                    D3_DimensionsId = Customer.D3_DimensionsId,
                    D4_DimensionsId = Customer.D4_DimensionsId,
                    CustomerAccountNo = AccountList.Where(x => x.ID == Customer.AccountNoReceivableId).FirstOrDefault().AccountNo,
                    IsDiscountByInvoice=true
                };

                //-------------------------------------
                oAccountSales.AccountSalesMaster = AccountSalesMaster;
                oAccountSales.AccountSalesMaster.UserId = UserId.ToString();
                oAccountSales.AccountSalesMaster.CurrencyID = oWIPDTO.AccountDetails.CurrencyId == null ? CurrencyId : (int)oWIPDTO.AccountDetails.CurrencyId;
                oAccountSales.AccountSalesMaster.AccSalesBranch = BranchId;
                oAccountSales.AccountSalesMaster.PaymentTerms = Customer.oLDBPaymentType > 0 ? Customer.oLDBPaymentType : 0;
                oAccountSales.CompanyId = CompanyId;
                oAccountSales.BranchId = BranchId;
                oAccountSales.AccountSalesMaster.InventoryAccountId = InvoiceType.AccountId;
                oAccountSales.CompanyType = 1;
                var oAccountSalesMaster = await _accountingApiClient.AccountSalesMaster_Insert(oAccountSales);

                return oAccountSalesMaster;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                result.Message = "Error Happend";
                return new AccountSalesMaster();
            }
        }


        [HttpPost]
        public async Task<JsonResult> DeleteService(DeleteServiceDTO dto)
        {
            var isDeleted = await _apiClient.DeleteService(dto);
            if (isDeleted > 0)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public async Task<JsonResult> GetWIPByVehicleId(int id)
        {
            var result = await _apiClient.GetWIPByVehicleId(id);
            var db = result;

            var dto = new 
            {
                OpenWIPCount = db.OpenWIPCount,
                Previous = db.Previous,
                OpenWIPs = string.IsNullOrWhiteSpace(db.OpenWIPs) ? new List<string>() : db.OpenWIPs.Split(',').ToList(),
                PreviousWIPs = string.IsNullOrWhiteSpace(db.PreviousWIPs) ? new List<string>() : db.PreviousWIPs.Split(',').ToList()
            };
            return Json(new { success = true, data = dto });
        }

        #endregion


        public async Task<JsonResult> GetAvailableTechnicians([FromQuery] DateTime date, decimal duration)
        {
            var technicians = await _apiClient.GetAvailableTechniciansAsync(date, duration, BranchId, true);

            var data = technicians
                .Where(t => t.FreeIntervalsList != null && t.FreeIntervalsList.Any())
                .Select(t => new
            {
                value = t.TechnicianId,
                text = lang == "en" ? t.PrimaryName : t.SecondaryName,
                freeIntervalsList = t.FreeIntervalsList
            }).ToList();

            return Json(new { success = true, data = data });

        }

        [HttpPost]
        public async Task<JsonResult> GetAvailableLocators([FromBody] FilterLocatorDTO dto)
        {

            dto.StatusCsv = "";
            var List = await _inventoryApiClient.GetAvailableLocatorsDDL(dto);
            //var Warehouses = List.Select(t => new SelectListItem { Text = t.LocatorCode+ "(Available:" +t.OnHandQtyInUnit+")", Value = t.LocatorId.ToString() }).ToList();

            return Json(new { success = true, data = List });
        }

        [HttpPost]
        public async Task<JsonResult> CreateInventoryTransaction([FromBody] CreateInventoryTransactionDTO dto)
        {

            dto.CreatedBy = UserId;
            dto.BranchId = BranchId;
            dto.CompanyId = CompanyId;
            dto.TransactionDate = DateTime.Now.Date;
            var result = await _inventoryApiClient.GRNAdd(dto);
           
            if (!result.Success)
            {
                return Json(new
                {
                    success = false,
                    message = result.Message,
                    shortages = result.Shortages
                });
            }
            else
            {
                return Json(new { success = true, data = result });
            }

                //bool isSuccess = result != null;

        }

        public async Task<string> CreateIssueVoucher([FromForm] CreateInventoryTransactionDTO model)
        {
            var rawDetails = HttpContext.Request.Form["Details"];
            var wipId = int.Parse(HttpContext.Request.Form["WIPId"]);
            
            model.CompanyId = CompanyId;
            model.BranchId = BranchId;
            model.CreatedBy = UserId;
            model.TransactionDate = DateTime.Now;

            model.FK_TransactionStatusId = 2; //Posted;

            if (model.FK_TransactionTypeId == 1 || model.FK_TransactionTypeId == 4) // 1 -> ISSUE , 4 -> Transfer Out
                model.StockType = -1;

            if (model.FK_TransactionTypeId == 11) // 11 -> Reservation
                model.StockType = 0;

            if (!string.IsNullOrEmpty(rawDetails))
            {
                model.Details = JsonConvert.DeserializeObject<List<InventoryTransactionDetailsDTO>>(rawDetails);
            }
            int keyId = int.Parse(model.Details.First().KeyId);

            // Step 1: Create the inventory transaction first (without financial fields)
            var result = await _inventoryApiClient.GRNAdd(model);

            if (!result.Success)
            {
                return JsonConvert.SerializeObject(new
                {
                    success = false,
                    message = result.Message,
                    shortages = result.Shortages
                });
            }

            long headerId = result.HeaderId!.Value;

            UpdateIssueIdDTO dto = new UpdateIssueIdDTO
                {
                    IssueId = (int)headerId,
                    WIPId = wipId,
                    Id = keyId
                };
          
           var addIssueToWIP = await _apiClient.UpdateIssueIdToWIP(dto);

            if(model.FK_TransactionTypeId != 11)
            {

                // Step 2: If inventory transaction succeeded, create the accounting transaction
                try {
                    var accountDefinitions = await _inventoryApiClient.GetInventoryAccountDefinitions();
                    var TranTypeNo = accountDefinitions.FK_JournalNameId;
                    var AccountTable = _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId).Result;
                    var warehouse = await _inventoryApiClient.GetWarehouseByIdAsync((int)model.FK_WarehouseId);
                    var CreditAccount = AccountTable?.FirstOrDefault(a => a.ID == warehouse.FK_AccountId).AccountNo;
                    var DebitAccount = AccountTable?.FirstOrDefault(a => a.ID == accountDefinitions.FK_WIPAccountId).AccountNo;

                    var accountingResponse = await _accountingApiClient.SaveIssueTransaction(
                       TranTypeNo,
                       (decimal)model.Details.Sum(x => x.Total),// avg cost * Qty 
                       DebitAccount,
                       CompanyId,
                       BranchId,
                       UserId,
                       CreditAccount,
                       model.TransactionDate,
                       "WIP :" + wipId,
                       CurrencyId,
                       null
                    );

                    // Step 3: If accounting transaction succeeded, update financial fields
                    if (accountingResponse != null && accountingResponse.ID > 0)
                    {
                        var updateFinancialFieldsRequest = new UpdateInventoryTransactionFinancialFieldsDTO
                        {
                            HeaderId = result.HeaderId!.Value,
                            FinancialTransactionNo = accountingResponse.TranNo,
                            FinancialTransactionTypeNo = accountingResponse.TranTypeNo,
                            Fk_FinancialTransactionMasterId = accountingResponse.ID,
                            Fk_InvoiceType = accountDefinitions.FK_InvoiceTypeId
                        };

                        await _inventoryApiClient.UpdateFinancialFieldsAsync(updateFinancialFieldsRequest);
                    }
                    else
                    {
                        return JsonConvert.SerializeObject(new
                        {
                            success = false,
                            message = "Inventory transaction created but accounting transaction failed. Please contact administrator."
                        });
                    }
                }
                catch (Exception accountingEx)
                {
                    return JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = $"Inventory transaction created but accounting process failed: {accountingEx.Message}"
                    });
                }

                var responseString = await _inventoryApiClient.GetAllGRNByIdHead(headerId);

                var response = JsonSerializer.Deserialize<InventoryTransactionByIdDTO>(
                  responseString,
                  new JsonSerializerOptions
                  {
                      PropertyNameCaseInsensitive = true
                  });

                if (response == null || response.Details == null)
                {
                    Console.WriteLine(" No details in response!");
                    return null;
                }


                var itemUnitList = new List<BaseItemDTO>();

                foreach (var d in response.Details)
                {
                    var matchingDetail = model.Details?
                        .FirstOrDefault(x => x.FK_ItemId == d.FK_ItemId && x.FK_UnitId == d.FK_UnitId);


                    var baseItem = new BaseItemDTO
                    {
                        WIPId = model.TransactionReferenceNo.HasValue ? (int)model.TransactionReferenceNo : 0,
                        RequestId = model.RequestId,
                        ItemId = d.FK_ItemId,
                        fk_UnitId = d.FK_UnitId.HasValue ? (int)d.FK_UnitId : 0,
                        RequestQuantity = matchingDetail?.Quantity ?? 0,
                        Quantity = d.UnitQuantity,
                        UsedQuantity = 0,
                        CostPrice = d.Price,
                        SalePrice = d.Price,
                        ModifyBy = UserId,
                        AccountType = null,
                        Discount = 0,
                        Total = 0
                    };


                    itemUnitList.Add(baseItem);
                }
            }
        

            var responseToClient = new
                {
                    success = true,
                    partsIssueId = headerId,
                    wipId = wipId
                };

            return JsonConvert.SerializeObject(responseToClient);
        }


        [HttpPost]
        public async Task<JsonResult> UndoIssueVoucher(int PartsIssueId, int WIPId, int Id)
        {
            try
            {
                if (PartsIssueId <= 0)
                {
                    return Json(new { success = false, message = lang == "en" ? "You must issue the item first.": "يجب تنفيذ الصرف أولاً."});
                }

                var responseString = await _inventoryApiClient.GetAllGRNByIdHead(PartsIssueId);

                if (string.IsNullOrWhiteSpace(responseString))
                {
                    return Json(new{  success = false,message = lang == "en"? "Issue voucher not found or already undone.": "سند الصرف غير موجود أو تم التراجع عنه مسبقًا."});    
                }

                InventoryTransactionByIdDTO? response;
                try
                {
                    response = JsonSerializer.Deserialize<InventoryTransactionByIdDTO>(
                        responseString,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
                catch
                {
                    return Json(new
                    {
                        success = false, message = lang == "en" ? "Invalid issue voucher data." : "بيانات سند الصرف غير صالحة."
                    });
                }

                if (response == null)
                {
                    return Json(new
                    {
                        success = false, message = lang == "en"  ? "Issue voucher not found." : "سند الصرف غير موجود."
                           
                    });
                }

                if (response.Fk_FinancialTransactionMasterId == null || response.FinancialTransactionTypeNo == null)
                {
                    return Json(new {success = false,message = lang == "en"? "Issue voucher financial data is missing." : "البيانات المالية لسند الصرف غير مكتملة."});
                }

                var transTypeNo = Convert.ToInt32(response.FinancialTransactionTypeNo);
                var masterId = (int)response.Fk_FinancialTransactionMasterId;

                var rev = await _accountingApiClient.ReverseTransactionAsync(masterId, transTypeNo, CompanyId, BranchId, UserId);

                if (rev == null || rev.ID <= 0)
                {
                    return Json(new
                    {
                        success = false, message = lang == "en" ? "Failed to reverse accounting transaction." : "فشل عكس القيد المحاسبي."
                    });
                }

                var deleteDTO = new DeleteInventoryTransactionDTO
                {
                    ID = PartsIssueId,
                    ModifiedBy = UserId,
                    FK_FinancialTransactionReverseId = rev.ID
                };

                var deleteResult = await _inventoryApiClient.InventoryTransactionDelete(deleteDTO);
                if (deleteResult == 1)
                {
                    await _apiClient.UpdateIssueIdToWIP(new UpdateIssueIdDTO
                    {
                        IssueId = 0,
                        WIPId = WIPId,
                        Id = Id
                    });

                    await _apiClient.UpdatePartStatusForSingleItem(new UpdateSinglePartStatusDTO
                    {
                        WIPId = WIPId,
                        Id = Id,
                        StatusId = 36
                    });
                }
                if (deleteResult != 1)
                {
                    return Json(new
                    {
                        success = false, message = lang == "en" ? "Failed to undo issue voucher." : "فشل التراجع عن سند الصرف."
                    });
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        public async Task<JsonResult> UpdatePartStatusForSingleItem([FromBody] UpdateSinglePartStatusDTO dto)
        {
            try
            {
                var updated = await _apiClient.UpdatePartStatusForSingleItem(dto);

                if (updated.HasValue && updated.Value > 0)
                {
                    return Json(new { success = true, data = updated.Value });
                }

                return Json(new { success = false, errorMessage = "No rows were updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message });
            }
        }

        public async Task<List<int>> GetOurWarehouse()
        {
            var warehouses = await _inventoryApiClient.GetAllWarehousesDDL(null, 1);

            var branchId = BranchId;

            var result = warehouses
                .Where(w => !string.IsNullOrWhiteSpace(w.WorkshopBranchIds))
                .Where(w =>
                {
                    var ids = w.WorkshopBranchIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Select(s => int.TryParse(s, out var n) ? n : (int?)null)
                        .Where(n => n.HasValue)
                        .Select(n => n.Value);

                    return ids.Contains(branchId);
                })
                .Select(w => w.Id)
                .ToList();

            return result;
        }

        [HttpPost]
        public async Task<JsonResult> CreditNote(int WIPId)
        {
            var Reverse = new TransactionMaster();
            var InvoiceDetailsList = await _apiClient.WIPInvoiceGetById(WIPId, null);
            var account = await _apiClient.GetAccountDefinitionGetAsync(CompanyId);

            InvoiceDetailsList = InvoiceDetailsList.Where(x => x.ReferanceNo == null && x.InvoiceType == 1 && x.IsReturn == false).ToList();
            foreach (var item in InvoiceDetailsList)
            {
                if (item.AccountType == 1 &&item.TransactionMasterId>0)
                {
                    TransactionMaster ReverseTrans = new TransactionMaster()
                    {
                        ID = (int)InvoiceDetailsList.FirstOrDefault().TransactionMasterId,
                        TranTypeNo = account.JournalId,
                        TranTypeNoReverse = account.JournalId,
                        TranDate = DateTime.Now,
                        CreateBy = UserId,
                        CompanyId = CompanyId,
                        BranchId = BranchId,
                        IsCompanyCenterialized = 1,
                        VoucherType = 1,

                    };
                    Reverse = await _accountingApiClient.ReverseTrans(ReverseTrans);
                    if (Reverse.ID > 0)
                    {
                        CreateWIPInvoiceDTO wIPInvoiceDTO = new CreateWIPInvoiceDTO
                        {
                            WIPId = WIPId,
                            InvoiceNo = (int)Reverse.TranNo,
                            InvoiceDate = Reverse.TranDate,
                            TransactionMasterId = (int)Reverse.ID,
                            Total = Reverse.Total,
                            Tax = 0,
                            Discount = 0,
                            Net = Reverse.Total,
                            InvoiceType = -3,
                            AccountType = (int)AccountTypeEnum.Internal,
                            CreatedBy = UserId,
                            OldTransactionMasterId = item.TransactionMasterId


                        };
                        await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);
                    }
                }

                if (item.AccountType == 2)
                {
                    string Notes = "The Credit note relates to invoice number :" + " " + InvoiceDetailsList.FirstOrDefault().InvoiceNo + " " + "issued on :" + " " + InvoiceDetailsList.FirstOrDefault().InvoiceDate.Value.ToString("dd/MM/yyyy") + "\n  هذا الإشعار الدائن يتعلق بالفاتورة رقم :" + 1 + " " + " الصادرة بتاريخ :" + " " + DateTime.Now.ToString("dd/MM/yyyy");

                    TransactionMaster ReverseTrans = new TransactionMaster()
                    {
                        ID = (int)InvoiceDetailsList.FirstOrDefault().TransactionCostMasterId,
                        TranTypeNo = account.JournalId,
                        TranTypeNoReverse = account.JournalId,
                        TranDate = DateTime.Now,
                        CreateBy = UserId,
                        CompanyId = CompanyId,
                        BranchId = BranchId,
                        IsCompanyCenterialized = 1,
                        VoucherType = 1,
                    };
                    Reverse = await _accountingApiClient.ReverseTrans(ReverseTrans);
                    ReverseTrans = new TransactionMaster()
                    {
                        ID = (int)InvoiceDetailsList.FirstOrDefault().TransactionMasterId,
                        TranTypeNo = 6,
                        TranTypeNoReverse = 11,
                        TranDate = DateTime.Now,
                        CreateBy = UserId,
                        CompanyId = CompanyId,
                        BranchId = BranchId,
                        IsCompanyCenterialized = 1,
                        VoucherType = 1,
                        Notes = Notes
                    };
                    Reverse = await _accountingApiClient.ReverseTrans(ReverseTrans);
                    if (Reverse.ID > 0)
                    {
                        CreateWIPInvoiceDTO wIPInvoiceDTO = new CreateWIPInvoiceDTO
                        {
                            WIPId = WIPId,
                            InvoiceNo = (int)Reverse.TranNo,
                            InvoiceDate = Reverse.TranDate,
                            TransactionMasterId = (int)Reverse.ID,
                            Total = item.Total,
                            Tax = item.Tax,
                            Discount = item.Discount,
                            Net = item.Net,
                            InvoiceType = -3,
                            AccountType = (int)AccountTypeEnum.External,
                            CreatedBy = UserId,
                            OldTransactionMasterId = item.TransactionMasterId

                        };
                        await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);
                    }
                }

                if (item.AccountType == 1 && item.TransactionMasterId == null)
                {
                    CreateWIPInvoiceDTO wIPInvoiceDTO = new CreateWIPInvoiceDTO
                    {
                        WIPId = WIPId,
                        ReferanceNo = item.InvoiceNo, // max +1
                        InvoiceDate = DateTime.Now,
                        Total = 0,
                        Tax = 0,
                        Discount = 0,
                        Net = 0,
                        InvoiceType = -3,
                        AccountType = (int)AccountTypeEnum.Internal,
                        CreatedBy = UserId,
                        InvoiceNo=0

                    };
                    await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);
                }
            }

            bool isSuccess = Reverse.ID > 0;
            UpdateWIPStatusDTO updateWIPStatusDTO = new UpdateWIPStatusDTO()
            {
                WIPId = WIPId,
                StatusId = 2031,
            };
            int? Updated = await _apiClient.UpdateWIPStatus(updateWIPStatusDTO);
            return Json(new { success = isSuccess, data = Reverse });

        }
        public ActionResult PrintExternal(int InvoiceType, int InvoiceNo)
        {
            ViewBag.HostName = _configuration["ApiSettings:AccountingUrl"];
            ViewBag.InvoiceType = InvoiceType;
            ViewBag.InvoiceNo = InvoiceNo;

            return View();
        }
        public async Task<ActionResult> PrintInternal(int WIPId, int TransactionMasterId,int InvoiceNo,int InvoiceType)
        {
            var PrintInternalDTO = new PrintInternalDTO();
            var InvoiceDetailsList = await _apiClient.WIPInvoiceGetById(WIPId, TransactionMasterId);
            if (TransactionMasterId==0)
            {
                PrintInternalDTO.WipInvoiceDetail = await _apiClient.WipInvoiceByHeaderId(InvoiceDetailsList.Where(x=>x.InvoiceNo== InvoiceNo && x.InvoiceType== InvoiceType).FirstOrDefault().Id);
                InvoiceDetailsList = InvoiceDetailsList.Where(x => x.InvoiceNo == InvoiceNo && x.InvoiceType == InvoiceType).ToList();
            }
            else
            {
                PrintInternalDTO.WipInvoiceDetail = await _apiClient.WipInvoiceByHeaderId(InvoiceDetailsList.FirstOrDefault().Id);

            }

            PrintInternalDTO.InvoiceDetails = InvoiceDetailsList.FirstOrDefault();
            foreach (var item in PrintInternalDTO.WipInvoiceDetail)
            {
                if (item.ItemId != null)
                {
                    var mapping = await _inventoryApiClient.GetItemByIdAsync((int)item.ItemId);
                    if (mapping != null)
                    {
                        item.FullDescription = mapping.Code + " - " + (lang == "en" ? mapping.PrimaryName : mapping.SecondaryName);
                    }
                }
            }

            var details = await _apiClient.GetWIPByIdAsync(WIPId);

            PrintInternalDTO.CreatingOperator = await GetUserFullNameAsync(details?.CreatedBy as int? ?? details?.CreatedBy); 

            if (details?.Status == 2032) // closed
            {
                PrintInternalDTO.InvoicingOperator = await GetUserFullNameAsync(details.ClosedBy as int? ?? details.ClosedBy);
            }

            var lastMovement = await _apiClient.GetLastVehicleMovementByVehicleIdAsync(details.VehicleId);

            if (lastMovement?.MovementOut == true)
            {
                PrintInternalDTO.BookedOutOperator = await GetUserFullNameAsync(lastMovement.CreatedBy as int? ?? lastMovement.CreatedBy);
            }


            return View(PrintInternalDTO);
        }

        public async Task<JsonResult> hasExternalPendingInvoice(int WIPId)
        {
            try
            {
                var result = await _apiClient.GetInvoiceDetailsByWIPIdAsync(WIPId);

                return Json(new { success = true, hasPending = result?.Any() == true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });

            }
        }

        public async Task<IActionResult> RepairOrderRequest(int Id)
        {
            try
            {
                var Details = await _apiClient.GetWIPByIdAsync(Id);
                var vehicleId = Details.VehicleId;
                var accountDetails = await _apiClient.WIP_GetAccountById(Id) ?? new AccountDTO();
                var movement = await _apiClient.GetVehicleMovementByIdAsync((int)Details.MovementId);
                var services = await _apiClient.WIP_GetServicesById(Id);
                var workOrderDetials = await _apiClient.GetMWorkOrderByID((int)Details.WorkOrderId);
                var Complaint = workOrderDetials != null ? workOrderDetials.Description : string.Empty;
                var Branche = await _erpApiClient.GetBranchById((int)BranchId);
                var companyInfo = await _erpApiClient.GetCompanyById(CompanyId);

                RepairOrderRequestReportModel model = new RepairOrderRequestReportModel();
                model.WIPId = Details.Id;
                model.MovementId = Details.MovementId;
                model.VehicleNo = Details.VehicleId;
                model.Branch = Branche != null ? (lang == "en" ? Branche.BranchPrimaryName : Branche.BranchSecondaryName) : string.Empty;
                model.CompanyData.Branch = Branche != null ? (lang == "en" ? Branche.BranchPrimaryName : Branche.BranchSecondaryName) : string.Empty;
                model.Note = Details.Note;
                model.CompanyData.CompanyPrimaryName = companyInfo != null ? companyInfo.CompanyPrimaryName : string.Empty;
                model.DriverName = movement.ResivedDriverId;

                // Logo
                model.CompanyData.Img = companyInfo.Img;

                //var customer = "";
                if (accountDetails.CustomerId != null)
                {
                    var _customer = await _accountingApiClient.Customer_GetById((int)accountDetails.CustomerId);

                    //customer = _customer.CustomerPrimaryName;
                    model.AccountNo = _customer.AccountNoReceivable;
                }

                if(Details.AgreementId != null)
                {
                    var agreement = await _vehicleApiClient.Get_AgreementCustomerAndCompanyName((int)Details.AgreementId, lang);

                    model.Company = agreement.CompanyName;
                    model.CustomerName = agreement.CustomerName;
                    model.CustomerMobileNumber = agreement.CustomerPhoneNumber;
                }

                    var root = _configuration["FileUpload:DirectoryInsidePath"]; 
                //Images
                if (!string.IsNullOrWhiteSpace(movement.DamageImagePath) && !string.IsNullOrWhiteSpace(movement.DamageImageName))
                {
                    var fixedPath = movement.DamageImagePath
                    .Replace("\\", Path.DirectorySeparatorChar.ToString())
                    .Replace("/", Path.DirectorySeparatorChar.ToString());

                    var physicalPath = Path.Combine(_env.WebRootPath, root,
                          fixedPath,
                        movement.DamageImageName);

                    if (System.IO.File.Exists(physicalPath))
                    {
                        var imageBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
                        model.DamageImageBytes = imageBytes;

                        using (var inputStream = new MemoryStream(imageBytes))
                        using (var image = Image.FromStream(inputStream))
                        {
                            image.RotateFlip(RotateFlipType.Rotate90FlipNone);

                            using (var outputStream = new MemoryStream())
                            {
                                image.Save(outputStream, ImageFormat.Jpeg);
                                model.DamageImageBytes_Vertical = outputStream.ToArray();
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(movement.DriverSignature))
                {
                    var driverSignaturePath = Path.Combine(
                        _env.WebRootPath, root, movement.DriverSignature.Replace("/", Path.DirectorySeparatorChar.ToString())
                    );

                    if (System.IO.File.Exists(driverSignaturePath))
                    {
                        using var stream = new MemoryStream(await System.IO.File.ReadAllBytesAsync(driverSignaturePath));
                        using var image = Image.FromStream(stream);
                        using var bitmap = new Bitmap(image.Width, image.Height);
                        using var graphics = Graphics.FromImage(bitmap);

                        graphics.Clear(System.Drawing.Color.White);
                        graphics.DrawImage(image, 0, 0);

                        using var output = new MemoryStream();
                        bitmap.Save(output, ImageFormat.Png);
                        model.DriverSignatureBytes = output.ToArray();
                    }
                }

                if (!string.IsNullOrWhiteSpace(movement.EmployeeSignature))
                {
                    var employeeSignaturePath = Path.Combine(
                        _env.WebRootPath, root, movement.EmployeeSignature.Replace("/", Path.DirectorySeparatorChar.ToString())
                    );

                    if (System.IO.File.Exists(employeeSignaturePath))
                    {
                        using var stream = new MemoryStream(await System.IO.File.ReadAllBytesAsync(employeeSignaturePath));
                        using var image = Image.FromStream(stream);
                        using var bitmap = new Bitmap(image.Width, image.Height);
                        using var graphics = Graphics.FromImage(bitmap);

                        graphics.Clear(System.Drawing.Color.White);
                        graphics.DrawImage(image, 0, 0);

                        using var output = new MemoryStream();
                        bitmap.Save(output, ImageFormat.Png);
                        model.EmployeeSignatureBytes = output.ToArray();
                    }
                }

                //============================================================================================================
                var vehicleInfo = await GetVehicleInfoAsync(Details.VehicleId, (int)workOrderDetials?.VehicleType);
                //============================================================================================================
                model.VehicleInfo ??= new VehicleInfoModel();
                model.Date = DateTime.Now;
                model.TimeReceived = movement.ReceivedTime;
                model.VehicleInfo.Year = vehicleInfo.Year;
                model.VehicleInfo.PlateNumber = vehicleInfo.PlateNumber;
                model.VehicleInfo.ColorName = vehicleInfo.ColorName;
                model.VehicleInfo.VIN = vehicleInfo.VIN;
                model.VehicleInfo.Make = vehicleInfo.Make;
                model.VehicleInfo.Model = vehicleInfo.Model;
                model.VehicleInfo.Mileage = vehicleInfo.Mileage?? movement.ReceivedMeter;
                model.ContractExpDate = await GetContractExpDateAsync(Details.VehicleId);
                
                model.InsuranceExpDate = await VehicleDocumants(Details.VehicleId, 5); 
                model.EstimaraExpDate = await VehicleDocumants(Details.VehicleId, 2);
                model.MVPIExpDate = await VehicleDocumants(Details.VehicleId, 6); 
                model.RegistrationExpDate = await VehicleDocumants(Details.VehicleId, 8); 
                model.Complaint = Complaint;
                model.DateIn = movement.GregorianMovementDate?.ToString("yyyy-MM-dd");
                model.TimeIn = movement.CreatedAt?.ToString("HH:mm"); 
                var lastMovement = await _apiClient.GetLastVehicleMovementByVehicleIdAsync(Details.VehicleId);
                if (lastMovement?.MovementOut == true)
                {
                    model.DateOut = lastMovement.CreatedAt?.ToString("yyyy-MM-dd");
                    model.TimeOut = lastMovement.CreatedAt?.ToString("HH:mm");
                }
                if(movement.FuelLevelId != null)
                {
                    model.FuelLevel = movement.FuelLevelId + "%" ;
                }
                model.Services = services?.ToList();
                var Items = await GetItemsModelsAsync(Id, lang);
                model.Items = Items ?? new List<ItemModel>();
                model.VehicleCkecklist = await GetVehicleChecklistAsync(Details.MovementId);
                model.TyreCkecklist = await GetTyreChecklistAsync(Details.MovementId);
                model.CreatedBy = await GetUserFullNameAsync(Details.CreatedBy);
                var user = await _erpApiClient.GetUserInfoById((int)Details.CreatedBy);
                model.UserPhoeNo = user.PhoneNo;
                model.CreatedDate = Details.CreatedAt?.ToString("dd-MM-yyyy");
                var options = await _apiClient.WIP_GetOptionsById(Id);
                model.RepeatRepair = options.RepeatRepair == true ? "Yes" : "No";
                var RegDoc = await _vehicleApiClient.Documants_GetByVehicleIdAndSystemTypeId(vehicleId, 8);
                model.RegistrationNo = RegDoc?.Number;
                if (vehicleInfo.VIN != null)
                {

                    var recallResponse = await _apiClient.GetActiveRecallsByChassis(vehicleInfo.VIN);
                    model.Recalls = recallResponse?.Recalls ?? new List<ActiveRecallDto>();
                    model.RecallListText = string.Join(", ", model.Recalls
                            .Where(r => !string.IsNullOrWhiteSpace(r.Title))
                            .Select(r => r.Title.Trim())
                    );

                }

                if ((int)workOrderDetials?.VehicleType == 2)
                {
                    var vehicleDetails = await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(Details.VehicleId);
                    var allCustomers = await _vehicleApiClient.Get_CustomerInformation(BranchId, "en", null);

                    if (vehicleDetails?.CompanyId is int companyId && companyId > 0)
                    {
                        var companyCustomer = allCustomers?.FirstOrDefault(c => c.Id == vehicleDetails?.CompanyId);

                        model.CompanyName = lang == "en" ? companyCustomer?.CustomerPrimaryName : companyCustomer.CustomerSecondaryname;
                    }
                }

                //return View(model);
                //Crystal Report
                var bytes = await _reportsServiceApiClient.RepairOrderRequestReportAsync(model);

                Response.Headers["Content-Disposition"] = "inline; filename=Wip.pdf";
                return File(bytes, "application/pdf");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "An error occurred in RepairOrderRequestReport");
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStrike(int movementId)
        {
            string strike = await _apiClient.GetVehicleMovementStrikeAsync(movementId);
            return Json(strike);
        }
        [HttpPut]
        public async Task<JsonResult> UpdateRecallVehicleStatus(string chassisNo)
        {
            try
            {
                var updated = await _apiClient.UpdateRecallVehicleStatusAsync(chassisNo, (int)RecallStatusEnum.Done);

                return Json(new
                {
                    success = true,
                    updated
                });
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                    updated = 0
                });
            }
        }



        private async Task<string?> GetContractExpDateAsync(int vehicleId)
        {
            try
            {
                var activeAgreement = await _vehicleApiClient.GetActiveAgreementId(vehicleId);

                if (activeAgreement?.AgreementId != null && activeAgreement.AgreementId > 0)
                    return activeAgreement.GregorianReturnDate?.ToString("yyyy-MM-dd");


                return null;

            }catch(Exception ex)
            {
                throw ex;
            }
            
        }

        private async Task<VehicleInfoModel> GetVehicleInfoAsync(int vehicleId, int? vehicleType)
        {
            try
            {
                var allManufacturers = await GetMakes();
                var allModels = await GetModels();
                var VehiclesColors = await _vehicleApiClient.GetAllColors(lang);

                if (vehicleType == (int)VehicleTypeId.Internal)
                {
                    var v = (await _vehicleApiClient.VehicleDefinitions_Find(vehicleId)) ?? new VehicleDefinitions();

                    return new VehicleInfoModel
                    {
                        Year = v.ManufacturingYear,
                        PlateNumber = v.PlateNumber,
                        ColorName = VehiclesColors?.FirstOrDefault(c => c?.Id == v.Color)?.Name,
                        VIN = v.ChassisNo,
                        Make = allManufacturers?.FirstOrDefault(i => i.Id == v.ManufacturerId)?.ManufacturerPrimaryName,
                        Model = allModels?.FirstOrDefault(i => i.Id == v.VehicleModelId)?.VehicleModelPrimaryName
                    };
                }
                else
                {
                    var v = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(vehicleId))
                            ?? new CreateVehicleDefinitionsModel();

                    return new VehicleInfoModel
                    {
                        Year = v.ManufacturingYear,
                        PlateNumber = v.PlateNumber,
                        ColorName = VehiclesColors?.FirstOrDefault(c => c?.Id == v.Color)?.Name,
                        VIN = v.ChassisNo,
                        Make = allManufacturers?.FirstOrDefault(i => i.Id == v.ManufacturerId)?.ManufacturerPrimaryName,
                        Model = allModels?.FirstOrDefault(i => i.Id == v.VehicleModelId)?.VehicleModelPrimaryName
                    };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task<List<ItemModel>> GetItemsModelsAsync(int id, string lang)
        {
            try
            {
                var items = await _apiClient.WIP_GetItemsById(id, lang);

                var uniqueIds = items
                    .Select(x => x.ItemId)
                    .Distinct()
                    .ToList();

                var details = await Task.WhenAll(
                    uniqueIds.Select(itemId => _inventoryApiClient.GetItemByIdAsync(itemId))
                );

                var dict = details
                    .Where(d => d != null)
                    .ToDictionary(d => d.Id);

                var result = items
                    .Select(x =>
                    {
                        if (!dict.TryGetValue(x.ItemId, out var d))
                            return null;

                        return new ItemModel
                        {
                            Id = d.Id,
                            Code = d.Code,
                            ItemNumber = d.ItemNumber,
                            Name = d.Name,
                            PrimaryName = d.PrimaryName,
                            SecondaryName = d.SecondaryName,
                            UnitPrimaryName = d.UnitPrimaryName,
                            UnitSecondaryName = d.UnitSecondaryName,
                            Qty = x.Quantity
                        };
                    })
                    .Where(m => m != null)
                    .Cast<ItemModel>()
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task<List<VehicleChecklist>> GetVehicleChecklistAsync(int movementId)
        {
            try
            {
                var checklistsTask = await _apiClient.GetVehicleChecklistByMovementId(movementId);
                var lookupTask = await _apiClient.GetVehicleChecklistLookup();

                //await Task.WhenAll(checklistsTask, lookupTask);

                var checklists = (checklistsTask) ?? new List<VehicleChecklist>();
                var lookup = (lookupTask) ?? new List<VehicleChecklistLookup>();

                var lookupById = lookup.ToDictionary(x => x.Id);

                foreach (var item in checklists)
                {
                    if (lookupById.TryGetValue(item.LookupId, out var lu))
                    {
                        item.LookupPrimaryDescription = lu.PrimaryDescription;
                        item.LookupSecondaryDescription = lu.SecondaryDescription;
                    }
                }
                if (checklists == null || checklists.Count() == 0)
                {
                    var vChecklistsTemp = new List<VehicleChecklist>();
                    foreach (var item in lookupTask ?? Enumerable.Empty<VehicleChecklistLookup>())
                    {
                        vChecklistsTemp.Add(new VehicleChecklist
                        {
                            LookupPrimaryDescription = item.PrimaryDescription,
                            LookupSecondaryDescription = item.SecondaryDescription,
                            Pass = false
                        });
                    }
                    checklists = vChecklistsTemp;
                }
                return checklists.ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<List<TyreChecklist>> GetTyreChecklistAsync(int movementId)
        {
            try
            {
                var checklistsTask = await _apiClient.GetTyresChecklistByMovementId(movementId);
                var lookupTask = await _apiClient.GetTyreChecklistLookup();

                //await Task.WhenAll(checklistsTask, lookupTask);

                var checklists = (checklistsTask) ?? new List<TyreChecklist>();
                var lookup = (lookupTask) ?? new List<TyreChecklistLookup>();

                var lookupById = lookup.ToDictionary(x => x.Id);

                foreach (var item in checklists)
                {
                    if (lookupById.TryGetValue(item.LookupId, out var lu))
                    {
                        item.LookupPrimaryDescription = lu.PrimaryDescription;
                        item.LookupSecondaryDescription = lu.SecondaryDescription;
                    }
                }

                if (checklists == null || checklists.Count() == 0)
                {
                    var tChecklistTemp = new List<TyreChecklist>();
                    foreach (var item in lookupTask ?? Enumerable.Empty<TyreChecklistLookup>())
                    {
                        tChecklistTemp.Add(new TyreChecklist
                        {
                            LookupPrimaryDescription = item.PrimaryDescription,
                            LookupSecondaryDescription = item.SecondaryDescription
                        });
                    }
                    checklists = tChecklistTemp;
                }

                return checklists.ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<string> GetUserFullNameAsync(int? userId)
        {
            if (userId is null) return string.Empty;

            var user = await _erpApiClient.GetUserInfoById(userId.Value);
            var first = user?.FirstName?.Trim();
            var last = user?.LastName?.Trim();

            return string.Join(" ",
                new[] { first, last }.Where(x => !string.IsNullOrWhiteSpace(x))
            );
        }
        private async Task<string> VehicleDocumants(int vehicleId, int type)
        {
            var vehicleDoc = await _vehicleApiClient.Documants_GetByVehicleIdAndSystemTypeId(vehicleId, type);
            return vehicleDoc?.ExpiryDate?.ToString("dd-MM-yyyy") ?? "";
        }


        [HttpPost]
        public async Task<JsonResult> UpdateWIPStatus([FromBody] UpdateWIPStatusDTO dto)
        {
            int? result = await _apiClient.UpdateWIPStatus(dto);
            return Json(result);
        }


        [HttpPost]
        [CustomAuthorize(Permissions.WIP.Create)]
        public async Task<IActionResult> Insert_Items(ItemsDTO dto)
        {
            try
            {

                int? result;

                var success = 0;

                    var newWip = new ItemsDTO
                    {
                        WIPId = dto.WIPId,
                        ItemsList = dto.ItemsList
                    };

                    success = await _apiClient.WIPInsertItemsAsync(newWip) ?? 0;
               

                if (success > 0)
                {
                    return Json(new { success = true, wipId = success });
                }
                return Json(new { success = false });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        private async Task<VehicleDocumentModel> GetVehicleDocumentDatesAsync(int vehicleId)
        {
            var result = new VehicleDocumentModel
            {
                RegDate = "N/A",
                MOTDate = "N/A"
            };

            try
            {
                var vehicleDoc = await _vehicleApiClient.Documants_GetByVehicleIdAndSystemTypeId(vehicleId, 8); // Reg
                result.RegDate = vehicleDoc?.strExpiryDate ?? "N/A";

                var motDoc = await _vehicleApiClient.Documants_GetByVehicleIdAndSystemTypeId(vehicleId, 3); // MOT
                result.MOTDate = motDoc?.strExpiryDate ?? "N/A";

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching vehicle documents for vehicle {VehicleId}", vehicleId);
                return result;
            }
        }
        private async Task<IEnumerable<LookupDetailsDTO>> GetInternalMatchesAsync()
        {
            try
            {
                return await _apiClient.GetAllLookupDetailsByHeaderIdAsync(9, CompanyId)?? new List<LookupDetailsDTO>();
                       
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching internal matches");
                return new List<LookupDetailsDTO>();
            }
        }

        private async Task<AccountDTO> GetAccountDetailsAsync(int wipId)
        {
            try
            {
                return await _apiClient.WIP_GetAccountById(wipId) ?? new AccountDTO();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching account details for WIP {WIPId}", wipId);
                return new AccountDTO();
            }
        }

        private async Task<SalesTypeModel> GetSalesTypesAsync(AccountDTO accountDetails)    
        {
            var result = new SalesTypeModel
            {
                SalesType = new List<SelectListItem>(),
                PartialSalesType = new List<SelectListItem>()
            };

            if (accountDetails == null)
                return result;

            try
            {
                var accType = (int)accountDetails.AccountType;
                var pAccType = (int)(accountDetails.PartialAccountType == 0
                    ? accountDetails.AccountType
                    : accountDetails.PartialAccountType);

                result.SalesType = await GetSalesTypeListAsync(accType, CompanyId, lang) ?? new List<SelectListItem>();
                result.PartialSalesType = await GetSalesTypeListAsync(pAccType, CompanyId, lang) ?? new List<SelectListItem>();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching sales type");
                return result;
            }
        }

        private async Task<IEnumerable<CreateWIPServiceDTO>> GetWipServicesAsync(int wipId)
        {
            try
            {
                return await _apiClient.WIP_GetServicesById(wipId, lang) ?? new List<CreateWIPServiceDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching services for WIP {WIPId}", wipId);
                return new List<CreateWIPServiceDTO>();
            }
        }

        private async Task<WipItemsModel> GetWipItemsAsync(int wipId)
        {
            var result = new WipItemsModel
            {
                Items = new List<CreateItemDTO>(),
                AllowActions = false
            };

            try
            {
                var items = (await _apiClient.WIP_GetItemsById(wipId, lang))?.ToList()?? new List<CreateItemDTO>();
                           
                foreach (var item in items)
                {
                    try
                    {
                        var mapping = await _inventoryApiClient.GetItemByIdAsync(item.ItemId);
                        if (mapping != null)
                        {
                            item.Code = mapping.Code;
                            item.Name = lang == "en" ? mapping.PrimaryName : mapping.SecondaryName;
                        }

                         item.fk_UnitId = item.fk_UnitId;
                        item.Status = item.Status;
                        item.RequiresApproval = (bool)item.RequiresPriceApproval ? "Requires Approval" : "";
                        item.PriceWorkflowStatusText = ((PriceWorkflowStatusEnum)item.PriceWorkflowStatus).ToString();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error mapping item {ItemId} for WIP {WIPId}", item.ItemId, wipId);
                    }
                }

                result.Items = items;
                result.AllowActions = items.Any();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching items for WIP {WIPId}", wipId);
            }

            return result;
        }

        private async Task<List<SelectListItem>> GetUnitsSelectListAsync()
        {
            try
            {
                var units = await _inventoryApiClient.GetAllUnitDDL();

                return units?.Select(t => new SelectListItem
                {
                    Text = lang == "en" ? t.primaryName : t.secondaryName,
                    Value = t.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching units");
                return new List<SelectListItem>();
            }
        }

        private async Task<List<SelectListItem>> GetWarehousesSelectListAsync()
        {
            try
            {
                var warehouses = await _inventoryApiClient.GetAllWarehousesDDL(null, 1);

                return warehouses?.Select(t => new SelectListItem
                {
                    Text = lang == "en" ? t.PrimaryName : t.SecondaryName,
                    Value = t.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching warehouses");
                return new List<SelectListItem>();
            }
        }

        private async Task<MovementOperatorsViewModel> GetMovementOperatorsAsync(int? movementId, WIPDTO dto)
        {
            try {
                var result = new MovementOperatorsViewModel();

                // current movement (IN)
                VehicleMovement currentMovement = null;

                if (movementId.HasValue && movementId > 0)
                {
                    currentMovement = await _apiClient.GetVehicleMovementByIdAsync(movementId.Value);

                }
                else if (dto?.MovementId > 0)
                {
                    currentMovement = await _apiClient.GetVehicleMovementByIdAsync(dto.MovementId);
                }

                if (currentMovement != null)
                {
                    result.DueInDate = currentMovement.CreatedAt?.ToString("yyyy-MM-dd");
                    result.ReceivedMeter = currentMovement.ReceivedMeter;
                    result.CreatingOperator = await GetUserFullNameAsync(currentMovement.CreatedBy);

                }

                // last movement (OUT)
                if (dto?.VehicleId > 0)
                {
                    var lastMovement = await _apiClient.GetLastVehicleMovementByVehicleIdAsync(dto.VehicleId);
                    if (lastMovement?.MovementOut == true)
                    {
                        result.DueOutDate = lastMovement.CreatedAt?.ToString("yyyy-MM-dd");
                        result.BookedOutOperator = await GetUserFullNameAsync(lastMovement.CreatedBy);
                    }
                }

                // invoicing operator
                if (dto?.Status == 2032 && dto.ClosedBy > 0)
                {
                    result.InvoicingOperator = await GetUserFullNameAsync(dto.ClosedBy);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching movement operators for movement {MovementId} and WIP {WIPId}", movementId, dto?.Id);
                return new MovementOperatorsViewModel();
            }
          
        }

        public async Task<JsonResult> CheckExistWIP(int VehicleId)
        {
            //var getWIPByMovement = await _apiClient.GetWIPByMovementId(movementId);
            var getOpenWIP = await _apiClient.GetWIPByVehicleId(VehicleId);
            if(getOpenWIP != null && getOpenWIP.OpenWIPCount > 0)
            {
                return Json(new { exist = true });
            }
            else
            {
                return Json(new { exist = false });
            }
        }
        public async Task<JsonResult> GetItemUnits(int itemId)
        {
            var units = await _inventoryApiClient.GetItemUnitByIdAsync(itemId);

            var result = units.Select(u => new
            {
                itemId = u.ItemId,
                unitId = u.UnitId,
                unitCode = u.UnitCode,
                unitPrimaryName = u.UnitPrimaryName,
                unitSecondaryName = u.UnitSecondaryName,
                conversionFactor = u.ConversionFactor,
                isBaseUnit = u.IsBaseUnit,
                IsDecimalUnit = u.IsDecimalUnit
            }).ToList();

            return Json(result);
        }

        //=====================================================================================================
        #region Petty Cash Methods

        [HttpGet]
        public async Task<JsonResult> GetPettyCashRequests()
        {
            try
            {
                // Add null check for UserId
                if (UserId <= 0)
                {
                    _logger.LogWarning("GetPettyCashRequests called with invalid UserId: {UserId}", UserId);
                    return Json(new List<object>());
                }

                var approvedRequestNo = await _accountingApiClient.PettyCashRequest_GetApproved(UserId);
                var result = new List<object>();

                if (approvedRequestNo > 0)
                {
                    var request = await _accountingApiClient.PettyCashRequest_GetById((int)approvedRequestNo);
                    if (request != null)
                    {
                        result.Add(new
                        {
                            requestNo = request.RequestNo,
                            reasonForRequest = request.Description ?? "Petty Cash Request",
                            requestAmount = request.RequestAmount
                        });
                    }
                }
                ViewBag.Currency = CurrencyId;
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting petty cash requests for user {UserId}", UserId);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetExpenseTypes()
        {
            try
            {
                var expenseTypes = await _accountingApiClient.ExpenseType_Get(CompanyId);
                var result = expenseTypes.Select(e => new
                {
                    id = e.Id,
                    primaryName = e.PrimaryName,
                    secondaryName = e.SecondaryName
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expense types for company {CompanyId}", CompanyId);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetPettyCashSellers()
        {
            try
            {
                var sellers = await _accountingApiClient.PettyCashSeller_Get(CompanyId);
                var result = sellers.Select(s => new
                {
                    id = s.Id,
                    primaryName = s.PrimaryName,
                    secondaryName = s.SecondaryName,
                    taxNumber = s.TaxNumber
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting petty cash sellers for company {CompanyId}", CompanyId);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCurrencies()
        {
            try
            {
                var currencies = await _erpApiClient.GetCurrecy(CompanyId, BranchId, lang);
                var result = currencies.Select(c => new
                {
                    currencyId = c.CurrencyID,
                    currencyCode = c.CurrencyCode,
                    currencyName = lang == "en" ? c.CurrencyPrimaryName : c.CurrencySecondlyName
                }).ToList();

                return Json(new
                {
                    currencies = result,
                    selectedCurrency = CurrencyId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting currencies for company {CompanyId}", CompanyId);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetTaxClassifications()
        {
            try
            {
                var taxClassifications = await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang);
                var result = taxClassifications.Select(t => new
                {
                    taxClassificationNo = t.TaxClassificationNo,
                    name = t.Name,
                    taxRate = t.TaxRate
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tax classifications for company {CompanyId}", CompanyId);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetRequestBalance(long requestNo)
        {
            try
            {
                var balance = await _accountingApiClient.PettyCashExpenses_UserBalanceByRequest(requestNo);
                return Json(balance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting request balance for request {RequestNo}", requestNo);
                return Json(0);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetExpenseTypesByRequest(long requestNo)
        {
            try
            {
                // This method might need to be implemented in the API if it doesn't exist
                // For now, return all expense types
                var expenseTypes = await _accountingApiClient.ExpenseType_Get(CompanyId);
                var result = expenseTypes.Select(e => new
                {
                    id = e.Id,
                    primaryName = e.PrimaryName,
                    secondaryName = e.SecondaryName
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expense types by request {RequestNo}", requestNo);
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetTypeOfExpenseByExpenseType(int expenseTypeId)
        {
            try
            {
                var expenseTypes = await _accountingApiClient.ExpenseType_Get(CompanyId);
                var selectedExpenseType = expenseTypes.FirstOrDefault(x => x.Id == expenseTypeId);

                if (selectedExpenseType == null)
                {
                    return Json(new List<object>());
                }

                // Return the related type of expense
                var result = new List<object>
        {
            new
            {
                id = selectedExpenseType.FK_TypeOfExpenseId,
                primaryName = selectedExpenseType.oLKP_TypeOfExpense?.FirstOrDefault()?.PrimaryName ?? "General Expense",
                secondaryName = selectedExpenseType.oLKP_TypeOfExpense?.FirstOrDefault()?.SecondaryName ?? "مصروف عام"
            }
        };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting type of expense by expense type {ExpenseTypeId}", expenseTypeId);
                return Json(new List<object>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> CreatePettyCashSeller([FromBody] CreatePettyCashSellerDTO model)
        {
            try
            {
                // Validate seller info first
                var isValid = await _accountingApiClient.PettyCashSeller_IsValidInfo(
                    0, model.PrimaryName, model.SecondaryName, model.TaxNumber);

                if (!isValid)
                {
                    var errror_msg = _common["SellerinformationAlreadyExists"];
                    return Json(new { success = false, message = errror_msg });
                }

                var pettyCashSeller = new PettyCashSeller
                {
                    PrimaryName = model.PrimaryName,
                    SecondaryName = model.SecondaryName,
                    TaxNumber = model.TaxNumber,
                    CompanyId = CompanyId,
                    CreatedBy = UserId,
                    ModifiedBy = UserId
                };

                var sellerId = await _accountingApiClient.PettyCashSeller_Insert(pettyCashSeller);

                if (sellerId > 0)
                {
                    return Json(new
                    {
                        success = true,
                        sellerId = sellerId,
                        sellerName = model.PrimaryName
                    });
                }

                var msg = _common["FailedCreateSeller"];
                return Json(new { success = false, message = msg });
            }
            catch (Exception ex)
            {
                var msg = _common["FailedCreateSeller"];
                _logger.LogError(ex, "Error creating petty cash seller");
                return Json(new { success = false, message = msg });
            }
        }

        [HttpPost]
        public async Task<IActionResult> TransferMoveInPettyCash(
            [FromForm] PettyCashVehicleMovementDTO model,
            [FromForm] List<Models.WipServiceFixDto> Services,
            [FromForm] IFormFile PettyCash_Files)
        {
            var resultJson = new TempData();

            try
            {
                // Get the original movement to access ExitMeter and VatRate
                var originalMovement = await _apiClient.GetVehicleMovementByIdAsync(model.MovementId.Value);

                // Calculate amounts from PartsCost and LaborCost
                var totalInvoice = model.PettyCash_PartsCost + model.PettyCash_LaborCost;
                var isNotTaxable = model.PettyCash_NotTaxable;
                var vatRate = isNotTaxable ? 0 : (model.PettyCash_VatRate);
                var vatAmount = totalInvoice * (vatRate / 100);
                var netAmount = totalInvoice + vatAmount;

                // Find the matching TaxClassificationId based on VatRate
                int? taxClassificationId = null;
                if (isNotTaxable)
                {
                    try
                    {
                        var taxClassifications = await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang);
                        var zeroTaxClassification = taxClassifications.FirstOrDefault(tc => tc.TaxRate == 0);
                        taxClassificationId = zeroTaxClassification?.TaxClassificationNo;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error finding zero-rate tax classification");
                    }
                }
                else if (vatRate > 0)
                {
                    try
                    {
                        var taxClassifications = await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang);
                        var matchingTaxClassification = taxClassifications.FirstOrDefault(tc => tc.TaxRate == vatRate);
                        taxClassificationId = matchingTaxClassification?.TaxClassificationNo;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error finding tax classification for VatRate {VatRate}", vatRate);
                    }
                }

                // Validate the petty cash request balance
                var availableBalance = await _accountingApiClient.PettyCashExpenses_UserBalanceByRequest(model.PettyCash_RequestNo);
                if (netAmount > availableBalance)
                {
                    resultJson.IsSuccess = false;
                    resultJson.Message = "Net amount exceeds available balance";
                    return Json(resultJson);
                }

                // Validate invoice number uniqueness
                var isValidInvoice = await _accountingApiClient.PettyCashExpenses_IsValidInvoiceNo(
                    0, model.PettyCash_SellerId, model.PettyCash_InvoiceNo);

                if (!isValidInvoice)
                {
                    resultJson.IsSuccess = false;
                    resultJson.Message = "Invoice number already exists for this seller";
                    return Json(resultJson);
                }

                var vehicleMovement = originalMovement;
                var workshopDetails = await _apiClient.GetWorkshopByIdAsync(vehicleMovement.MoveInWorkshopId ?? 0);

                vehicleMovement.GregorianMovementEndDate = model.PettyCash_MovementDate;
                vehicleMovement.ReceivedTime = model.PettyCash_ReceivedTime;
                vehicleMovement.ReceivedMeter = model.PettyCash_ReceivedMeter;
                vehicleMovement.FuelLevelId = model.PettyCash_FuelLevelId;
                vehicleMovement.ReceivedDriverId = model.PettyCash_DriverName;
                vehicleMovement.TotalWorkOrder = netAmount; // Use calculated net amount
                vehicleMovement.Vat = vatAmount; // Use calculated VAT amount
                vehicleMovement.PartsCost = model.PettyCash_PartsCost;
                vehicleMovement.LaborCost = model.PettyCash_LaborCost;
                vehicleMovement.VatRate = vatRate; // Use VatRate from original movement
                vehicleMovement.InvoceNo = model.PettyCash_InvoiceNo;
                vehicleMovement.CompanyId = CompanyId;
                vehicleMovement.CreatedBy = UserId;
                vehicleMovement.MovementIN = true;
                vehicleMovement.MovementInId = null;
                vehicleMovement.MovementOut = null;
                vehicleMovement.MovementOutId = model.MovementId;
                vehicleMovement.WorkshopId = BranchId;
                vehicleMovement.Status = 1;
                vehicleMovement.IsExternal = true;


                // Check vehicle movement status
                var vehicleMovementStatus = await _apiClient.CheckVehicleMovementStatusAsync(vehicleMovement.VehicleID.Value);
                if (vehicleMovement.GregorianMovementDate.Value.Date.Add(vehicleMovement.ReceivedTime.Value) < vehicleMovementStatus.lastmovemnetDate)
                {
                    resultJson.IsSuccess = false;
                    resultJson.Message = "Cannot make In before last movement in " + vehicleMovementStatus.lastmovemnetDate;
                    return Json(resultJson);
                }
                vehicleMovement.IsPettyCash = true;
                // Insert vehicle movement
                var movements = await _apiClient.InsertVehicleMovementAsync(vehicleMovement);

                if (Services != null && Services.Any())
                {
                    await _apiClient.UpdateWIPServicesExternalAndFixStatus(Services);
                }

                // Handle file upload to accounting module directory
                string filePath = null, fileName = null;
                if (PettyCash_Files != null)
                {
                    var validationResult = _fileValidationService.CheckFileTypeAndSize(PettyCash_Files);
                    if (validationResult.IsSuccess)
                    {
                        if (PettyCash_Files.FileName != "blob")
                        {
                            string guid = Guid.NewGuid().ToString();
                            var accountingDirectoryPath = _configuration["FileUpload:DirectoryAccountingPath"];
                            var path1 = Path.Combine(accountingDirectoryPath, "TransFiles", guid);

                            if (!Directory.Exists(path1))
                            {
                                Directory.CreateDirectory(path1);
                            }

                            var filename = DateTime.Now.Ticks.ToString();
                            var extension = Path.GetExtension(PettyCash_Files.FileName);
                            var fullFileName = filename + extension;

                            var fullPath = Path.Combine(path1, fullFileName);

                            // Save the file to accounting module directory
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await PettyCash_Files.CopyToAsync(stream);
                            }

                            filePath = guid; // Store the GUID as the path reference
                            fileName = fullFileName;
                        }
                    }
                    else
                    {
                        resultJson.IsSuccess = false;
                        resultJson.Message = validationResult.Message;
                        return Json(resultJson);
                    }
                }

                var VehicleDetails = await _vehicleApiClient.GetVehicleDetails((int)model.VehicleID, lang);

                var accountDefinition = await _apiClient.GetAccountDefinitionGetAsync(CompanyId);
                // Create petty cash expense with calculated values
                var pettyCashExpense = new PettyCashExpenses
                {
                    FK_RequestNo = model.PettyCash_RequestNo,
                    FK_TypeOfExpense = 1, // Direct
                    FK_ExpenseType = accountDefinition.PettyCashExpenseTypeId,
                    FK_EmployeeId = UserId,
                    InvoiceNumber = model.PettyCash_InvoiceNo,
                    InvoiceDate = model.PettyCash_InvoiceDate,
                    FK_CurrencyId = model.PettyCash_CurrencyId,
                    FK_SellerId = model.PettyCash_SellerId,
                    NetAmount = netAmount, // Use calculated net amount (Total Invoice + VAT)
                    Tax = vatAmount, // Use calculated VAT amount
                    TotalAmount = totalInvoice, // Use calculated total invoice (Parts + Labor)
                    Description = model.PettyCash_Description,
                    FK_VehicleId = VehicleDetails.FixedAsset_DimensionsId ?? 0,
                    LastKM = (int)originalMovement.ExitMeter, // Use ExitMeter from original movement
                    KM = (int)model.PettyCash_ReceivedMeter, // Use ReceivedMeter as current KM
                    CreatedBy = UserId,
                    FileName = fileName,
                    FilePath = filePath,
                    CompanyId = CompanyId,
                    BranchId = BranchId,
                    TaxClassificationId = taxClassificationId ?? 0 // Use matching TaxClassificationId or 0 if not found

                };

                var expenseId = await _accountingApiClient.PettyCashExpenses_Insert(pettyCashExpense);

                MovementInvoice invoice = new MovementInvoice();
                if (!string.IsNullOrEmpty(vehicleMovement.InvoceNo) && vehicleMovement.TotalWorkOrder != null && vehicleMovement.TotalWorkOrder > 0)
                {
                    invoice.MovementId = vehicleMovement.MovementId.Value;
                    invoice.MasterId = vehicleMovement.MasterId.Value;
                    invoice.ExternalWorkshopId = Convert.ToInt32(vehicleMovement.MoveOutWorkshopId);
                    invoice.InvoiceNo = vehicleMovement.InvoceNo;
                    invoice.TotalInvoice = Convert.ToDecimal(vehicleMovement.TotalWorkOrder);
                    invoice.WorkOrderId = Convert.ToInt32(vehicleMovement.WorkOrderId);
                    invoice.DeductibleAmount = vehicleMovement.DeductibleAmount ?? 0m;
                    invoice.ConsumptionValueOfSpareParts = vehicleMovement.ConsumptionValueOfSpareParts ?? 0m;
                    invoice.Vat = vehicleMovement.Vat ?? 0;
                    invoice.PartsCost = vehicleMovement.PartsCost ?? 0;
                    invoice.LaborCost = vehicleMovement.LaborCost ?? 0;
                    invoice.Invoice_Date = DateTime.Now;

                    await _apiClient.WorkshopInvoiceInsertAsync(invoice);
                }

                resultJson.IsSuccess = true;
                resultJson.Type = "success";
                resultJson.Message = $"Petty cash expense created successfully. Expense ID: {expenseId}";
                return Json(resultJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating petty cash expense");
                resultJson.IsSuccess = false;
                resultJson.Type = "error";
                resultJson.Message = "An error occurred while processing the petty cash expense";
                return Json(resultJson);
            }
        }

        #endregion
        //=====================================================================================================
        #region Price WF

        [HttpPost]
        public async Task<IActionResult> Approve([FromBody] ApproveDto dto)
        {
            try
            {
                var state = await _erpApiClient.GetWorkflowStateByMasterIdAndCompanyId(dto.MasterId, CompanyId);
                if (state == null)
                    return Json(new { success = false, message = "State not found" });

                var groupIds = (GroupId?.ToString() ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (!groupIds.Contains(state.NextGroupId))
                    return Json(new { success = false, message = "No permission" });

                var responseApprove = await _erpApiClient.ApproveWorkflowInstance(
                    dto.MasterId, CompanyId, UserId, dto.ActionId, dto.Reason
                );

                if (!responseApprove.IsScusses)
                    return Json(new { success = false, message = "Approve failed" });

                state = await _erpApiClient.GetWorkflowStateByMasterIdAndCompanyId(dto.MasterId, CompanyId);
                if (state == null)
                    return Json(new { success = false, message = "State not found after approve" });

                var isFinished = state.IsFinished;

                if (isFinished)
                {
                    await _apiClient.WipPriceWorkflow_Finish(new FinishWipPriceWorkflowRequest
                    {
                        WipItemId = dto.Id,
                        MasterId = dto.MasterId,
                        Status = 2,
                        Reason = dto.Reason,
                        UserId = UserId
                    });
                }
                else
                {
                    if ((state.UsersContactInformation == null || state.UsersContactInformation.Count == 0) && state.NextGroupId > 0)
                    {
                        var nextUsers = await _erpApiClient.GetUsersByGroupId(state.NextGroupId);

                        state.UsersContactInformation = nextUsers?
                            .Where(u => u.IsActive)
                            .Select(u => new UserContactInformation
                            {
                                Id = u.UserID,
                                Email = u.Email,
                                PhoneNo = u.PhoneNo
                            })
                            .ToList() ?? new List<UserContactInformation>();
                    }

                    await _workflowEmailService.SendAsync(new WorkflowEmailRequest
                    {
                        MasterId = dto.MasterId,
                        CompanyId = CompanyId,
                        WipId = dto.WIPId,
                        WipItemId = dto.WipItemId,
                        KeyId = dto.KeyId,
                        Action = 1,
                        Lang = lang,
                        CreatedBy = UserId
                    }, state);
                }

                return Json(new
                {
                    success = true,
                    isFinished = isFinished,
                    priceWorkflowStatus = isFinished ? 2 : 1,
                    priceWorkflowStatusText = isFinished
                        ? (lang == "en" ? "Approved" : "تم الاعتماد")
                        : (lang == "en" ? "Pending" : "قيد الانتظار")
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Reject([FromBody] RejectDto dto)
        {
            try
            {
                var state = await _erpApiClient.GetWorkflowStateByMasterIdAndCompanyId(dto.MasterId, CompanyId);
                if (state == null)
                    return Json(new { success = false, message = "State not found" });

                var groupIds = (GroupId?.ToString() ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                if (!groupIds.Contains(state.NextGroupId))
                    return Json(new { success = false, message = "No permission" });

                var responseReject = await _erpApiClient.RejectWorkflowInstance(
                    dto.MasterId, CompanyId, UserId, dto.Reason
                );

                if (!responseReject.IsScusses)
                    return Json(new { success = false, message = "Reject failed" });

                await _apiClient.WipPriceWorkflow_Finish(new FinishWipPriceWorkflowRequest
                {
                    WipItemId = dto.Id,
                    MasterId = dto.MasterId,
                    Status = 3,
                    Reason = dto.Reason,
                    UserId = UserId
                });

                await _apiClient.UpdatePartStatusForSingleItem(new UpdateSinglePartStatusDTO
                {
                    WIPId = dto.WIPId,
                    Id = dto.Id,
                    StatusId = 37
                });

                return Json(new
                {
                    success = true,
                    isRejected = true,
                    priceWorkflowStatus = 3,
                    priceWorkflowStatusText = lang == "en" ? "Rejected" : "مرفوض"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetHistory(Guid MasterId)
        {
            List<WorkflowHistory> data = new List<WorkflowHistory>();

            try
            {
                data = await _erpApiClient.GetHistory(MasterId, CompanyId, lang);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(data);

            }
        }

        #endregion
        //=====================================================================================================
        #region Reservation item
        public async Task<JsonResult> UndoReservationUrl(int PartReserveId, int WIPId, int Id)
        {
            var response = await _inventoryApiClient.SetStatus(new InventoryTransactionHeaderSetStatusDTO
            {
                HeaderId = PartReserveId,
                NewStatusId = 3,
                ModifiedBy = UserId
            });

            if(response)
            {
                await _apiClient.UpdateIssueIdToWIP(new UpdateIssueIdDTO
                {
                    IssueId = 0,
                    WIPId = WIPId,
                    Id = Id
                });

                await _apiClient.UpdatePartStatusForSingleItem(new UpdateSinglePartStatusDTO
                {
                    WIPId = WIPId,
                    Id = Id,
                    StatusId = 35
                });

                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
        #endregion
    }
}
