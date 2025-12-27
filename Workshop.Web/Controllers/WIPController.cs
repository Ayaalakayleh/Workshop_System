
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Domain.Enum;
using Workshop.Infrastructure;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
        private readonly IFileService _fileService;
        private readonly IFileValidationService _fileValidationService;
        private readonly ILogger<WIPController> _logger;

        public readonly string lang;
        public WIPController(
            AccountingApiClient accountingApiClient,
            VehicleApiClient vehicleApiClient,
            WorkshopApiClient apiClient,
            InventoryApiClient inventoryApiClient,
            ERPApiClient erpApiClient,
            IConfiguration configuration,
            IWebHostEnvironment env,
            IFileService fileService,
            IFileValidationService fileValidationService,
            ILogger<WIPController> logger
            , IMemoryCache cache) : base(cache, configuration, env)
        {
            _accountingApiClient = accountingApiClient;
            _vehicleApiClient = vehicleApiClient;
            _apiClient = apiClient;
            _inventoryApiClient = inventoryApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            _fileService = fileService;
            _fileValidationService = fileValidationService;
            _erpApiClient = erpApiClient;
            _logger = logger;
        }


        public IActionResult CreateJobCard(int movementId)
        {
            return RedirectToAction("Edit", new { id = (int?)null, movementId = movementId });
        }

        [CustomAuthorize(Permissions.WIP.View)]
        public async Task<IActionResult> Index([FromForm] FilterWIPDTO? oFilterWIPDTO)
        {

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


        public async Task<IActionResult> Edit(int? id, int? movementId = 0)
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

                if (dto.MovementId > 0)
                {
                    movement = await _apiClient.GetVehicleMovementByIdAsync(dto.MovementId);
                    if (movement != null)
                    {
                        var user = await _erpApiClient.GetUserInfoById((int)movement.CreatedBy);

                        var first = user?.FirstName?.Trim();
                        var last = user?.LastName?.Trim();

                        string userFullName = string.Join(" ", new[] { first, last }.Where(x => !string.IsNullOrWhiteSpace(x)));
                        ViewBag.CreatingOperator = userFullName;
                        ViewBag.DueInDate = movement.CreatedAt?.ToString("yyyy-MM-dd");

                        ViewBag.DueOutDate = movement.MovementOut == true ? movement.CreatedAt?.ToString("yyyy-MM-dd") : null;
                        ViewBag.ReceivedMeter = movement.ReceivedMeter;
                    }
                }


                if (id.HasValue && id.Value > 0)
                {
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

                    if (dto.MovementId > 0)
                    {
                        movement = await _apiClient.GetVehicleMovementByIdAsync(dto.MovementId);
                        if (movement != null)
                        {
                            var user = await _erpApiClient.GetUserInfoById((int)movement.CreatedBy);

                            var first = user?.FirstName?.Trim();
                            var last = user?.LastName?.Trim();

                            string userFullName = string.Join(" ", new[] { first, last }.Where(x => !string.IsNullOrWhiteSpace(x)));
                            ViewBag.CreatingOperator = userFullName;
                            ViewBag.DueInDate = movement.CreatedAt?.ToString("yyyy-MM-dd");

                        }
                        var LastMovement = await _apiClient.GetLastVehicleMovementByVehicleIdAsync(dto.VehicleId);
                        ViewBag.DueOutDate = LastMovement.MovementOut == true ? LastMovement.CreatedAt?.ToString("yyyy-MM-dd") : null;
                    }

                    // Get vehicle documents - handle nulls
                    try
                    {
                        var vehicleDoc = await _vehicleApiClient.Documants_GetByVehicleIdAndSystemTypeId(dto.VehicleId, 8);
                        ViewBag.RegDate = vehicleDoc?.strExpiryDate ?? "N/A";

                        var MOTDoc = await _vehicleApiClient.Documants_GetByVehicleIdAndSystemTypeId(dto.VehicleId, 3);
                        ViewBag.MOTDate = MOTDoc?.strExpiryDate ?? "N/A";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching vehicle documents for vehicle {VehicleId}", dto.VehicleId);
                        ViewBag.RegDate = "N/A";
                        ViewBag.MOTDate = "N/A";
                    }

                    // Get internal matches
                    try
                    {
                        ViewBag.InternalMatches = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(9, CompanyId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching internal matches");
                        ViewBag.InternalMatches = new List<LookupDetailsDTO>();
                    }

                    // Get account details
                    dto.AccountDetails = new AccountDTO();
                    try
                    {
                        dto.AccountDetails = await _apiClient.WIP_GetAccountById(dto.Id) ?? new AccountDTO();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching account details for WIP {WIPId}", dto.Id);
                        dto.AccountDetails = new AccountDTO();
                    }

                    // Get sales type based on account type
                    if (dto.AccountDetails != null)
                    {
                        try
                        {
                            ViewBag.SalesType = await GetSalesTypeListAsync((int)dto.AccountDetails.AccountType, CompanyId, lang);
                            var reverseType = dto.AccountDetails.AccountType == AccountTypeEnum.Internal
                                ? AccountTypeEnum.External
                                : AccountTypeEnum.Internal;

                            ViewBag.PartialSalesType = await GetSalesTypeListAsync((int)reverseType, CompanyId, lang);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error fetching sales type");
                            ViewBag.SalesType = new List<SelectListItem>();
                        }
                    }
                    else
                    {
                        ViewBag.SalesType = new List<SelectListItem>();
                    }

                    // Get services
                    try
                    {
                        ViewBag.Services = await _apiClient.WIP_GetServicesById(id.Value, lang) ?? new List<CreateWIPServiceDTO>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching services for WIP {WIPId}", id.Value);
                        ViewBag.Services = new List<CreateWIPServiceDTO>();
                    }

                    // Get account details

                    dto.InvoiceDetailsList = await _apiClient.WIPInvoiceGetById(dto.Id, null);


                    // Get items
                    try
                    {
                        var items = await _apiClient.WIP_GetItemsById(id.Value, lang) ?? new List<CreateItemDTO>();
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
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Error mapping item {ItemId} for WIP {WIPId}", item.ItemId, id.Value);
                            }
                        }
                        ViewBag.Items = items;
                        ViewBag.AllowActions = items != null && items.Any();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching items for WIP {WIPId}", id.Value);
                        ViewBag.Items = new List<BaseItemDTO>();
                    }
                }

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

                        }
                        else
                        {
                            var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(dto.VehicleId)) ?? new CreateVehicleDefinitionsModel();
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
                            Text = e.ToString()
                        }).ToList();
                }
                catch
                {
                    ViewBag.AccountTypes = new List<SelectListItem>();
                }

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
                try
                {
                    var units = await _inventoryApiClient.GetAllUnitDDL();
                    ViewBag.Units = units?.Select(t => new SelectListItem
                    {
                        Text = lang == "en" ? t.primaryName : t.secondaryName,
                        Value = t.Id.ToString()
                    }).ToList() ?? new List<SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching units");
                    ViewBag.Units = new List<SelectListItem>();
                }

                // Get warehouses
                try
                {
                    var Warehouses = await _inventoryApiClient.GetAllWarehousesDDL(null, 1);
                    ViewBag.Warehouses = Warehouses?.Select(t => new SelectListItem
                    {
                        Text = lang == "en" ? t.PrimaryName : t.SecondaryName,
                        Value = t.Id.ToString()
                    }).ToList() ?? new List<SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching warehouses");
                    ViewBag.Warehouses = new List<SelectListItem>();
                }

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
                if (dto.Status == (int)WIPStatusEnum.C)
                {
                    ViewBag.IsWaitingInvoiced = true;
                }

                int? selectedCustomerId = null;

                // Get customer agreements for vehicle
                try
                {
                    if (dto.VehicleId > 0)
                    {

                        var activeAgreement = await _vehicleApiClient.GetActiveAgreementId(dto.VehicleId);

                        selectedCustomerId = activeAgreement?.CustomerId;
                        var status = (activeAgreement?.AgreementId > 0) ? "Open" : "No Agreement";


                        ViewBag.AgreementStatus = status;
                        if (activeAgreement.AgreementId != null && activeAgreement.AgreementId > 0)
                            ViewBag.AgreementEndDate = activeAgreement.GregorianReturnDate.ToString("yyyy-MM-dd");
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
                    if (dto.Status == (int)WIPStatusEnum.G)
                    {
                        await GetReturnParts(dto.Id);
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
        public async Task<IActionResult> GetAccountNumber(int Id)
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
            }).ToList();
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
        public async Task<JsonResult> GetAllItems(int fK_GroupId, int fK_CategoryId, int fK_SubCategoryId)
        {
            var items = await _inventoryApiClient.GetItemsWithStockAndLocation(fK_GroupId, fK_CategoryId, fK_SubCategoryId);

            var allCategories = await _inventoryApiClient.GetAllCategoriesAsync();
            var allUnits = await _inventoryApiClient.GetAllUnitDDL();

            var myWarehouseIds = new HashSet<int>(await GetOurWarehouse());

            var catById = allCategories.ToDictionary(c => c.Id);
            var unitById = allUnits.ToDictionary(u => u.Id);

            object Map(ItemDTO item)
            {
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
                    costPrice = item.PurchasePrice,
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

                    unitPrimaryName = unit?.primaryName,
                    unitSecondaryName = unit?.secondaryName
                };
            }

            var ours = items
                .Where(i => myWarehouseIds.Contains(i.WarehouseId))
                .Select(Map)
                .ToList();

            var others = items
                .Where(i => !myWarehouseIds.Contains(i.WarehouseId))
                .Select(Map)
                .ToList();

            return Json(new { ours, others });
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
            //List<ItemDTO> items = await _inventoryApiClient.GetAllItemsAsync(0, 0, 0);

            //List<CategoryDTO> allCategories = await _inventoryApiClient.GetAllCategoriesAsync();
            //List<UnitDTO> allUnits = await _inventoryApiClient.GetAllUnitDDL();
            var _item = await _inventoryApiClient.GetItemByIdAsync(itemId);
            //var result = _item.Select(item => new
            //{
            //    Code=item.Code,
            //    Name= lang == "en" ? item.primaryName : item.secondaryName

            //}).ToList();
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

            return matches.Select(sc => new SelectListItem
            {
                Value = sc.Id.ToString(),
                Text = lang == "en" ? sc.PrimaryName : sc.SecondaryName
            }).ToList();
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
            model = await _apiClient.WIP_SChedule_Get(RTSId, WIPId, KeyId);
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

        public async Task<IActionResult> GetWIPServiceHistory(int VehicleId)
        {
            try
            {
                var result = await _apiClient.GetWIPServiceHistory(VehicleId);
                var labourHistory = await _apiClient.WIPServiceHistoryDetails_GetLabours();
                var partsHistory = await _apiClient.WIPServiceHistoryDetails_GetParts();
                foreach (var item in result)
                {
                    item.HistoryLabours = labourHistory.Where(record => record.FK_WIPId == item.WIPId);
                    item.HistoryParts = partsHistory.Where(record => record.FK_WIPId == item.WIPId);
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
                var labourHistory = await _apiClient.WIPServiceHistoryDetails_GetLabours();
                var partsHistory = await _apiClient.WIPServiceHistoryDetails_GetParts();
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
                movement.MoveOutWorkshopId = BranchId;
                movement.MovementOut = true;
                movement.WorkshopId = BranchId;
                movement.Status = 4;
                movement.MasterId = move.MasterId;
                movement.WorkOrderId = move.WorkOrderId;
                movement.LastVehicleStatus = move.LastVehicleStatus;
                movement.IsExternal = true;
                movement.VehicleID = move.VehicleID;

                var movements = await _apiClient.InsertVehicleMovementAsync(movement);
                await _apiClient.TransferMaintenanceMovement(movements.MovementId.Value, (int)movement.MoveInWorkshopId, movement.MasterId.Value, movement.Reason);

                var workshop = await _apiClient.GetWorkshopByIdAsync((int)movement.MoveInWorkshopId);
                int updated = await _apiClient.UpdateWIPServicesIsExternalAsync(SelectedServicesIds);
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
                var externalVehicles = await _vehicleApiClient.GetExteralVehicleName(lang);
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
            movement.WIPServices = (await _apiClient.GetWIPServicesByMovementIdAsync(movement.MovementInId.Value))?.Where(s => s.IsExternal).ToList();
            movement.WIPServices ??= (new List<CreateWIPServiceDTO>());

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
        [FromForm] string FixedServiceIds,
        [FromForm] IFormFile file)
        {
            var resultJson = new TempData();

            resultJson.Notification = new List<Notification>();
            //var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            //int? accountId = null;
            //var accountTable = new AccountTable();
            //var ColTaxClassification = await _accountingApiClient.GetTaxClassificationListByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            //var zeroTaxClassification = ColTaxClassification.Where(a => a.TaxRate == 0).FirstOrDefault();

            try
            {
                var ExternalWorkshop = await _apiClient.GetWorkshopByIdAsync((int)movement.MoveOutWorkshopId);
                //if (!string.IsNullOrEmpty(movement.InvoceNo) && movement.TotalWorkOrder != null && movement.TotalWorkOrder > 0)
                //{
                //    bool IsValid = await _accountingApiClient.AccountSalesMaster_IsValidSupplierInvoiceNo((int)ExternalWorkshop.SupplierId, movement.InvoceNo);
                //    if (!IsValid)
                //    {
                //        resultJson.IsSuccess = false;
                //        resultJson.Type = "error";
                //        resultJson.Message = "InvoiceNo" + " " + movement.InvoceNo + " " + "Already Exist";
                //        return Json(resultJson);
                //    }
                //}

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
                //Check
                //Movement.DamageId = Movement.ColMaintenanceCard[0].DamageId;
                var movements = await _apiClient.InsertVehicleMovementAsync(movement);
                if (!string.IsNullOrWhiteSpace(FixedServiceIds))
                {
                    await _apiClient.UpdateWIPServicesIsFixedAsync(FixedServiceIds);
                }

                MovementInvoice invoice = new MovementInvoice();
                if (!string.IsNullOrEmpty(movement.InvoceNo) && movement.TotalWorkOrder != null && movement.TotalWorkOrder > 0)
                {
                    invoice.MovementId = movements.MovementId.Value;
                    invoice.MasterId = movement.MasterId.Value;
                    invoice.ExternalWorkshopId = Convert.ToInt32(movement.MoveOutWorkshopId);
                    invoice.InvoiceNo = movement.InvoceNo;
                    invoice.TotalInvoice = Convert.ToDecimal(movement.TotalWorkOrder);
                    invoice.WorkOrderId = Convert.ToInt32(movement.WorkOrderId);
                    invoice.DeductibleAmount = movement.DeductibleAmount ?? 0m;
                    invoice.ConsumptionValueOfSpareParts = movement.ConsumptionValueOfSpareParts ?? 0m;
                    invoice.Vat = movement.Vat ?? 0;
                    invoice.PartsCost = movement.PartsCost.Value;
                    invoice.LaborCost = movement.LaborCost.Value;
                    invoice.Invoice_Date = DateTime.Now;

                    await _apiClient.WorkshopInvoiceInsertAsync(invoice);
                }





                //MovementInvoice invoice = new MovementInvoice();

                // List<HttpPostedFileBase> files = Request.Files["DamageReport"];
                //for (int i = 0; i < file.Length; i++)
                //{
                //    var validationResult = _fileValidationService.CheckFileTypeAndSize(file);

                //    //Logic should be implemented 
                //    //if (Request.Files.GetKey(i) == "ExternalWorkshopInvoice")
                //    if (validationResult.IsSuccess)
                //    {
                //        var (filePath, fileName) = await _fileService.SaveFileAsync(file, "ExternalWorkshopInvoice");

                //        invoice.FileName = fileName;
                //        invoice.FilePath = filePath;
                //        invoice.MovementId = movements.MovementId.Value;
                //        invoice.Invoice_Date = DateTime.Now;
                //        await _apiClient.DExternalWorkshopInvoiceInsertAsync(invoice);
                //    }
                //    else
                //    {
                //        await _apiClient.UpdateWorkOrderInvoicingStatusAsync((int)movement.WorkOrderId);
                //    }
                //}


                //if (!string.IsNullOrEmpty(movement.InvoceNo) && movement.TotalWorkOrder != null && movement.TotalWorkOrder > 0)
                //{
                //    invoice.MovementId = movements.MovementId.Value;
                //    invoice.MasterId = movement.MasterId.Value;
                //    invoice.ExternalWorkshopId = Convert.ToInt32(movement.MoveOutWorkshopId);
                //    invoice.InvoiceNo = movement.InvoceNo;
                //    invoice.TotalInvoice = Convert.ToDecimal(movement.TotalWorkOrder);
                //    invoice.WorkOrderId = Convert.ToInt32(movement.WorkOrderId);
                //    invoice.DeductibleAmount = movement.DeductibleAmount.Value;
                //    invoice.ConsumptionValueOfSpareParts = movement.ConsumptionValueOfSpareParts.Value;
                //    invoice.Vat = movement.Vat ?? 0;// من لفيو اذا مش تاكسبل ابعتها صفر 
                //    invoice.PartsCost = movement.PartsCost.Value;
                //    invoice.LaborCost = movement.LaborCost.Value;

                //    await _apiClient.WorkshopInvoiceInsertAsync(invoice);
                //    AccountSales oAccountSales = new AccountSales();
                //    oAccountSales.AccountSalesDetails = new List<AccountSalesDetails>();
                //    oAccountSales.AccountSalesMaster = new AccountSalesMaster();

                //    var oAccountSalesDetails = new AccountSalesDetails();
                //    var Supplier = await _accountingApiClient.Supplier_Find(ExternalWorkshop.SupplierId.Value);
                //    var VehicleDetails = new VehicleDefinitions();
                //    var items = new List<Item>();
                //    items = await _accountingApiClient.GetItemsByCategoryNo(-1, lang);
                //    var InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetById((int)movement.InvoiceTypeId);


                //    oAccountSales.AccountSalesMaster = new AccountSalesMaster()
                //    {

                //        Total = invoice.LaborCost + invoice.PartsCost,
                //        Net = invoice.TotalInvoice,
                //        Final = invoice.LaborCost + invoice.PartsCost,
                //        Tax = invoice.Vat,
                //        AccSalesTypeNo = 8,
                //        AccSalesDate = DateTime.Now,
                //        InvoiceType = 2,
                //        TypeSalesPurchasesID = (int)movement.InvoiceTypeId,
                //        Notes = "Maintenance",
                //        SupplierInvoiceNo = invoice.InvoiceNo,
                //        CustomerId = (int)Supplier.Id,
                //        Customer_DimensionsId = Supplier.Customer_DimensionsId,
                //        Vendor_DimensionsId = Supplier.Vendor_DimensionsId,
                //        LOB_DimensionsId = Supplier.LOB_DimensionsId,
                //        Regions_DimensionsId = Supplier.Regions_DimensionsId,
                //        Locations_DimensionsId = Supplier.Locations_DimensionsId,
                //        Item_DimensionsId = Supplier.Item_DimensionsId,
                //        Worker_DimensionsId = Supplier.Worker_DimensionsId,
                //        FixedAsset_DimensionsId = Supplier.FixedAsset_DimensionsId,
                //        Department_DimensionsId = Supplier.Department_DimensionsId,
                //        Contract_CC_DimensionsId = Supplier.Contract_CC_DimensionsId,
                //        City_DimensionsId = Supplier.City_DimensionsId,
                //        D1_DimensionsId = Supplier.D1_DimensionsId,
                //        D2_DimensionsId = Supplier.D2_DimensionsId,
                //        D3_DimensionsId = Supplier.D3_DimensionsId,
                //        D4_DimensionsId = Supplier.D4_DimensionsId,
                //        CustomerAccountNo = AccountList.Where(x => x.ID == Supplier.AccountNoPayableId).FirstOrDefault().AccountNo,
                //    };

                //    VehicleDetails = await _vehicleApiClient.GetVehicleDetails(movement.VehicleID.Value, lang);

                //    if (invoice.PartsCost > 0)
                //    {
                //        accountId = items.Where(a => a.ItemNumber == -1).FirstOrDefault()?.ItemSalesAccountId;
                //        accountId = accountId == null ? InvoiceType.AccountId : accountId;
                //        accountTable = new AccountTable();
                //        accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                //        oAccountSalesDetails = new AccountSalesDetails()
                //        {
                //            ItemNumber = items.Where(a => a.ItemNumber == -1).FirstOrDefault().ItemId,
                //            UnitId = items.Where(a => a.ItemNumber == -1).FirstOrDefault().UnitId,
                //            Discount = 0,
                //            Description = "Maintenance Parts " + "( " + VehicleDetails.PlateNumber + " ) " + " صيانة قطع غيار ",
                //            Quantity = 1,
                //            UnitQuantity = 1,
                //            Price = invoice.PartsCost,
                //            Total = invoice.PartsCost,
                //            TaxValue = movement.Vat == 0 ? 0 : invoice.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100),///اذا فوق صفر ياخدها صفر
                //            TaxClassificationId = movement.Vat == 0 ? zeroTaxClassification.TaxClassificationNo : items.Where(a => a.ItemNumber == -1).FirstOrDefault().TaxClassificationNo,
                //            Final = invoice.PartsCost + (invoice.PartsCost * (items.Where(a => a.ItemNumber == -1).FirstOrDefault().taxRate / 100)),
                //            CostsCentersNo = VehicleDetails.CostCenter,
                //            Reference = VehicleDetails.PlateNumber,
                //            Customer_DimensionsId = accountTable.IsCustomer_Dimensions ? VehicleDetails.Customer_DimensionsId : null,
                //            Vendor_DimensionsId = accountTable.IsVendor_Dimensions ? VehicleDetails.Vendor_DimensionsId : null,
                //            LOB_DimensionsId = accountTable.IsLOB_Dimensions ? VehicleDetails.LOB_DimensionsId : null,
                //            Regions_DimensionsId = accountTable.IsRegions_Dimensions ? VehicleDetails.Regions_DimensionsId : null,
                //            Locations_DimensionsId = accountTable.IsLocations_Dimensions ? VehicleDetails.Locations_DimensionsId : null,
                //            Item_DimensionsId = accountTable.IsItem_Dimensions ? VehicleDetails.Item_DimensionsId : null,
                //            Worker_DimensionsId = accountTable.IsWorker_Dimensions ? VehicleDetails.Worker_DimensionsId : null,
                //            FixedAsset_DimensionsId = accountTable.IsFixedAsset_Dimensions ? VehicleDetails.FixedAsset_DimensionsId : null,
                //            Department_DimensionsId = accountTable.IsDepartment_Dimensions ? VehicleDetails.Department_DimensionsId : null,
                //            Contract_CC_DimensionsId = accountTable.IsContract_CC_Dimensions ? VehicleDetails.Contract_CC_DimensionsId : null,
                //            City_DimensionsId = accountTable.IsCity_Dimensions ? VehicleDetails.City_DimensionsId : null,
                //            D1_DimensionsId = accountTable.IsD1_Dimensions ? VehicleDetails.D1_DimensionsId : null,
                //            D2_DimensionsId = accountTable.IsD2_Dimensions ? VehicleDetails.D2_DimensionsId : null,
                //            D3_DimensionsId = accountTable.IsD3_Dimensions ? VehicleDetails.D3_DimensionsId : null,
                //            D4_DimensionsId = accountTable.IsD4_Dimensions ? VehicleDetails.D4_DimensionsId : null,
                //        };
                //        oAccountSales.AccountSalesDetails.Add(oAccountSalesDetails);

                //    }
                //    if (invoice.LaborCost > 0)
                //    {
                //        accountId = items.Where(a => a.ItemNumber == -2).FirstOrDefault()?.ItemSalesAccountId;
                //        accountId = accountId == null ? InvoiceType.AccountId : accountId;
                //        accountTable = new AccountTable();
                //        accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                //        oAccountSalesDetails = new AccountSalesDetails()
                //        {
                //            ItemNumber = items.Where(a => a.ItemNumber == -2).FirstOrDefault().ItemId,
                //            UnitId = items.Where(a => a.ItemNumber == -2).FirstOrDefault().UnitId,
                //            Discount = 0,
                //            Description = "Maintenance Labor " + "( " + VehicleDetails.PlateNumber + " ) " + " صيانة عمالة",
                //            Quantity = 1,
                //            UnitQuantity = 1,
                //            Price = invoice.LaborCost,
                //            Total = invoice.LaborCost,
                //            TaxValue = movement.Vat == 0 ? 0 : invoice.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100), //??
                //            TaxClassificationId = movement.Vat == 0 ? zeroTaxClassification.TaxClassificationNo : items.Where(a => a.ItemNumber == -2).FirstOrDefault().TaxClassificationNo,
                //            Final = invoice.LaborCost + (invoice.LaborCost * (items.Where(a => a.ItemNumber == -2).FirstOrDefault().taxRate / 100)),
                //            CostsCentersNo = VehicleDetails.CostCenter,
                //            Reference = VehicleDetails.PlateNumber,
                //            Customer_DimensionsId = accountTable.IsCustomer_Dimensions ? VehicleDetails.Customer_DimensionsId : null,
                //            Vendor_DimensionsId = accountTable.IsVendor_Dimensions ? VehicleDetails.Vendor_DimensionsId : null,
                //            LOB_DimensionsId = accountTable.IsLOB_Dimensions ? VehicleDetails.LOB_DimensionsId : null,
                //            Regions_DimensionsId = accountTable.IsRegions_Dimensions ? VehicleDetails.Regions_DimensionsId : null,
                //            Locations_DimensionsId = accountTable.IsLocations_Dimensions ? VehicleDetails.Locations_DimensionsId : null,
                //            Item_DimensionsId = accountTable.IsItem_Dimensions ? VehicleDetails.Item_DimensionsId : null,
                //            Worker_DimensionsId = accountTable.IsWorker_Dimensions ? VehicleDetails.Worker_DimensionsId : null,
                //            FixedAsset_DimensionsId = accountTable.IsFixedAsset_Dimensions ? VehicleDetails.FixedAsset_DimensionsId : null,
                //            Department_DimensionsId = accountTable.IsDepartment_Dimensions ? VehicleDetails.Department_DimensionsId : null,
                //            Contract_CC_DimensionsId = accountTable.IsContract_CC_Dimensions ? VehicleDetails.Contract_CC_DimensionsId : null,
                //            City_DimensionsId = accountTable.IsCity_Dimensions ? VehicleDetails.City_DimensionsId : null,
                //            D1_DimensionsId = accountTable.IsD1_Dimensions ? VehicleDetails.D1_DimensionsId : null,
                //            D2_DimensionsId = accountTable.IsD2_Dimensions ? VehicleDetails.D2_DimensionsId : null,
                //            D3_DimensionsId = accountTable.IsD3_Dimensions ? VehicleDetails.D3_DimensionsId : null,
                //            D4_DimensionsId = accountTable.IsD4_Dimensions ? VehicleDetails.D4_DimensionsId : null,
                //        };
                //        oAccountSales.AccountSalesDetails.Add(oAccountSalesDetails);

                //    }

                //    oAccountSales.AccountSalesMaster.UserId = userID.ToString();
                //    //ToDo Important
                //    oAccountSales.AccountSalesMaster.CurrencyID = 1;//((CompanyInfo)Session["CompanyInfo"]).CurrencyIDH;
                //    oAccountSales.AccountSalesMaster.AccSalesBranch = BranchId;
                //    oAccountSales.AccountSalesMaster.PaymentTerms = Supplier.oLDBPaymentType > 0 ? Supplier.oLDBPaymentType : 0;
                //    oAccountSales.CompanyId = CompanyId;
                //    oAccountSales.BranchId = BranchId;
                //    oAccountSales.AccountSalesMaster.InventoryAccountId = InvoiceType.AccountId;
                //    //ToDo Important
                //    oAccountSales.CompanyType = 1; // ((CompanyInfo)Session["CompanyInfo"]).CompanyType;
                //    await _accountingApiClient.AccountSalesMaster_Insert(oAccountSales);
                //}

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
            }
        }

        [HttpPost]
        public async Task<JsonResult> CloseWIP(UpdateWIPDTO dto)
        {
            try
            {
                dto.ClosedBy = UserId;


                var isValid = await _apiClient.WIP_Validation(dto.Id); //1=valid
                var ExternalInvoice = new AccountSalesMaster();
                if (isValid == 1)
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
                                Total = dto.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.Internal).Sum(x => x.CostPrice * (decimal)x.Quantity),
                                Tax = 0,
                                Net = dto.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.Internal).Sum(x => x.CostPrice * (decimal)x.Quantity),
                                InvoiceType = (int)AccountTypeEnum.Internal,
                                AccountType = (int)AccountTypeEnum.Internal,
                                CreatedBy = UserId
                            };
                            await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);

                        }
                        if (dto.AccountDetails.AccountType == AccountTypeEnum.External || dto.AccountDetails.PartialAccountType == AccountTypeEnum.External)
                        {
                            ExternalInvoice = await SaveInvoice(dto);

                        }
                    }
                    if ((ExternalInvoice.ID > 0 || Internalinvoice.ID > 0))
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
                    return Json(new { success = true });
                }
                else
                {
                    switch (isValid)
                    {
                        case -100:
                            return Json(new { success = false, message = "USER_CONTEXT_MISSING" });
                        case -101:
                            return Json(new { success = false, message = "WIP_NOT_FOUND" });
                        case -102:
                            return Json(new { success = false, message = "ALREADY_CLOSED" });
                        case -103:
                            return Json(new { success = false, message = "SERVICE_NOT_COMPLETED" });
                        case -104:
                            return Json(new { success = false, message = "SERVICE_TIME_MISSING" });
                        case -105:
                            return Json(new { success = false, message = "PARTS_RETURN_PENDING" });
                        case -106:
                            return Json(new { success = false, message = "PARTIAL_INVOICE_INCOMPLETE" });
                        case -107:
                            return Json(new { success = false, message = "UPDATE_AFFECTED_UNEXPECTED_ROWS" });
                        case -999:
                        default:
                            return Json(new { success = false, message = "An unknown error occurred." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }


        public async Task<TransactionMaster> _SaveInvoice(UpdateWIPDTO oWIPDTO)
        {
            string InternalType = "";
            var AccountTable = _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId).Result;
            var account = await _apiClient.GetAccountDefinitionGetAsync(CompanyId);
            decimal? totalInternal = oWIPDTO.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.Internal).Sum(x => x.CostPrice * (decimal)x.Quantity);
            decimal? totalExternal = oWIPDTO.ItemsList.Where(x => x.AccountType == (int)AccountTypeEnum.External).Sum(x => x.CostPrice * (decimal)x.Quantity);
            var VehicleDetails = _vehicleApiClient.GetVehicleDetails(oWIPDTO.VehicleId, lang).Result;

            if (oWIPDTO.AccountDetails.AccountType == AccountTypeEnum.Internal || oWIPDTO.AccountDetails.PartialAccountType == AccountTypeEnum.Internal)
            {
                var InternalList = await _apiClient.GetLookupDetailByIdAsync(oWIPDTO.AccountDetails.SalesType == null ? (int)oWIPDTO.AccountDetails.PartialSalesType : (int)oWIPDTO.AccountDetails.SalesType, 9, CompanyId);

                InternalType = InternalList.Code;
            }
            var saveTransaction = await _accountingApiClient.SaveTransaction(VehicleDetails, AccountTable, account, CompanyId, BranchId, UserId, account.JournalId, totalInternal, totalExternal, DateTime.Now, "Close WIP No : " + oWIPDTO.Id, CurrencyId, InternalType);
            return saveTransaction;

        }

        //===========================================
        //
        //===========================================
        [HttpPost]
        public async Task<AccountSalesMaster> SaveInvoice(UpdateWIPDTO oWIPDTO)
        {
            var result = new TempData();

            var account = await _apiClient.GetAccountDefinitionGetAsync(CompanyId);
            var invoice = account.InvoiceTypeId; //to be invoice type only
            var AccountList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId, lang);
            var Customer = await _accountingApiClient.Customer_GetById((int)oWIPDTO.AccountDetails.CustomerId);
            int? accountId = null;
            var accountTable = new AccountTable();
            var oAccountSalesDetails = new AccountSalesDetails();
            AccountSales oAccountSales = new AccountSales();
            var VehicleDetails = _vehicleApiClient.GetVehicleDetails(oWIPDTO.VehicleId, lang).Result;
            var taxClass = _accountingApiClient.GetTaxClassificationById(oWIPDTO.AccountDetails.Vat ?? 1).Result;
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
                        accountId = items.Where(a => a.ItemNumber == -2).FirstOrDefault()?.ItemSalesAccountId;
                        //accountId = accountId == null ? InvoiceType.AccountId : accountId;
                        accountTable = new AccountTable();
                        accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                        oAccountSalesDetails = new AccountSalesDetails()
                        {
                            ItemNumber = items.Where(a => a.ItemNumber == -2).FirstOrDefault().ItemId,
                            UnitId = items.Where(a => a.ItemNumber == -2).FirstOrDefault().UnitId,
                            Discount = (int)item.Discount,
                            Description = item.LongDescription,
                            Quantity = 1,
                            UnitQuantity = 1,
                            Price = item.Total,
                            Total = item.Total - (int)item.Discount,
                            TaxValue = (item.Total - (int)item.Discount) * (taxClass.TaxRate / 100),
                            TaxClassificationId = taxClass.TaxClassificationNo,
                            Final = (item.Total - (int)item.Discount) + ((item.Total - (int)item.Discount) * taxClass.TaxRate / 100),
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

                    accountId = items.Where(a => a.ItemNumber == -1).FirstOrDefault()?.ItemSalesAccountId;
                    //accountId = accountId == null ? InvoiceType.AccountId : accountId;
                    accountTable = new AccountTable();
                    accountTable = AccountList.Where(x => x.ID == accountId).FirstOrDefault();
                    oAccountSalesDetails = new AccountSalesDetails()
                    {
                        ItemNumber = items.Where(a => a.ItemNumber == -1).FirstOrDefault().ItemId,
                        UnitId = items.Where(a => a.ItemNumber == -1).FirstOrDefault().UnitId,
                        Discount = (int)item.Discount,
                        Description = mapping.Name,
                        Quantity = (int)item.Quantity,
                        UnitQuantity = (int)item.Quantity,
                        Price = item.SalePrice,
                        Total = (item.SalePrice - (int)item.Discount) * (int)item.Quantity,
                        TaxValue = (item.SalePrice * (int)item.Quantity - (int)item.Discount) * (taxClass.TaxRate / 100),
                        TaxClassificationId = taxClass.TaxClassificationNo,
                        Final = (item.SalePrice * (int)item.Quantity - (int)item.Discount) + ((item.SalePrice * (int)item.Quantity - (int)item.Discount) * taxClass.TaxRate / 100),
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
                    Discount = oAccountSales.AccountSalesDetails.Sum(a => a.Discount),
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
            return Json(new { success = true, data = result });
        }

        #endregion


        public async Task<JsonResult> GetAvailableTechnicians([FromQuery] DateTime date, decimal duration)
        {
            var technicians = await _apiClient.GetAvailableTechniciansAsync(date, duration, BranchId);

            var data = technicians.Select(t => new
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

            bool isSuccess = result != null;
            return Json(new { success = isSuccess, data = result });

        }

        public async Task<string> CreateIssueVoucher([FromForm] CreateInventoryTransactionDTO model)
        {
            var rawDetails = HttpContext.Request.Form["Details"];
            var wipId = int.Parse(HttpContext.Request.Form["WIPId"]);
            // From base controller
            model.CompanyId = CompanyId;
            model.BranchId = BranchId;
            model.CreatedBy = UserId;

            if (!string.IsNullOrEmpty(rawDetails))
            {
                model.Details = JsonConvert.DeserializeObject<List<InventoryTransactionDetailsDTO>>(rawDetails);
            }
            int keyId = int.Parse(model.Details.First().KeyId);
            var result = string.Empty;
            var accountDefinitions = await _inventoryApiClient.GetInventoryAccountDefinitions();
            var AccountTable = _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId).Result;
            var warehouse = await _inventoryApiClient.GetWarehouseByIdAsync((int)model.FK_WarehouseId);
            var CreditAccount = AccountTable?.FirstOrDefault(a => a.ID == warehouse.FK_AccountId).AccountNo;
            var DebitAccount = AccountTable?.FirstOrDefault(a => a.ID == accountDefinitions.FK_WIPAccountId).AccountNo;
            var TranTypeNo = accountDefinitions.FK_JournalNameId;
            model.TransactionDate = DateTime.Now;
            // Save the transaction
            var accountingResponse = await _accountingApiClient.SaveIssueTransaction(
                TranTypeNo,
                (decimal)model.Details.Sum(x => x.Total),
                DebitAccount,
                CompanyId,
                BranchId,
                UserId,
                CreditAccount,
                model.TransactionDate,
                model.Description,
                CurrencyId,
                null
            );

            // If successful, add the ISSUE
            if (accountingResponse != null && accountingResponse.ID > 0)
            {
                model.FinancialTransactionNo = accountingResponse.TranNo;
                model.FinancialTransactionTypeNo = accountingResponse.TranTypeNo;
                model.Fk_FinancialTransactionMasterId = accountingResponse.ID;
                model.Fk_InvoiceType = accountDefinitions.FK_InvoiceTypeId;

                result = await _inventoryApiClient.GRNAdd(model); // step 2
            }

            var json = JsonDocument.Parse(result);
            long headerId = json.RootElement.GetProperty("newId").GetInt64();
            UpdateIssueIdDTO dto = new UpdateIssueIdDTO
            {
                IssueId = (int)headerId,
                WIPId = wipId,
                Id = keyId
            };

            var addIssueToWIP = await _apiClient.UpdateIssueIdToWIP(dto);

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
            var responseToClient = new
            {
                success = true,
                partsIssueId = headerId,
                wipId = wipId
            };

            return JsonConvert.SerializeObject(responseToClient);
            //return result;
        }


        public async Task<JsonResult> UndoIssueVoucher(int PartsIssueId, int WIPId)
        {
            var responseString = await _inventoryApiClient.GetAllGRNByIdHead(PartsIssueId);
            var response = JsonSerializer.Deserialize<InventoryTransactionByIdDTO>(
              responseString,
              new JsonSerializerOptions
              {
                  PropertyNameCaseInsensitive = true
              });

            var TransTypeNo = Convert.ToInt32(response.FinancialTransactionTypeNo);
            var MasterId = (int)response.Fk_FinancialTransactionMasterId;

            var rev = await _accountingApiClient.ReverseTransactionAsync(MasterId, TransTypeNo, CompanyId, BranchId, UserId);
            var deleteDTO = new DeleteInventoryTransactionDTO
            {
                ID = PartsIssueId,
                ModifiedBy = 1,
                FK_FinancialTransactionReverseId = rev.ID
            };

            var deleteResult = await _inventoryApiClient.InventoryTransactionDelete(deleteDTO);

            if (deleteResult == 0)
            {
                var isDelete = await _apiClient.WIP_DeleteItems(WIPId, PartsIssueId);
            }
            return Json(new { success = true, data = deleteResult });
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
                if (item.AccountType == 1)
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
                            Total = 0,
                            Tax = 0,
                            Discount = 0,
                            Net = 0,
                            InvoiceType = -3,
                            AccountType = (int)AccountTypeEnum.External,
                            CreatedBy = UserId,
                            OldTransactionMasterId = item.TransactionMasterId

                        };
                        await _apiClient.InsertWIPInvoice(wIPInvoiceDTO);
                    }
                }
            }

            bool isSuccess = Reverse.ID > 0;
            UpdateWIPStatusDTO updateWIPStatusDTO = new UpdateWIPStatusDTO()
            {
                WIPId = WIPId,
                StatusId = 2030,
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
        public async Task<ActionResult> PrintInternal(int WIPId, int TransactionMasterId)
        {
            var PrintInternalDTO = new PrintInternalDTO();
            var InvoiceDetailsList = await _apiClient.WIPInvoiceGetById(WIPId, TransactionMasterId);
            PrintInternalDTO.WipInvoiceDetail = await _apiClient.WipInvoiceByHeaderId(InvoiceDetailsList.FirstOrDefault().Id);
            //PrintInternalDTO.Service = await _apiClient.GetAllInternalLabourLineAsync(WIPId);
            //PrintInternalDTO.Items = await _apiClient.GetAllInternalPartsLineAsync(WIPId);
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
                return Json(new {success = false,error = ex.Message});
                
            }
        }

    }
}
