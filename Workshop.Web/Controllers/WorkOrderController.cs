using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Infrastructure;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class WorkOrderController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _erpApiClient;
        //private readonly IConfiguration _configuration;
        //private readonly IWebHostEnvironment _env;

        public readonly string lang;
        public WorkOrderController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, IConfiguration configuration, IWebHostEnvironment env, VehicleApiClient vehicleApiClient) : base(configuration, env)
        {
            _apiClient = apiClient;
            _vehicleApiClient = vehicleApiClient;
            //_configuration = configuration;
            //_env = env;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.WorkOrder.View)]
        public async Task<IActionResult> Index()
        {
            WorkOrderModel workOrderModel = new WorkOrderModel();
            workOrderModel.WorkOrderDTOs = new List<MWorkOrderDTO>();

            workOrderModel.WorkOrderForm = new MWorkOrderDTO();

            List<SelectListItem> WorkOrderTypes = new List<SelectListItem>();
            WorkOrderTypes.Add(new SelectListItem { Text = lang == "en" ? "Maintenance" : "Maintenance", Value = 2.ToString() });

            ViewBag.WorkOrderType = WorkOrderTypes;

            var vehicles = (await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId));
            var vehiclesExternal = (await _vehicleApiClient.GetExteralVehicleName(lang));
            var SubStatus = (await _vehicleApiClient.GetAllSubStatus(CompanyId, lang)).Select(r => new SelectListItem { Text = lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();
            ViewBag.SubStatus = SubStatus;
            WorkOrderFilterDTO workOrderFilter = new WorkOrderFilterDTO();
            workOrderFilter.CompanyId = CompanyId;
            workOrderFilter.page = 1;
            workOrderModel.WorkOrderFilter = workOrderFilter;

            workOrderModel.WorkOrderDTOs = await LoadWorkOrdersAsync(workOrderFilter, vehicles, vehiclesExternal);

            var vehicleList = new List<SelectListItem>();
            foreach (var item in vehicles)
            {
                vehicleList.Add(new SelectListItem(lang == "en" ? item.VehicleName : item.VehicleName, item.id.ToString()));
            }
            foreach(var item in vehiclesExternal)
            {
                vehicleList.Add(new SelectListItem(lang == "en" ? item.VehicleName : item.VehicleName, item.id.ToString()));
            }

            ViewBag.Vehicles = vehicleList;
            VehicleTypeId vehicleTypeId;
            ViewBag.VehicleType = Enum.GetValues(typeof(VehicleTypeId))
     .Cast<VehicleTypeId>()
     .Select(v => new SelectListItem
     {
         Text = v.ToString(),        // "Internal", "External", etc.
         Value = ((int)v).ToString() // "1", "2", "3"
     })
     .ToList();

            ViewBag.FK_AgreementId = new List<SelectListItem>();
            ViewBag.FkVehicleMovementId = new List<SelectListItem>();

            ViewBag.Agreements = SubStatus;
            return View(workOrderModel);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.WorkOrder.View)]
        public async Task<IActionResult> Index(WorkOrderModel workOrderModel)
        {

            workOrderModel ??= new WorkOrderModel();
            workOrderModel.WorkOrderDTOs = new List<MWorkOrderDTO>();
            workOrderModel.WorkOrderForm = new MWorkOrderDTO();

            List<SelectListItem> WorkOrderTypes = new List<SelectListItem>();
            WorkOrderTypes.Add(new SelectListItem { Text = lang == "en" ? "Maintenance" : "Maintenance", Value = 2.ToString() });
            ViewBag.WorkOrderType = WorkOrderTypes;

            var vehicles =  (await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId));
            var vehiclesExternal = (await _vehicleApiClient.GetExteralVehicleName(lang));

            var vehicleList = new List<SelectListItem>();
            foreach (var item in vehicles)
            {
                vehicleList.Add(new SelectListItem(lang == "en" ? item.VehicleName : item.VehicleName, item.id.ToString()));
            }
            foreach (var item in vehiclesExternal)
            {
                vehicleList.Add(new SelectListItem(lang == "en" ? item.VehicleName : item.VehicleName, item.id.ToString()));
            }
            ViewBag.Vehicles = vehicleList;
            var SubStatus = (await _vehicleApiClient.GetAllSubStatus(CompanyId, lang)).Select(r => new SelectListItem { Text = lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();
            ViewBag.SubStatus = SubStatus;

            var workOrderFilter = workOrderModel.WorkOrderFilter ?? new WorkOrderFilterDTO();
            workOrderFilter.CompanyId = CompanyId;
            workOrderFilter.page ??= 1;

            workOrderModel.WorkOrderDTOs = await LoadWorkOrdersAsync(workOrderFilter, vehicles, vehiclesExternal);
            workOrderModel.WorkOrderFilter = workOrderFilter;

            VehicleTypeId vehicleTypeId;
            ViewBag.VehicleType = Enum.GetValues(typeof(VehicleTypeId))
     .Cast<VehicleTypeId>()
     .Select(v => new SelectListItem
     {
         Text = v.ToString(),        // "Internal", "External", etc.
         Value = ((int)v).ToString() // "1", "2", "3"
     })
     .ToList();

            ViewBag.FK_AgreementId = new List<SelectListItem>();
            ViewBag.FkVehicleMovementId = new List<SelectListItem>();

            ViewBag.Agreements = SubStatus;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_WorkOrderList", workOrderModel);
            }

            return View(workOrderModel);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.WorkOrder.View)]
        public async Task<IActionResult> WorkOrderList([FromBody] WorkOrderFilterDTO workOrderFilter)
        {
            workOrderFilter ??= new WorkOrderFilterDTO();
            workOrderFilter.CompanyId = CompanyId;
            workOrderFilter.page ??= 1;

            var vehicles = (await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId));
            var vehiclesExternal = (await _vehicleApiClient.GetExteralVehicleName(lang));

            var workOrderModel = new WorkOrderModel
            {
                WorkOrderDTOs = await LoadWorkOrdersAsync(workOrderFilter, vehicles, vehiclesExternal),
                WorkOrderFilter = workOrderFilter,
                WorkOrderForm = new MWorkOrderDTO()
            };

            return PartialView("_WorkOrderList", workOrderModel);
        }

        [CustomAuthorize(Permissions.WorkOrder.Edit)]
        public async Task<IActionResult> Edit(int? id)
        {
            WorkOrderModel workOrderModel = new WorkOrderModel();
            workOrderModel.WorkOrderDTOs = new List<MWorkOrderDTO>();
            workOrderModel.WorkOrderFilter = new WorkOrderFilterDTO();
            workOrderModel.WorkOrderForm = new MWorkOrderDTO();
            if (id != null && id > 0)
            {

                var workOrder = await _apiClient.GetMWorkOrderByID(id ?? 0);
                workOrderModel.WorkOrderForm = workOrder ?? new MWorkOrderDTO();
                return View(workOrderModel);
            }
            else
            {

                workOrderModel.WorkOrderForm = new MWorkOrderDTO();
                var vehicles = await _vehicleApiClient.GetAllVehicleModel(0, lang);
                var vehicleList = new List<SelectListItem>();
                foreach (var item in vehicles)
                {
                    vehicleList.Add(new SelectListItem(lang == "en" ? item.VehicleModelPrimaryName : item.VehicleModelSecondaryName, item.Id.ToString()));
                }
                ViewBag.Vehicles = vehicleList;
                return View("Edit", workOrderModel);
            }

        }

        [HttpGet]
        public async Task<JsonResult> M_Agreement_GetAgreementId([FromQuery] int VehicleDefinitionId)
        {
            var agreement = new List<Agreement>();
            if (VehicleDefinitionId > 0)
            {
                agreement = await _vehicleApiClient.GetLastAgreement(lang, 6193);
            }
            return Json(agreement);
        }
        [HttpGet]
        public async Task<JsonResult> M_Agreement_GetMovementId([FromQuery] int VehicleDefinitionId)
        {
            var movement = new VehicleMovement();
            if (VehicleDefinitionId > 0)
            {
                movement = await _vehicleApiClient.GetVehicleMovement(6193);
            }
            return Json(movement);
        }

        public async Task<IActionResult> VehicleList(int VehicleTypeId)
        {
            var filter = new VehicleAdvancedFilter();
            var colVehicleDefinitions = new List<SelectListItem>();
            string language = "en";
            filter.VehicleTypeId = VehicleTypeId;
            filter.CompanyId = CompanyId;


            if (filter.VehicleTypeId == 1) // internal
            {
                colVehicleDefinitions = (await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId))
                    .Select(item => new SelectListItem
                    {
                        Text = lang == "en" ? item.VehicleName : item.VehicleName,
                        Value = item.id.ToString()
                    })
                    .ToList();
            }
            else if (filter.VehicleTypeId == 2) // external
            {
                colVehicleDefinitions = (await _vehicleApiClient.GetExteralVehicleName(lang))
                .Select(item => new SelectListItem
                {
                    Text = lang == "en" ? item.VehicleName : item.VehicleName,
                    Value = item.id.ToString()
                })
                .ToList();
            }

            else
            {
                colVehicleDefinitions = new List<SelectListItem>();
            }

            return Json(colVehicleDefinitions);
        }

        private async Task<List<MWorkOrderDTO>> LoadWorkOrdersAsync(WorkOrderFilterDTO workOrderFilter, List<VehicleNams> vehicles, List<VehicleNams> vehiclesExternal)
        {
            var workOrders = await _apiClient.GetMWorkOrdersAsync(workOrderFilter) ?? new List<MWorkOrderDTO>();
            var workOrderStatusNames = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(12, CompanyId);

            MapWorkOrderDisplayData(workOrders, vehicles, vehiclesExternal, workOrderStatusNames);
            return workOrders;
        }

        private void MapWorkOrderDisplayData(IEnumerable<MWorkOrderDTO> workOrders, IEnumerable<VehicleNams> vehicles, IEnumerable<VehicleNams> vehiclesExternal, IEnumerable<LookupDetailsDTO> workOrderStatusNames)
        {
            if (workOrders == null)
            {
                return;
            }

            var statusLookup = workOrderStatusNames ?? Enumerable.Empty<LookupDetailsDTO>();

            foreach (var item in workOrders)
            {
                if (item.VehicleType == 1)
                {
                    item.VehicleName = vehicles
                        .FirstOrDefault(v => v.id == item.VehicleId) is var v && v != null
                            ? (lang == "en" ? v.VehicleName : v.VehicleName)
                            : string.Empty;
                }
                else if (item.VehicleType == 2)
                {
                    item.VehicleName = vehiclesExternal
                        .FirstOrDefault(v => v.id == item.VehicleId) is var v && v != null
                            ? (lang == "en" ? v.VehicleName : v.VehicleName)
                            : string.Empty;
                }

                item.WorkOrderStatusName = statusLookup
                    .FirstOrDefault(t => t.Id == item.WorkOrderStatus) is var t && t != null
                        ? (lang == "en" ? t.PrimaryName : t.SecondaryName)
                        : string.Empty;
            }
        }


        public async Task<MWorkOrderDTO> loadWorkOrderData(int? id)
        {
            WorkOrderModel workOrderModel = new WorkOrderModel();
            workOrderModel.WorkOrderDTOs = new List<MWorkOrderDTO>();
            workOrderModel.WorkOrderFilter = new WorkOrderFilterDTO();
            workOrderModel.WorkOrderForm = new MWorkOrderDTO();
            if (id != null && id > 0)
            {

                var workOrder = await _apiClient.GetMWorkOrderByID(id ?? 0);
                workOrderModel.WorkOrderForm = workOrder ?? new MWorkOrderDTO();

                if (string.IsNullOrWhiteSpace(workOrder.ImagesFilePath) ||
                    string.IsNullOrWhiteSpace(workOrder.FileName))
                {
                    workOrder.ImagesFilePath = null;
                    return workOrder;
                }

                var baseFolder = _configuration["FileUpload:DirectoryPath"] ?? "Uploads";

                workOrder.ImagesFilePath =
                    $"/{baseFolder}/{workOrder.ImagesFilePath}/{workOrder.FileName}"
                        .Replace("\\", "/")
                        .Replace("//", "/");

                return workOrder;


            }
            else
            {

                workOrderModel.WorkOrderForm = new MWorkOrderDTO();
                var vehicles = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
                var AgreementsIds = (await _apiClient.GetMWorkOrdersAsync(new WorkOrderFilterDTO())).Select(work => new SelectListItem { Text = work.FkAgreementId.ToString(), Value = work.FkAgreementId.ToString() });
                var VehicleMovementId = (await _apiClient.GetMWorkOrdersAsync(new WorkOrderFilterDTO())).Select(work => new SelectListItem { Text = work.FkVehicleMovementId.ToString(), Value = work.FkVehicleMovementId.ToString() });

                ViewBag.FK_AgreementId = AgreementsIds;
                ViewBag.FkVehicleMovementId = VehicleMovementId;
                var vehicleList = new List<SelectListItem>();
                foreach (var item in vehicles)
                {
                    vehicleList.Add(new SelectListItem(item.VehicleName + item.PlateNumber, item.id.ToString()));
                }
                ViewBag.Vehicles = vehicleList;
                return new MWorkOrderDTO();
            }

        }

        [HttpPost]
        [CustomAuthorize(Permissions.WorkOrder.Edit)]
        public async Task<IActionResult> Edit(WorkOrderModel workOrderModel, IFormFile formFile)
        {

            workOrderModel.WorkOrderDTOs = new List<MWorkOrderDTO>();
            workOrderModel.WorkOrderFilter = new WorkOrderFilterDTO();
            workOrderModel.WorkOrderForm.BranchId = BranchId;
            if (formFile != null && formFile.Length > 0)
            {
                var relativePath = _configuration["FileUpload:DirectoryPath"]?.TrimStart('/', '\\');
                var guid = Guid.NewGuid().ToString();

                // Combine with wwwroot to get the absolute path
                var folderPath = Path.Combine(base._env.WebRootPath, relativePath, "WorkOrderAttachments", guid);

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var filename = $"{DateTime.Now.Ticks}{Path.GetExtension(formFile.FileName)}";
                var fullPath = Path.Combine(folderPath, filename);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await formFile.CopyToAsync(stream);
                }

                workOrderModel.WorkOrderForm.ImagesFilePath = Path.Combine("WorkOrderAttachments", guid).Replace("\\", "/");
                workOrderModel.WorkOrderForm.FileName = filename;
            }

            if (workOrderModel.WorkOrderForm != null && workOrderModel.WorkOrderForm.Id > 0)
            {
                MWorkOrderDTO workOrderDTO = new MWorkOrderDTO();
                workOrderDTO = workOrderModel.WorkOrderForm;

                //workOrderModel.WorkOrderForm = workOrder ?? new MWorkOrderDTO();
                workOrderModel.WorkOrderForm.CompanyId = CompanyId;

                //workOrderModel.WorkOrderForm.
                var workOrder = await _apiClient.UpdateMWorkOrderAsync(workOrderDTO);
                return RedirectToAction("Index", workOrderModel);
            }
            else
            {

                MWorkOrderDTO mWorkOrderDTO = new MWorkOrderDTO();
                mWorkOrderDTO = workOrderModel.WorkOrderForm;
                workOrderModel.WorkOrderForm.CompanyId = CompanyId;
                var workOrderStatusDef = (await _apiClient.GetAllLookupDetailsByHeaderIdAsync(12, CompanyId)).Select(l => l.Id).FirstOrDefault();
                workOrderModel.WorkOrderForm.WorkOrderStatus = workOrderStatusDef;
                var workOrder = await _apiClient.InsertMWorkOrderAsync(mWorkOrderDTO);
                return RedirectToAction("Index", workOrderModel);
            }

        }

        [HttpGet]
        public async Task<IActionResult> VehicleWorkOrderList(int id, int? type = null, int isExternal = 1)
        {

            List<MWorkOrderDTO> workOders = new List<MWorkOrderDTO>();
            WorkOrderFilterDTO filter = new WorkOrderFilterDTO();
            filter.VehicleID = id;
            filter.CompanyId = CompanyId;
            filter.language = lang;
            filter.Type = type;
            filter.IsExternal = isExternal == 1 ? false : true;
            workOders = await _apiClient.GetMWorkOrdersAsync(filter);

            return Json(workOders, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // This preserves the original case
                DictionaryKeyPolicy = null
            });

        }

    }
}
