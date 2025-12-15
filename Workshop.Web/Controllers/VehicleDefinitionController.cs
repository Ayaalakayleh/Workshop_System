using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Controllers;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Controllers
{
    [SessionTimeout]
    public class VehicleDefinitionController : BaseController
    {
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _eRPApiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly string lang;

        public VehicleDefinitionController(VehicleApiClient vehicleApiClient, ERPApiClient eRPApiClient, AccountingApiClient accountingApiClient, 
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _vehicleApiClient = vehicleApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            _eRPApiClient = eRPApiClient;
            _accountingApiClient = accountingApiClient;
        }

        [CustomAuthorize(Permissions.VehicleDefinition.View)]
        public async Task<IActionResult> Index()
        {

            var model = new CreateVehicleDefinitionsModel();
            //ToDo: Caching
            //if (cache.Get(string.Format(CacheKeys.Manufacturers)) != null)
            //{
            //    model.ColManufacturers = (List<Manufacturers>)cache.Get(string.Format(CacheKeys.Manufacturers));
            //}
            //else
            //{
            //    model.ColManufacturers = VehicleApi.GetAllManufacturers(language);
            //    cache.Set(string.Format(CacheKeys.Manufacturers), model.ColManufacturers, DateTimeOffset.Now.AddHours(48));
            //}

            model.ColManufacturers = await _vehicleApiClient.GetAllManufacturers(lang);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetDataList([FromBody] VehicleFilter filter)
        {
            filter ??= new VehicleFilter();
            try
            {
                List<VehicleDefinitions> vehicles = new List<VehicleDefinitions>();

                vehicles = await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicles(filter.Page, filter.ManufacturerId, filter.PlateNumber, null);
                return PartialView("_VehicleList", vehicles);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        public async Task<JsonResult> FillVehicleModel(int id)
        {
            var colVehicleModel = new List<VehicleModel>();


            //ToDo: Caching
            //if (cache.Get(string.Format(CacheKeys.VehicleModel, id)) != null)
            //{
            //    colVehicleModel = (List<VehicleModel>)cache.Get(string.Format(CacheKeys.VehicleModel, id));
            //}
            //else
            //{
            //    colVehicleModel = VehicleApi.GetAllVehicleModel(id, language);
            //    cache.Set(string.Format(CacheKeys.VehicleModel, id), colVehicleModel, DateTimeOffset.Now.AddHours(48));
            //}

            colVehicleModel = await _vehicleApiClient.GetAllVehicleModel(id, lang);

            //Check
            //return Json(colVehicleModel, JsonRequestBehavior.AllowGet);
            return Json(colVehicleModel);
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
            return PartialView("_VehicleList", colVehicleDefinitions);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(int id, int Type = 1) // 1 internal , 2 external
        {
            var oVehicleDefinitions = new VehicleDefinitions();
            if (Type == 1)
            {
                oVehicleDefinitions = await _vehicleApiClient.GetVehicleDetails(id, lang);
            }
            else
            {
                oVehicleDefinitions = await _vehicleApiClient.GetExternalVehicleDetails(id, lang);
            }

            //Check
            //return Json(oVehicleDefinitions, JsonRequestBehavior.AllowGet);
            return Json(oVehicleDefinitions, new JsonSerializerOptions
            {
                PropertyNamingPolicy = null, // This preserves the original case
                DictionaryKeyPolicy = null
            });

        }

        [HttpGet]
        [CustomAuthorize(Permissions.VehicleDefinition.Edit)]
        public async Task<IActionResult> Edit(int? Id)
        {
            CreateVehicleDefinitionsModel vehicle = new CreateVehicleDefinitionsModel();

            if (Id > 0)
            {
                vehicle = await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById((int)Id);
            }

            //ToDo : Caching
            //if (cache.Get(string.Format(CacheKeys.Manufacturers)) != null)
            //{
            //    vehicle.ColManufacturers = (List<Manufacturers>)cache.Get(string.Format(CacheKeys.Manufacturers));
            //}
            //else
            //{
            //    vehicle.ColManufacturers = await _vehicleApiClient.GetAllManufacturers(language);
            //    cache.Set(string.Format(CacheKeys.Manufacturers), vehicle.ColManufacturers, DateTimeOffset.Now.AddHours(48));
            //}

            vehicle.ColManufacturers = await _vehicleApiClient.GetAllManufacturers(lang);
            var isCompanyCenterialized = 1;
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            ViewBag.Customers = allCustomers;
            vehicle.ColVehicleModels = await _vehicleApiClient.GetAllVehicleModel(vehicle.ManufacturerId, lang);
            //ToDo : Caching
            //if (cache.Get(string.Format(CacheKeys.VehicleColors)) != null)
            //{
            //    vehicle.ColVehicleColors = (List<M_VehicleColor>)cache.Get(string.Format(CacheKeys.VehicleColors));
            //}
            //else
            //{
            //    vehicle.ColVehicleColors = await _vehicleApiClient.GetAllColors(language);
            //    cache.Set(string.Format(CacheKeys.VehicleColors), vehicle.ColVehicleColors, DateTimeOffset.Now.AddHours(48));
            //}
            vehicle.ColVehicleColors = await _vehicleApiClient.GetAllColors(lang);

            return View(vehicle);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.VehicleDefinition.Edit)]
        public async Task<IActionResult> Edit(CreateVehicleDefinitionsModel vehicle)
        {
            var result = new TempData();
            vehicle.CreatedBy = UserId.ToString();

            try
            {

                var dataResult = await _vehicleApiClient.InsertExternalVehicle(vehicle);

                if (dataResult == 1)
                {
                    result.IsSuccess = false;
                    result.Type = "error";
                    result.Message = "Plate Number Already Exist";
                    return Json(result);
                }
                else if (dataResult == 2)
                {
                    result.IsSuccess = false;
                    result.Type = "error";
                    result.Message = "Chassis No Already Exist";
                    return Json(result);
                }
                else
                {
                    result.IsSuccess = true;
                    result.Type = "success";
                    return Json(result);
                }
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Type = "error";
                return Json(result);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetVehiclesDDL(string search, int id = 0)
        {
            var VehicleNams = new List<VehicleNams>();
            //ToDo : Caching
            //if (cache.Get(string.Format(CacheKeys.VehiclesDDL)) != null)
            //{
            //    VehicleNams = (List<VehicleNams>)cache.Get(string.Format(CacheKeys.VehiclesDDL));
            //}
            //else
            //{
            //    VehicleNams = VehicleApi.GetVehiclesDDL(language, CompanyId);
            //    cache.Set(string.Format(CacheKeys.VehiclesDDL), VehicleNams, DateTimeOffset.Now.AddHours(24));
            //}

            VehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);

            if (!string.IsNullOrEmpty(search))
                VehicleNams = VehicleNams.Where(a => a.VehicleName.ToLower().Contains(search.Trim().ToLower())).ToList();

            if (id > 0)
                VehicleNams = VehicleNams.Where(a => a.id == id).ToList();

            //Check
            //return Json(VehicleNams, JsonRequestBehavior.AllowGet);
            return Json(VehicleNams);
        }
		[HttpGet]
		public async Task<IActionResult> GetVehiclesClass() 
		{
			List<VehicleClass> oVehicleClass = new List<VehicleClass>();
		
				oVehicleClass = await _vehicleApiClient.GetAllVehicleClass(lang);
			
			return Json(oVehicleClass);

		}
	}
}