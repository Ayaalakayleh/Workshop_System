using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Resources;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class ServiceReminderController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly WorkshopApiClient _serviceReminderService;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly IStringLocalizer<Common> _localizer;
        private readonly string lang;
        private static readonly string INDEX_PAGE = "Index";
        public ServiceReminderController(IConfiguration configuration, IWebHostEnvironment env, WorkshopApiClient serviceReminderService,
            VehicleApiClient vehicleApiClient, AccountingApiClient accountingApiClient, ERPApiClient erpApiClient,
            IStringLocalizer<Common> localizer, IMemoryCache cache) : base(cache, configuration, env)
        {
            _configuration = configuration;
            _env = env;
            _serviceReminderService = serviceReminderService;
            _vehicleApiClient = vehicleApiClient;
            _accountingApiClient = accountingApiClient;
            _localizer = localizer;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            _erpApiClient = erpApiClient;
        }

        [CustomAuthorize(Permissions.ServiceReminder.View)]
        public async Task<IActionResult> Index(ServiceReminderPageVM serviceReminderDTO, int PageNumber, int PageSize)
        {

            if(serviceReminderDTO == null)
                serviceReminderDTO = new ServiceReminderPageVM();
            if (serviceReminderDTO.ReminderForm == null)
                serviceReminderDTO.ReminderForm = new ServiceReminderDTO();
            if (serviceReminderDTO.Reminders == null)
                serviceReminderDTO.Reminders = new List<GetServiceReminderDTO>();
            GetServiceReminderDTO? getServiceReminderDTO = new GetServiceReminderDTO();
            getServiceReminderDTO.PageSize = 25;
            getServiceReminderDTO.PageNumber = 1;
            getServiceReminderDTO.ManufacturerId = serviceReminderDTO.ReminderForm.ManufacturerId;
            getServiceReminderDTO.VehicleId = serviceReminderDTO.ReminderForm.VehicleId;
            getServiceReminderDTO.VehicleModelId = serviceReminderDTO.ReminderForm.VehicleModelId;
            getServiceReminderDTO.ItemId = serviceReminderDTO.ReminderForm.ItemId;
            getServiceReminderDTO.ManufacturingYear = serviceReminderDTO.ReminderForm.ManufacturingYear;

            var result = new ServiceReminderPageVM();
            result.Reminders = await _serviceReminderService.GetAllServiceRemindersAsync(getServiceReminderDTO) ;
            result.ReminderForm = new ServiceReminderDTO();

            ViewBag.dateUnits = (from ServiceReminderTimeUnitEnum source in Enum.GetValues(typeof(ServiceReminderTimeUnitEnum))
                                 select new SelectListItem
                                 {
                                     Value = Convert.ToInt32(source).ToString(),
                                     Text = GetLocalizedTimeUnitName(source, lang)
                                 }).ToList();

            var manufacturers = await _vehicleApiClient.GetAllManufacturers();


            ViewBag.manufacturers = manufacturers.Select(t => new SelectListItem { Text = lang == "en" ? t.ManufacturerPrimaryName : t.ManufacturerSecondaryName, Value = t.Id.ToString() }).ToList();
            //ViewBag.vechile = vechiles.Select(v => new SelectListItem { Text = lang == "en" ? v.VehicleClassPrimaryName : v.VehicleClassSecondaryName, Value = v.Id.ToString() }).ToList();

            var vehicle = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            foreach (var item in result.Reminders)
            {
                item.CurrentMeter = vehicle.Where(v => v.id == item.VehicleId).Select(r => r.CurrentMeter).FirstOrDefault();
            }
            ViewBag.vehicle = vehicle.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleName : v.VehicleName, // <-- fallback if not English
                Value = v.id.ToString()
            }).ToList();
            var vehiclesModels = await _vehicleApiClient.GetAllVehicleModel(0, lang);
            ViewBag.vehiclesModels = vehiclesModels.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleModelPrimaryName : v.VehicleModelSecondaryName, // <-- fallback if not English
                Value = v.Id.ToString()
            }).ToList();
            foreach (var item in result.Reminders)
            {
                item.VehicleName = vehicle.Where(a => a.id == item.VehicleId).Select(a => a.VehicleName).FirstOrDefault();
            }
            var services = await _serviceReminderService.GetAllRTSCodesDDLAsync();
            //result.ReminderForm.Services =services;

            ViewBag.services = services.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.PrimaryName : v.SecondaryName, // <-- fallback if not English
                Value = v.Id.ToString()
            }).ToList();
            foreach(var item in result.Reminders)
            {
                item.ServiceName = services.Where(a => a.Id == item.ItemId).Select(a => a.PrimaryName ?? a.SecondaryName).FirstOrDefault();
            }
            var vehicleClasss = await _vehicleApiClient.GetAllVehicleClass();

            ViewBag.vehicleClasss = vehicleClasss.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleClassPrimaryName : v.VehicleClassSecondaryName, // <-- fallback if not English
                Value = v.Id.ToString()
            }).ToList();
            foreach (var item in result.Reminders)
            {
                item.ReminderStatusName = Enum.GetName(typeof (ServiceReminderStatusEnum), item.ReminderStatus);
            }
            ViewBag.ServiceReminderStatus = (from ServiceReminderStatusEnum source in Enum.GetValues(typeof(ServiceReminderStatusEnum))
                                             select new SelectListItem
                                             {
                                                 Value = Convert.ToInt32(source).ToString(),
                                                 Text = source.ToString()
                                             }).ToList();

            var DueServiceReminders = await _serviceReminderService.GetDueServiceReminders();
            ViewBag.DueServiceReminders = DueServiceReminders;


            var groups = await _erpApiClient.GetAllNotificationGroups(lang, BranchId, CompanyId);

            var modules = await _erpApiClient.GetAllModules();

            ViewBag.Groups = modules.Select(g => new SelectListItem
            {
                Text = lang == "en" ? g.ModulePrimaryName : g.ModuleSecondaryName,
                Value = g.Id.ToString()
            }).ToList();


            //Load chart data

            result.ScheduledRemindersCount =
            result.Reminders.Count(s => s.ReminderStatus == (int)ServiceReminderStatusEnum.Scheduled);

            result.DueSoonRemindersCount =
                result.Reminders.Count(s => s.ReminderStatus == (int)ServiceReminderStatusEnum.DueSoon);

            result.DueSoonRemindersCount =
                result.Reminders.Count(s => s.ReminderStatus == (int)ServiceReminderStatusEnum.Overdue);

            //--------
            return View(result);
        }

        public async Task<object> GetVehicleByManufacturerId(int manufacturerId)
        {
            var vehicles = await _vehicleApiClient.GetAllVehicleModel(manufacturerId, lang);

            return vehicles;
        }

        [CustomAuthorize(Permissions.ServiceReminder.Edit)]
        public async Task<IActionResult> Add()
        {
            // Prepare ViewBag data for dropdowns (similar to Index action)
            ViewBag.dateUnits = (from ServiceReminderTimeUnitEnum source in Enum.GetValues(typeof(ServiceReminderTimeUnitEnum))
                                 select new SelectListItem
                                 {
                                     Value = Convert.ToInt32(source).ToString(),
                                     Text = GetLocalizedTimeUnitName(source, lang)
                                 }).ToList();

            var manufacturers = await _vehicleApiClient.GetAllManufacturers();
            ViewBag.manufacturers = manufacturers.Select(t => new SelectListItem { Text = lang == "en" ? t.ManufacturerPrimaryName : t.ManufacturerSecondaryName, Value = t.Id.ToString() }).ToList();

            var vehicle = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            ViewBag.vehicle = vehicle.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleName : v.VehicleName,
                Value = v.id.ToString()
            }).ToList();

            var vehiclesModels = await _vehicleApiClient.GetAllVehicleModel(0, lang);
            ViewBag.vehiclesModels = vehiclesModels.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleModelPrimaryName : v.VehicleModelSecondaryName,
                Value = v.Id.ToString()
            }).ToList();

            var services = await _serviceReminderService.GetAllRTSCodesDDLAsync();
            ViewBag.services = services.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.PrimaryName : v.SecondaryName,
                Value = v.Id.ToString()
            }).ToList();

            var vehicleClasss = await _vehicleApiClient.GetAllVehicleClass();
            ViewBag.vehicleClasss = vehicleClasss.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleClassPrimaryName : v.VehicleClassSecondaryName,
                Value = v.Id.ToString()
            }).ToList();

            var groups = await _erpApiClient.GetAllNotificationGroups(lang, BranchId, CompanyId);
            var modules = await _erpApiClient.GetAllModules();
            ViewBag.Groups = modules.Select(g => new SelectListItem
            {
                Text = lang == "en" ? g.ModulePrimaryName : g.ModuleSecondaryName,
                Value = g.Id.ToString()
            }).ToList();

            // Creating new record - return empty form with default values
            var viewModel = new ServiceReminderPageVM
            {
                ReminderForm = new ServiceReminderDTO
                {
                    Id = 0, // New record
                    UseSameStart = true, // Default to true as per DTO
                    IsManually = false,
                    HasNotification = false,
                    TimeIntervalUnit = 2, // Default to months
                    TimeDueUnit = 2, // Default to months
                    Repates = 1
                }
            };

            return View(viewModel);
        }

        [CustomAuthorize(Permissions.ServiceReminder.Edit)]
        public async Task<IActionResult> Edit(int Id)
        {
            // Prepare ViewBag data for dropdowns (similar to Index action)
            ViewBag.dateUnits = (from ServiceReminderTimeUnitEnum source in Enum.GetValues(typeof(ServiceReminderTimeUnitEnum))
                                 select new SelectListItem
                                 {
                                     Value = Convert.ToInt32(source).ToString(),
                                     Text = GetLocalizedTimeUnitName(source, lang)
                                 }).ToList();

            var manufacturers = await _vehicleApiClient.GetAllManufacturers();
            ViewBag.manufacturers = manufacturers.Select(t => new SelectListItem { Text = lang == "en" ? t.ManufacturerPrimaryName : t.ManufacturerSecondaryName, Value = t.Id.ToString() }).ToList();

            var vehicle = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            ViewBag.vehicle = vehicle.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleName : v.VehicleName,
                Value = v.id.ToString()
            }).ToList();

            var vehiclesModels = await _vehicleApiClient.GetAllVehicleModel(0, lang);
            ViewBag.vehiclesModels = vehiclesModels.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleModelPrimaryName : v.VehicleModelSecondaryName,
                Value = v.Id.ToString()
            }).ToList();

            var services = await _serviceReminderService.GetAllRTSCodesDDLAsync();
            ViewBag.services = services.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.PrimaryName : v.SecondaryName,
                Value = v.Id.ToString()
            }).ToList();

            var vehicleClasss = await _vehicleApiClient.GetAllVehicleClass();
            ViewBag.vehicleClasss = vehicleClasss.Select(v => new SelectListItem
            {
                Text = lang == "en" ? v.VehicleClassPrimaryName : v.VehicleClassSecondaryName,
                Value = v.Id.ToString()
            }).ToList();

            var groups = await _erpApiClient.GetAllNotificationGroups(lang, BranchId, CompanyId);
            var modules = await _erpApiClient.GetAllModules();
            ViewBag.Groups = modules.Select(g => new SelectListItem
            {
                Text = lang == "en" ? g.ModulePrimaryName : g.ModuleSecondaryName,
                Value = g.Id.ToString()
            }).ToList();

            // Editing existing record
            var result = await _serviceReminderService.GetServiceReminderByIdAsync(Id);

            if (!string.IsNullOrEmpty(result.NotificationsGroup))
            {
                var ids = result.NotificationsGroup
                                .Split(',')                       // split string into array
                                .Select(x => int.Parse(x.Trim())) // convert each to int
                                .ToList();

                result.NotificationsGroupId.AddRange(ids);
            }

            // Determine if UseSameStart should be checked based on existing data
            var useSameStart = result.UseSameStart;
            // If there's already a start date, uncheck UseSameStart so fields are visible
            if (result.StartDate.HasValue || !string.IsNullOrEmpty(result.StartMeter.ToString()))
            {
                useSameStart = false;
            }

            // Create a view model for the edit page
            var viewModel = new ServiceReminderPageVM
            {
                ReminderForm = new ServiceReminderDTO
                {
                    Id = result.Id,
                    ManufacturerId = result.ManufacturerId,
                    VehicleId = result.VehicleId,
                    VehicleModelId = result.VehicleModelId,
                    ManufacturingYear = result.ManufacturingYear,
                    VehicleGroupId = result.VehicleGroupId,
                    ItemId = result.ItemId,
                    Repates = result.Repates,
                    TimeInterval = result.TimeInterval,
                    TimeIntervalUnit = result.TimeIntervalUnit,
                    TimeDue = result.TimeDue,
                    TimeDueUnit = result.TimeDueUnit,
                    PrimaryMeterInterval = result.PrimaryMeterInterval,
                    PrimaryMeterDue = result.PrimaryMeterDue,
                    IsManually = result.IsManually,
                    ManualDate = result.ManualDate,
                    ManualPrimaryMeter = result.ManualPrimaryMeter,
                    HasNotification = result.HasNotification,
                    NotificationsGroupId = result.NotificationsGroupId,
                    UseSameStart = useSameStart,
                    StartDate = result.StartDate,
                    StartMeter = result.StartMeter,
                    CurrentMeter = result.CurrentMeter,
                    ServiceRemindersSchedules = result.ServiceRemindersSchedules
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.ServiceReminder.Edit)]
        public async Task<IActionResult> Add(ServiceReminderPageVM serviceReminderDTO)
        {
            // Validate Manufacturing Year is required
            if (!serviceReminderDTO.ReminderForm.ManufacturingYear.HasValue || serviceReminderDTO.ReminderForm.ManufacturingYear.Value <= 0)
            {
                ModelState.AddModelError("ReminderForm.ManufacturingYear", "Manufacturing Year is required.");

                ViewBag.dateUnits = (from ServiceReminderTimeUnitEnum source in Enum.GetValues(typeof(ServiceReminderTimeUnitEnum))
                                     select new SelectListItem
                                     {
                                         Value = Convert.ToInt32(source).ToString(),
                                         Text = GetLocalizedTimeUnitName(source, lang)
                                     }).ToList();

                return View(serviceReminderDTO);
            }

            if (serviceReminderDTO.ReminderForm.vehicleNams == null)
                serviceReminderDTO.ReminderForm.vehicleNams = new List<VehicleNams>();
            if (serviceReminderDTO.ReminderForm.ColManufacturers == null)
                serviceReminderDTO.ReminderForm.ColManufacturers = new List<Manufacturers>();
            if (serviceReminderDTO.ReminderForm.vehicleNams == null)
                serviceReminderDTO.ReminderForm.vehicleNams = new List<VehicleNams>();
            if (serviceReminderDTO.ReminderForm.Services == null)
                serviceReminderDTO.ReminderForm.Services = new List<Item>();

            VehicleDefinitions vehicle = new VehicleDefinitions();
            vehicle.ManufacturerId = serviceReminderDTO.ReminderForm.ManufacturerId??0;
            vehicle.VehicleModelId = serviceReminderDTO.ReminderForm.VehicleModelId??0;
            vehicle.ManufacturingYear = serviceReminderDTO.ReminderForm.ManufacturingYear ?? 0;
            var vehicles = (await _vehicleApiClient.GetServiceScheduleVehicle(vehicle));

            foreach (var item in vehicles)
            {
                CreateServiceReminderDTO createServiceReminderDTO = new CreateServiceReminderDTO
                {
                    vehicleNams = serviceReminderDTO.ReminderForm.vehicleNams,
                    VehicleGroupId = serviceReminderDTO.ReminderForm.VehicleGroupId,
                    VehicleId = serviceReminderDTO.ReminderForm.VehicleId, //*
                    VehicleModelId = serviceReminderDTO.ReminderForm.VehicleModelId, //*
                    VehicleName = vehicles.Where(item => item.Id == serviceReminderDTO.ReminderForm.VehicleId).Select(i => i.VehicleName).FirstOrDefault(),
                    NextDuePrimaryMeter = serviceReminderDTO.ReminderForm.NextDuePrimaryMeter,
                    NextDate = serviceReminderDTO.ReminderForm.NextDate,
                    NextPrimaryMeter = serviceReminderDTO.ReminderForm.NextPrimaryMeter,
                    PrimaryMeterInterval = serviceReminderDTO.ReminderForm.PrimaryMeterInterval,
                    ColManufacturers = serviceReminderDTO.ReminderForm.ColManufacturers,
                    CurrentMeter = vehicles.Where(item => item.Id == serviceReminderDTO.ReminderForm.VehicleId).Select(i => i.CurrentMeter).FirstOrDefault(), //serviceReminderDTO.ReminderForm.CurrentMeter,
                    Id = serviceReminderDTO.ReminderForm.Id, //*
                    ManufacturerId = serviceReminderDTO.ReminderForm.ManufacturerId, //*
                    ManualPrimaryMeter = serviceReminderDTO.ReminderForm.ManualPrimaryMeter,
                    ManualDate = DateTime.Now,
                    ManufacturingYear = serviceReminderDTO.ReminderForm.ManufacturingYear,//*
                    Repates = serviceReminderDTO.ReminderForm.Repates, //*
                    Services = serviceReminderDTO.ReminderForm.Services, //*
                    ServiceName = serviceReminderDTO.ReminderForm.ServiceName,
                    ItemId = serviceReminderDTO.ReminderForm.ItemId,
                    PrimaryMeterDue = serviceReminderDTO.ReminderForm.PrimaryMeterDue,
                    TimeDue = serviceReminderDTO.ReminderForm.TimeDue, //*
                    TimeDueUnit = serviceReminderDTO.ReminderForm.TimeDueUnit,
                    TimeInterval = serviceReminderDTO.ReminderForm.TimeInterval, //*
                    TimeIntervalUnit = serviceReminderDTO.ReminderForm.TimeIntervalUnit, //*
                    IsManually = serviceReminderDTO.ReminderForm.IsManually,
                    HasNotification = serviceReminderDTO.ReminderForm.HasNotification,
                    StartDate = serviceReminderDTO.ReminderForm.StartDate,
                    StartMeter = serviceReminderDTO.ReminderForm.StartMeter,
                    UseSameStart = serviceReminderDTO.ReminderForm.UseSameStart,
                    ReminderStatusSecondaryName = serviceReminderDTO.ReminderForm.ReminderStatusSecondaryName,
                    ReminderStatusPrimaryName = serviceReminderDTO.ReminderForm.ReminderStatusPrimaryName,
                    ReminderStatus = serviceReminderDTO.ReminderForm.ReminderStatus,
                    LastCompleted = serviceReminderDTO.ReminderForm.LastCompleted,
                    NotificationsGroupId = serviceReminderDTO.ReminderForm.NotificationsGroupId,
                };

                var response = await _serviceReminderService.AddServiceReminderAsync(createServiceReminderDTO);
            }
           

            return RedirectToAction("Index");
        }

        [HttpPost]
        [CustomAuthorize(Permissions.ServiceReminder.Edit)]
        public async Task<IActionResult> Edit(ServiceReminderPageVM serviceReminderDTO)
        {
            // Validate Manufacturing Year is required
            if (!serviceReminderDTO.ReminderForm.ManufacturingYear.HasValue || serviceReminderDTO.ReminderForm.ManufacturingYear.Value <= 0)
            {
                ModelState.AddModelError("ReminderForm.ManufacturingYear", "Manufacturing Year is required.");
                // Re-populate ViewBag data for the form
                ViewBag.dateUnits = (from ServiceReminderTimeUnitEnum source in Enum.GetValues(typeof(ServiceReminderTimeUnitEnum))
                                     select new SelectListItem
                                     {
                                         Value = Convert.ToInt32(source).ToString(),
                                         Text = GetLocalizedTimeUnitName(source, lang)
                                     }).ToList();

                return View(serviceReminderDTO);
            }

            if (serviceReminderDTO.ReminderForm.vehicleNams == null)
                serviceReminderDTO.ReminderForm.vehicleNams = new List<VehicleNams>();
            if (serviceReminderDTO.ReminderForm.ColManufacturers == null)
                serviceReminderDTO.ReminderForm.ColManufacturers = new List<Manufacturers>();
            if (serviceReminderDTO.ReminderForm.vehicleNams == null)
                serviceReminderDTO.ReminderForm.vehicleNams = new List<VehicleNams>();
            if (serviceReminderDTO.ReminderForm.Services == null)
                serviceReminderDTO.ReminderForm.Services = new List<Item>();

            var vehicle = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);

            UpdateServiceReminderDTO updateServiceReminderDTO = new UpdateServiceReminderDTO
            {
                vehicleNams = serviceReminderDTO.ReminderForm.vehicleNams,
                VehicleGroupId = serviceReminderDTO.ReminderForm.VehicleGroupId,
                VehicleId = serviceReminderDTO.ReminderForm.VehicleId, //*
                VehicleModelId = serviceReminderDTO.ReminderForm.VehicleModelId, //*
                VehicleName = serviceReminderDTO.ReminderForm.VehicleName,
                NextDuePrimaryMeter = serviceReminderDTO.ReminderForm.NextDuePrimaryMeter,
                NextDate = serviceReminderDTO.ReminderForm.NextDate,
                NextPrimaryMeter = serviceReminderDTO.ReminderForm.NextPrimaryMeter,
                PrimaryMeterInterval = serviceReminderDTO.ReminderForm.PrimaryMeterInterval,
                ColManufacturers = serviceReminderDTO.ReminderForm.ColManufacturers,
                CurrentMeter = vehicle.Where(item => item.id == serviceReminderDTO.ReminderForm.VehicleId).Select(i => i.CurrentMeter).FirstOrDefault(),
                Id = serviceReminderDTO.ReminderForm.Id, //*
                ManufacturerId = serviceReminderDTO.ReminderForm.ManufacturerId, //*
                ManualPrimaryMeter = serviceReminderDTO.ReminderForm.ManualPrimaryMeter,
                ManualDate = serviceReminderDTO.ReminderForm.ManualDate,
                ManufacturingYear = serviceReminderDTO.ReminderForm.ManufacturingYear,//*
                Repates = serviceReminderDTO.ReminderForm.Repates, //*
                Services = serviceReminderDTO.ReminderForm.Services, //*
                ItemId = serviceReminderDTO.ReminderForm.ItemId,
                PrimaryMeterDue = serviceReminderDTO.ReminderForm.PrimaryMeterDue,
                TimeDue = serviceReminderDTO.ReminderForm.TimeDue, //*
                TimeDueUnit = serviceReminderDTO.ReminderForm.TimeDueUnit,
                TimeInterval = serviceReminderDTO.ReminderForm.TimeInterval, //*
                TimeIntervalUnit = serviceReminderDTO.ReminderForm.TimeIntervalUnit, //*
                IsManually = serviceReminderDTO.ReminderForm.IsManually,
                HasNotification = serviceReminderDTO.ReminderForm.HasNotification,
                StartDate = serviceReminderDTO.ReminderForm.StartDate,
                StartMeter = serviceReminderDTO.ReminderForm.StartMeter,
                UseSameStart = serviceReminderDTO.ReminderForm.UseSameStart,
                ReminderStatusSecondaryName = serviceReminderDTO.ReminderForm.ReminderStatusSecondaryName,
                ReminderStatusPrimaryName = serviceReminderDTO.ReminderForm.ReminderStatusPrimaryName,
                ReminderStatus = serviceReminderDTO.ReminderForm.ReminderStatus,
                LastCompleted = serviceReminderDTO.ReminderForm.LastCompleted,
                NotificationsGroupId = serviceReminderDTO.ReminderForm.NotificationsGroupId,
            };

            var response = await _serviceReminderService.UpdateServiceReminderAsync(updateServiceReminderDTO);

            return RedirectToAction("Index");
        }

        [CustomAuthorize(Permissions.ServiceReminder.Delete)]
        public async Task<bool> Delete(int Id)
        {
  
            GetServiceReminderDTO? getServiceReminderDTO = new GetServiceReminderDTO();
            getServiceReminderDTO.PageSize = 25;
            getServiceReminderDTO.PageNumber = 1;
            var result = new ServiceReminderPageVM();
            return await _serviceReminderService.DeleteServiceRemindersAsync(Id);

        }

        [HttpGet]
        public async Task<List<ReminderStatus>> GetServiceRemindersStatus()
        {
            var reminderStatusResult = await _serviceReminderService.GetServiceRemindersStatus();
            return reminderStatusResult?? new List<ReminderStatus>();
        }

        [HttpGet]
        public async Task<IEnumerable<ReminderStatus>> GetDueServiceReminders()
        {
            var reminderStatusResult = await _serviceReminderService.GetDueServiceReminders();

            return reminderStatusResult.Select(s =>
                new ReminderStatus(Enum.GetName<ServiceReminderStatusEnum>((ServiceReminderStatusEnum)s.ReminderStatus), s.StatusCount));

        }

        public static string GetCurrentLanguage()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }

        private string GetLocalizedTimeUnitName(ServiceReminderTimeUnitEnum unit, string language)
        {
            switch (unit)
            {
                case ServiceReminderTimeUnitEnum.Days:
                    return _localizer["Label_Day"];
                case ServiceReminderTimeUnitEnum.Weeks:
                    return _localizer["Label_Week"];
                case ServiceReminderTimeUnitEnum.Months:
                    return _localizer["Label_Month"];
                case ServiceReminderTimeUnitEnum.Years:
                    return _localizer["Label_Year"];
                default:
                    return unit.ToString();
            }
        }


    }
}
