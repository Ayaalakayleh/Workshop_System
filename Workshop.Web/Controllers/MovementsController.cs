using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class MovementsController : BaseController
    {
        private readonly WorkshopApiClient _workshopapiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _eRPApiClient;

        private readonly IFileService _fileService;
        public readonly string lang;


        public MovementsController(
            WorkshopApiClient workshopapiClient,
            VehicleApiClient vehicleApiClient,
            ERPApiClient ERPAPI,
            IFileService fileService,
            IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _workshopapiClient = workshopapiClient;
            _vehicleApiClient = vehicleApiClient;
            _eRPApiClient = ERPAPI;
            _fileService = fileService;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.Movements.View)]
        public async Task<IActionResult> Index()
        {

            WorkshopMovementFilter filter = new WorkshopMovementFilter();
            filter.WorkshopId = BranchId;
            List<VehicleNams> vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            VehicleMovement movement = new VehicleMovement();
            movement.ColMovements = await _workshopapiClient.GetAllDWorkshopVehicleMovementDDL(filter);
            
            movement.vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            List<VehicleNams> ExternalVehicles = new List<VehicleNams>();
            ExternalVehicles = await _vehicleApiClient.GetExteralVehicleName(lang);

            foreach (var m in movement.ColMovements)
            {
                if (m.IsExternal == true)
                {
                    var match = ExternalVehicles.Find(p => p.id == m.VehicleID);
                    m.VehicleName = match?.VehicleName ?? "Unknown Vehicle";
                }
                else
                {
                    var match = movement.vehicleNams.Find(p => p.id == m.VehicleID);
                    m.VehicleName = match?.VehicleName ?? "Unknown Vehicle";
                }
            }

            var allVeh = movement.ColMovements;
            ViewBag.VehicleNams = (allVeh ?? new List<VehicleMovement>())
                .Select(s => new SelectListItem
                {
                    Value = s.VehicleID.ToString(),
                    Text = s.VehicleName
                })
                .ToList();

            var chassisList = await _vehicleApiClient.GetChassiDDL(CompanyId, 1); // To be modified for real vehicleType
            ViewBag.Chassis = new SelectList(chassisList, "Id", "ChassisNo");

            //ToDo: Caching
            //if (cache.Get(string.Format(CacheKeys.VehiclesDDL, language)) != null)
            //{
            //    filter.vehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL, language));
            //}
            //else
            //{
            //    filter.vehicleNams = VehicleApi.GetVehiclesDDL(language, CompanyId);
            //    cache.Set(string.Format(CacheKeys.VehiclesDDL, language), filter.vehicleNams, DateTimeOffset.Now.AddDays(10));
            //}

            return View(filter);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.Movements.View)]
        public async Task<IActionResult> MovementList([FromBody] WorkshopMovementFilter filter)
        {

            try
            {

                filter ??= new WorkshopMovementFilter();

                VehicleMovement movement = new VehicleMovement();
                movement.ColMovements = new List<VehicleMovement>();

                filter.WorkshopId = BranchId;
                movement.ColMovements = await _workshopapiClient.GetAllDWorkshopVehicleMovementAsync(filter);
                movement.ColBranches = await _eRPApiClient.GetActiveBranchesByCompanyId(CompanyId);

                List<VehicleNams> ExternalVehicles = new List<VehicleNams>();

                movement.vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);

                //ToDo: Caching
                //if (cache.Get(string.Format(CacheKeys.VehiclesDDL, language)) != null)
                //{
                //    movement.vehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL, language));
                //}
                //else
                //{
                //    movement.vehicleNams = VehicleApi.GetVehiclesDDL(language, CompanyId);
                //    cache.Set(string.Format(CacheKeys.VehiclesDDL, language), movement.vehicleNams, DateTimeOffset.Now.AddDays(10));
                //}


                ExternalVehicles = await _vehicleApiClient.GetExteralVehicleName(lang);

                //ToDo: Caching
                //if (cache.Get(string.Format(CacheKeys.ExternalVehiclesDDL)) != null)
                //{
                //    ExternalVehicles = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.ExternalVehiclesDDL));
                //}
                //else
                //{
                //    ExternalVehicles = VehicleApi.GetExteralVehicleName(language);
                //    cache.Set(string.Format(CacheKeys.ExternalVehiclesDDL), ExternalVehicles, DateTimeOffset.Now.AddDays(5));
                //}

                foreach (var m in movement.ColMovements)
                {
                    if (m.IsExternal == true)
                    {
                        var match = ExternalVehicles.Find(p => p.id == m.VehicleID);
                        m.VehicleName = match?.VehicleName ?? "Unknown Vehicle";
                    }
                    else
                    {
                        var match = movement.vehicleNams.Find(p => p.id == m.VehicleID);
                        m.VehicleName = match?.VehicleName ?? "Unknown Vehicle";
                    }
                }

                return PartialView("MovementList", movement);

            }
            catch (Exception ex)
            {

                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpGet]
        [CustomAuthorize(Permissions.Movements.View)]
        public async Task<IActionResult> VehicleMovementFind(int movementId)
        {

            VehicleMovement movement = new VehicleMovement();
            movement = await _workshopapiClient.GetVehicleMovementByIdAsync(movementId);
            movement.LastVehicleMovementDocuments = new List<VehicleMovementDocument>();
            movement.ColMaintenanceCard = new List<MaintenanceCardDTO>();

            //ToDo: Mantainance Cards
            movement.ColMaintenanceCard = await _workshopapiClient.GetDMaintenanceCardsByMovementIdAsync(movementId);
            if (movement.ColMaintenanceCard.Count == 0)
            {
                movement.ColMaintenanceCard = await _workshopapiClient.GetDMaintenanceCardsByMasterIdAsync(movement.MasterId.Value);

            }

            WorkOrderFilterDTO workOrderFilter = new WorkOrderFilterDTO();
            workOrderFilter.VehicleID = movement.VehicleID;
            workOrderFilter.CompanyId = CompanyId;
            //workOrderFilter.BranchId = BranchId;
            workOrderFilter.language = lang;
            //workOrderFilter.IsExternal = movement.IsExternal;
            movement.WorkOrders = await _workshopapiClient.GetMWorkOrdersAsync(workOrderFilter);

            movement.VehicleMovementDocuments = new List<VehicleMovementDocument>();
            movement.VehicleMovementDocuments = await _workshopapiClient.GetMovementDocumentsAsync(movement.MovementId.Value);
            movement.MovementInvoice = await _workshopapiClient.GetWorkshopInvoiceByMovementId(movementId);

            if (movement.ColMaintenanceCard.Count == 0)
            {
                movement.ColMaintenanceCard.Add(new MaintenanceCardDTO());
                movement.AddService = true;
                if (movement.WorkOrders.Count > 0)
                {
                    var lastWorkOrder = movement.WorkOrders.FirstOrDefault();
                    movement.WorkOrders = new List<MWorkOrderDTO>
                    {
                        lastWorkOrder
                    };
                }
            }

            movement.LastMovementDetails = await _workshopapiClient.GetLastVehicleMovementByVehicleIdAsync(movement.VehicleID.Value);

            var vChecklists = await _workshopapiClient.GetVehicleChecklistByMovementId(movementId);
            var vLookupChecklist = await _workshopapiClient.GetVehicleChecklistLookup();
            var tChecklist = await _workshopapiClient.GetTyresChecklistByMovementId(movementId);
            var tLookupChecklist = await _workshopapiClient.GetTyreChecklistLookup();
            foreach (var item in vChecklists)
            {
                item.LookupPrimaryDescription = vLookupChecklist?.Where(i => i.Id == item.LookupId)?.Select(i=>i.PrimaryDescription)?.FirstOrDefault();
                item.LookupSecondaryDescription = vLookupChecklist?.Where(i => i.Id == item.LookupId)?.Select(i => i.SecondaryDescription)?.FirstOrDefault();
            }
            foreach(var item in tChecklist)
            {
                item.LookupPrimaryDescription = tLookupChecklist?.Where(i => i.Id == item.LookupId)?.Select(i => i.PrimaryDescription)?.FirstOrDefault();
                item.LookupSecondaryDescription = tLookupChecklist?.Where(i => i.Id == item.LookupId)?.Select(i => i.SecondaryDescription)?.FirstOrDefault();
            }
            if(vChecklists == null || vChecklists.Count() == 0)
            {
                var vChecklistsTemp = new List<VehicleChecklist>();  
                foreach (var item in vLookupChecklist ?? Enumerable.Empty<VehicleChecklistLookup>())
                {
                    vChecklistsTemp.Add(new VehicleChecklist
                    {
                        LookupPrimaryDescription = item.PrimaryDescription,
                        LookupSecondaryDescription = item.SecondaryDescription,
                        Pass = false
                    });
                }
                vChecklists = vChecklistsTemp;
            }
            if(tChecklist == null || tChecklist.Count() == 0)
            {
                var tChecklistTemp = new List<TyreChecklist>();
                foreach (var item in tLookupChecklist ?? Enumerable.Empty<TyreChecklistLookup>())
                {
                    tChecklistTemp.Add(new TyreChecklist
                    {
                        LookupPrimaryDescription = item.PrimaryDescription,
                        LookupSecondaryDescription = item.SecondaryDescription
                    });
                }
                tChecklist = tChecklistTemp;
            }
            movement.VehicleCkecklist = vChecklists?.ToList();
            movement.TyreCkecklist = tChecklist?.ToList();

            var recalls = await _workshopapiClient.GetAllRecallsDDLAsync();

            if (movement.IsExternal ?? false)
            {
                var vehicle = await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(movement.VehicleID ?? 0);
                if (vehicle != null && vehicle.ChassisNo != null)
                {
                    var vRecall = (await _workshopapiClient.GetActiveRecallsByChassis(vehicle.ChassisNo));
                    movement.HasRecall = vRecall != null && vRecall?.Id != null && vRecall?.Id != 0;
                }
                //    movement.HasRecall = recalls
                //        .SelectMany(r => r.Vehicles ?? Enumerable.Empty<VehicleRecallDTO>())
                //        .Any(v => v.Chassis == vehicle.ChassisNo || (v.MakeID == vehicle.ManufacturerId && v.ModelID == vehicle.VehicleModelId));
            }
            else
            {
                var vehicle = await _vehicleApiClient.VehicleDefinitions_Find(movement.VehicleID ?? 0);
                if (vehicle != null && vehicle.ChassisNo != null)
                {
                    var vRecall = (await _workshopapiClient.GetActiveRecallsByChassis(vehicle.ChassisNo));
                    movement.HasRecall = vRecall != null && vRecall?.Id != null && vRecall?.Id != 0;
                }
                //    movement.HasRecall = recalls
                //        .SelectMany(r => r.Vehicles ?? Enumerable.Empty<VehicleRecallDTO>())
                //        .Any(v => v.Chassis == vehicle.ChassisNo || (v.MakeID == vehicle.ManufacturerId && v.ModelID == vehicle.VehicleModelId));
                //}
            }



                return View(movement);
        }

        [HttpGet]
        public async Task<IActionResult> GetStrike(int movementId)
        {

            string strike = await _workshopapiClient.GetVehicleMovementStrikeAsync(movementId);
            return Json(strike);
        }

        public async Task<JsonResult> GetVehicleDefentionById(int id, string lang)
        {
            try
            {
                var vehicle = await _vehicleApiClient.VehicleDefinitions_Find(id);
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Vehicle = vehicle,
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        
    }
}
