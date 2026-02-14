using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.DTOs.WorkshopMovement;
using Workshop.Infrastructure;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    public class MovementsHistoryController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly WorkshopApiClient _apiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _erpApiClient;
        public readonly string lang;

        public MovementsHistoryController(IConfiguration configuration, IWebHostEnvironment env,
            WorkshopApiClient apiClient, VehicleApiClient vehicleApiClient, AccountingApiClient accountingApiClient,
            ERPApiClient erpApiClient, IMemoryCache cache) : base(cache, configuration, env)
        {
            _configuration = configuration;
            _env = env;
            _apiClient = apiClient;
            _vehicleApiClient = vehicleApiClient;
            _accountingApiClient = accountingApiClient;
            _erpApiClient = erpApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                WorkshopMovementFilter ovehicleMovement = new WorkshopMovementFilter();
                ovehicleMovement.vehicleNams = new List<VehicleNams>();
                if (cache.Get(string.Format(CacheKeys.VehiclesDDL, lang)) != null)
                {
                    ovehicleMovement.vehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL, lang));
                }
                else
                {
                    ovehicleMovement.vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
                    cache.Set(string.Format(CacheKeys.VehiclesDDL, lang), ovehicleMovement.vehicleNams, DateTimeOffset.Now.AddDays(10));
                }

                return View(ovehicleMovement);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public async Task<ActionResult> MovementList( WorkshopMovementFilter vehicleMovementFilter)
        {
            try
            {
                vehicleMovementFilter ??= new WorkshopMovementFilter();
                VehicleMovement ovehicleMovement = new VehicleMovement();
                ovehicleMovement.ColMovements = new List<VehicleMovement>();
                vehicleMovementFilter.WorkshopId = BranchId;
                vehicleMovementFilter.page ??= 1;
                ovehicleMovement.ColMovements = await _apiClient.GetAllDWorkshopVehicleMovementAsync(vehicleMovementFilter);
                ovehicleMovement.ColBranches = await _erpApiClient.GetActiveBranchesByCompanyId(CompanyId);
                List<VehicleNams> ExternalVehicles = new List<VehicleNams>();

                if (cache.Get(string.Format(CacheKeys.VehiclesDDL, lang)) != null)
                {
                    ovehicleMovement.vehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL, lang));
                }
                else
                {
                    ovehicleMovement.vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
                    cache.Set(string.Format(CacheKeys.VehiclesDDL, lang), ovehicleMovement.vehicleNams, DateTimeOffset.Now.AddDays(10));
                }

                if (cache.Get(string.Format(CacheKeys.ExternalVehiclesDDL)) != null)
                {
                    ExternalVehicles = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.ExternalVehiclesDDL));
                }
                else
                {
                    ExternalVehicles = await _vehicleApiClient.GetExteralVehicleName(lang);
                    cache.Set(string.Format(CacheKeys.ExternalVehiclesDDL), ExternalVehicles, DateTimeOffset.Now.AddDays(5));
                }

                foreach (var movement in ovehicleMovement.ColMovements)
                {
                    if (movement.IsExternal!=null && movement.IsExternal == true)
                        movement.VehicleName = ExternalVehicles.Find(p => p.id == movement.VehicleID).VehicleName;
                    else
                        movement.VehicleName = ovehicleMovement.vehicleNams.Find(p => p.id == movement.VehicleID).VehicleName;
                }

                return PartialView("MovementList", ovehicleMovement);

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<ActionResult> VehicleMovementFind(int MovementId)
        {
            VehicleMovement ovehicleMovement = new VehicleMovement();
            ovehicleMovement = await _apiClient.GetVehicleMovementByIdAsync(MovementId);
            ovehicleMovement.LastVehicleMovementDocuments = new List<VehicleMovementDocument>();
            ovehicleMovement.ColMaintenanceCard = new List<MaintenanceCardDTO>();
            ovehicleMovement.ColMaintenanceCard = await _apiClient.GetDMaintenanceCardsByMovementIdAsync(MovementId);
           
            if (ovehicleMovement.ColMaintenanceCard.Count == 0 && ovehicleMovement.MasterId.HasValue)
            {
                ovehicleMovement.ColMaintenanceCard = await _apiClient.GetDMaintenanceCardsByMasterIdAsync(ovehicleMovement.MasterId.Value);
            }
            else if (ovehicleMovement.ColMaintenanceCard.Count == 0)
            {
                ovehicleMovement.ColMaintenanceCard = new List<MaintenanceCardDTO>(); 
            }

            WorkOrderFilterDTO damageFilter = new WorkOrderFilterDTO();
            damageFilter.VehicleID = ovehicleMovement.VehicleID;
            damageFilter.CompanyId = CompanyId;
            damageFilter.language = lang;
            damageFilter.IsExternal = ovehicleMovement.IsExternal;
            ovehicleMovement.WorkOrders = await _apiClient.GetMWorkOrdersAsync(damageFilter);
            if(ovehicleMovement.MovementId != null)
            {
                ovehicleMovement.VehicleMovementDocuments = await _apiClient.GetMovementDocumentsAsync((int)ovehicleMovement.MovementId);
            }
            else
            {
                ovehicleMovement.VehicleMovementDocuments = new List<VehicleMovementDocument>();
            }
            ovehicleMovement.MovementInvoice = await _apiClient.GetWorkshopInvoiceByMovementId(MovementId);

            if (ovehicleMovement.ColMaintenanceCard.Count == 0)
            {
                ovehicleMovement.ColMaintenanceCard.Add(new MaintenanceCardDTO());
                ovehicleMovement.AddService = true;
                if (ovehicleMovement.WorkOrders.Count > 0)
                {
                    var LastDamage = ovehicleMovement.WorkOrders.FirstOrDefault();
                    ovehicleMovement.WorkOrders = new List<MWorkOrderDTO>
                    {
                        LastDamage
                    };
                }
            }

            ovehicleMovement.LastMovementDetails = new VehicleMovement();
            ovehicleMovement.LastMovementDetails = await _apiClient.GetLastVehicleMovementByVehicleIdAsync((int)ovehicleMovement.VehicleID);
            return View(ovehicleMovement);
        }
    }
}
