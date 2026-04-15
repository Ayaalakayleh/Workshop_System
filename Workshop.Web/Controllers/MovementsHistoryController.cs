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


        [CustomAuthorize(Permissions.MovementsHistory.View)]
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

                var external = (await _vehicleApiClient.GetExteralVehicleName(lang)) ?? new List<VehicleNams>();
                ovehicleMovement.vehicleNams.AddRange(external);

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

                var ovehicleMovement = new VehicleMovement
                {
                    ColMovements = new List<VehicleMovement>()
                };

                vehicleMovementFilter.WorkshopId = BranchId;
                vehicleMovementFilter.page ??= 1;

                ovehicleMovement.ColMovements = await _apiClient.GetAllMovementsHistoryFilter(vehicleMovementFilter)
                                                ?? new List<VehicleMovement>();

                ovehicleMovement.ColBranches = await _erpApiClient.GetActiveBranchesByCompanyId(CompanyId);

                List<VehicleNams> externalVehicles = new List<VehicleNams>();

                if (cache.Get(string.Format(CacheKeys.VehiclesDDL, lang)) != null)
                {
                    ovehicleMovement.vehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL, lang));
                }
                else
                {
                    ovehicleMovement.vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId) ?? new List<VehicleNams>();
                    cache.Set(string.Format(CacheKeys.VehiclesDDL, lang), ovehicleMovement.vehicleNams, DateTimeOffset.Now.AddDays(10));
                }

                if (cache.Get(string.Format(CacheKeys.ExternalVehiclesDDL)) != null)
                {
                    externalVehicles = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.ExternalVehiclesDDL));
                }
                else
                {
                    externalVehicles = await _vehicleApiClient.GetExteralVehicleName(lang) ?? new List<VehicleNams>();
                    cache.Set(string.Format(CacheKeys.ExternalVehiclesDDL), externalVehicles, DateTimeOffset.Now.AddDays(5));
                }

                ovehicleMovement.vehicleNams ??= new List<VehicleNams>();
                externalVehicles ??= new List<VehicleNams>();

                foreach (var movement in ovehicleMovement.ColMovements)
                {
                    if (movement == null)
                        continue;

                    bool isExternal = false;
                    var workOrder = await _apiClient.GetMWorkOrderByID((int)movement.WorkOrderId);
                    if (movement.WorkOrderId.HasValue)
                    {
                        isExternal = workOrder.IsExternal == true;
                    }

                    VehicleNams? vehicle = isExternal == true
                            ? externalVehicles.Find(p => p.id == movement.VehicleID)
                            : ovehicleMovement.vehicleNams.Find(p => p.id == movement.VehicleID);

                    movement.VehicleName = vehicle?.VehicleName ?? string.Empty;
                }

                return PartialView("MovementList", ovehicleMovement);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
