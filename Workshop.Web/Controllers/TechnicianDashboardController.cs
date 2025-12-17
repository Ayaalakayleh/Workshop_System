using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class TechnicianDashboardController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        public readonly string lang;
        private readonly StdSchedulerFactory _clockingSchedulerFactory;
        private IScheduler? _clockingScheduler;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TechnicianDashboardController> _logger;

        private const string SessionKeyMainClockingModel = "MainClockingModel_v1";
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        NameValueCollection props = new NameValueCollection
        {
            { "quartz.serializer.type", "binary" }
        };

        public TechnicianDashboardController(WorkshopApiClient apiClient, IRecurringJobManager recurringJobManager,
            IBackgroundJobClient backgroundJobClient, IConfiguration configuration, ILogger<TechnicianDashboardController> logger, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            _clockingSchedulerFactory = new StdSchedulerFactory(props);
            _recurringJobManager = recurringJobManager ?? throw new ArgumentNullException(nameof(recurringJobManager));
            _backgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Session-backed main model helper
        private ClockingModel GetMainModel()
        {
            try
            {
                if (HttpContext == null)
                {
                    _logger.LogWarning("GetMainModel called outside of an HTTP context; returning new ClockingModel.");
                    return new ClockingModel();
                }

                var session = HttpContext.Session;
                if (session == null)
                {
                    _logger.LogWarning("Session is not available on HttpContext; returning new ClockingModel.");
                    return new ClockingModel();
                }

                var json = session.GetString(SessionKeyMainClockingModel);
                if (string.IsNullOrEmpty(json)) return new ClockingModel();
                var model = JsonSerializer.Deserialize<ClockingModel>(json, _jsonOptions);
                return model ?? new ClockingModel();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize main clocking model from session, creating new one.");
                return new ClockingModel();
            }
        }

        private void SaveMainModel(ClockingModel model)
        {
            try
            {
                if (model == null) return;

                if (HttpContext == null)
                {
                    _logger.LogWarning("SaveMainModel called outside of an HTTP context; skipping save.");
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
                _logger.LogWarning(ex, "Failed to serialize main clocking model to session.");
            }
        }

        [CustomAuthorize(Permissions.TechnicianDashboard.View)]
        public async Task<IActionResult> Index()
        {
            try
            {
                // If technician already authenticated in session, initialize and redirect to Clocking
                if (HttpContext.Session.Keys.Contains("IsAuthenticated") && HttpContext.Session.Keys.Contains("TechnicianID"))
                {
                    if (bool.TryParse(HttpContext.Session.GetString("IsAuthenticated"), out var isAuthenticated) && isAuthenticated)
                    {
                        int technicianId = HttpContext.Session.GetInt32("TechnicianID") ?? 0;
                        if (technicianId > 0)
                        {
                            var technician = await _apiClient.GetTechnicianByIdAsync(technicianId);
                            if (technician != null)
                            {
                                // Prepare model and save to session for Clocking
                                var model = new ClockingModel
                                {
                                    ClockingForm = new ClockingDTO { TechnicianID = technician.Id, TechnicianName = technician.PrimaryName },
                                    ClockingList = new List<ClockingDTO>(),
                                    Labourlines = new List<CreateWIPServiceDTO>(),
                                    WIPS = new List<WIPDTO>()
                                };
                                SaveMainModel(model);
                                return RedirectToAction(nameof(Clocking));
                            }
                        }
                    }
                }

                var filterTechnician = new FilterTechnicianDTO { PageNumber = 1, Email = string.Empty, Name = string.Empty, WorkshopId = BranchId };
                var technicians = (await _apiClient.GetAllPINTechniciansAsync(filterTechnician))?.ToList() ?? new List<TechnicianDTO>();

                var modelView = new ClockingModel
                {
                    ClockingForm = new ClockingDTO { TechnicianID = 0, WIPID = 0, RTSID = 0, TechnicianName = string.Empty },
                    ClockingList = new List<ClockingDTO>(),
                    Technicians = technicians,
                    Labourlines = new List<CreateWIPServiceDTO>(),
                    WIPS = new List<WIPDTO>()
                };

                // Await API calls and populate viewbag / model properly
                var filterWIPDTO = new FilterWIPDTO();
                var wips = await _apiClient.GetAllWIPsAsync(filterWIPDTO);
                var labourLines = await _apiClient.GetAllRTSCodesDDLAsync();

                ViewBag.ClockingList = wips?.Select(w => new SelectListItem { Value = w.Id.ToString(), Text = "WIP - " + w.Id }) ?? new List<SelectListItem>();
                // labourLines is a collection of RTSCodeDTO; expose it directly for the view
                ViewBag.Labourlines = labourLines ?? Enumerable.Empty<RTSCodeDTO>();
                ViewBag.SerivcesSchedules = null;

                // One call to get schedules
                var WIPSchedules = await _apiClient.GetClockingFilter();

                // Initialize and save empty main model to session
                SaveMainModel(modelView);

                return View(modelView);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index action failed");
                return View(new ClockingModel
                {
                    ClockingForm = new ClockingDTO(),
                    Technicians = new List<TechnicianDTO>(),
                    Labourlines = new List<CreateWIPServiceDTO>()
                });
            }
        }

        [HttpGet]
        [CustomAuthorize(Permissions.TechnicianDashboard.Edit)]
        public async Task<IActionResult> Edit()
        {
            var filterTechnician = new FilterTechnicianDTO { PageNumber = 1, Email = string.Empty, Name = string.Empty, WorkshopId = BranchId };
            var technician = await _apiClient.GetAllPINTechniciansAsync(filterTechnician);
            return View(technician);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Permissions.TechnicianDashboard.Edit)]
        public async Task<IActionResult> Edit(ClockingModel clocking)
        {
            // Mirror GET for now
            var filterTechnician = new FilterTechnicianDTO { PageNumber = 1, Email = string.Empty, Name = string.Empty, WorkshopId = BranchId };
            var technician = await _apiClient.GetAllPINTechniciansAsync(filterTechnician);
            return View(technician);
        }

        [HttpGet]
        [CustomAuthorize(Permissions.TechnicianDashboard.EditClock)]
        public async Task<IActionResult> EditClock()
        {
            var filterTechnician = new FilterTechnicianDTO { PageNumber = 1, Email = string.Empty, Name = string.Empty, WorkshopId = BranchId };
            var technician = await _apiClient.GetAllPINTechniciansAsync(filterTechnician);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Permissions.TechnicianDashboard.EditClock)]
        public async Task<IActionResult> EditClock(ClockingModel clocking)
        {
            var main = GetMainModel();

            try
            {
                if (clocking?.ClockingForm != null && (clocking.ClockingForm.StatusID ?? 0) == (int)Status.Working)
                {
                    clocking.ClockingForm.StartedAt = clocking.ClockingForm.StartedAt ?? DateTime.Now;

                    // Ensure labourlines are available
                    if (main.Labourlines == null || !main.Labourlines.Any())
                    {
                        var schedules = await _apiClient.GetClockingFilter();
                        main.Labourlines = schedules?.Select(s => new CreateWIPServiceDTO { Id = (s.RTSId ?? 0), StandardHours = 0 }).ToList() ?? new List<CreateWIPServiceDTO>();
                    }

                    var Rlabour = main?.RTSCodes?.FirstOrDefault(s => s.Id == clocking.ClockingForm.RTSID);
                    clocking.ClockingForm.AllowedTime = Rlabour?.StandardHours ?? 0;

                    main.ClockingList = main.ClockingList ?? new List<ClockingDTO>();

                    var currentClocking = main.ClockingList.Any(e => e.TechnicianID == clocking.ClockingForm.TechnicianID);
                    var checkForNullValues = (clocking.ClockingForm.RTSID == null || clocking.ClockingForm.WIPID == null);
                    if (!currentClocking && !checkForNullValues)
                    {
                        
                        var result = await _apiClient.InsertClock(clocking.ClockingForm);
                        if (result != null && result > 0)
                        {
                            clocking.ClockingForm.ID = result;
                            main.ClockingList.Add(clocking.ClockingForm);
                            main.ClockingForm = clocking.ClockingForm;
                            SaveMainModel(main);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EditClock failed");
            }

            return RedirectToAction(nameof(Clocking));
        }

        [HttpPost]
        public async Task<IEnumerable<SelectListItem>> SetWIP([FromBody] int WIPId)
        {
            var main = GetMainModel();
            main.ClockingForm = main.ClockingForm ?? new ClockingDTO();
            main.ClockingForm.WIPID = WIPId;

            var services = await _apiClient.WIP_GetServicesById(WIPId);

            // Build service list from schedules as authoritative source
            var WIPSchedules = await _apiClient.GetClockingFilter();

            var bServiceIds = services
    .Where(s => s?.StatusCode == "B")
    .Select(s => s?.Id)
    .ToHashSet();

            var serviceList = WIPSchedules?
                .Where(s => s.WIPId == WIPId && bServiceIds.Contains(s.RTSId ?? 0))
                .Select(x => new { x.RTSId, Text = lang == "en" ? x.RTSPrimaryName : x.RTSSecondaryName })
                .Distinct()
                .Select(x => new SelectListItem { Value = x.RTSId.ToString(), Text = x.Text })
                .ToList()
                ?? new List<SelectListItem>();

            main.LabourlinesSelectList = serviceList;
            SaveMainModel(main);
            return serviceList;
        }

        [HttpPost]
        public async Task<IEnumerable<SelectListItem>> SetTechnician([FromBody] int TechnicianID)
        {
            var main = GetMainModel();
            var WIPSchedules = await _apiClient.GetClockingFilter();

            var serviceList = WIPSchedules?.Where(s => s.TechnicianId == TechnicianID)
                .Select(x => new { x.WIPId, Text = "WIP - " + x.WIPId })
                .Distinct()
                .Select(x => new SelectListItem { Value = x.WIPId.ToString(), Text = x.Text })
                .ToList() ?? new List<SelectListItem>();
            
            main.WIPSSelectList = serviceList;
            SaveMainModel(main);
            return serviceList;
        }

        public async Task SelectTechnician(int TechnicianID)
        {
            var main = GetMainModel();
            var WIPSchedules = await _apiClient.GetClockingFilter();

            var serviceList = WIPSchedules?.Where(s => s.TechnicianId == TechnicianID)
                .Select(x => new { x.WIPId, Text = "WIP - " + x.WIPId })
                .Distinct()
                .Select(x => new SelectListItem { Value = x.WIPId.ToString(), Text = x.Text })
                .ToList() ?? new List<SelectListItem>();

            main.WIPSSelectList = serviceList;
            SaveMainModel(main);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BreakClock(int ID, int status)
        {
            var main = GetMainModel();

            try
            {
                var clockItem = main.ClockingList?.FirstOrDefault(i => i.ID == ID);
                if (clockItem == null)
                    clockItem = await _apiClient.GetClockById(ID);
                if (clockItem == null)
                    return RedirectToAction(nameof(Clocking));

                var state = (Status)Enum.Parse(typeof(Status), status.ToString());

                if (state == Status.Break)
                {
                    clockItem.StatusID = (int)Status.Break;
                    clockItem.EndedAt = DateTime.Now;
                    
                    if (clockItem.Elapsed == null) clockItem.Elapsed = TimeSpan.Zero;
                    clockItem.Breaks = (clockItem.Breaks ?? 0) + 1;

                    var result = await _apiClient.UpdateClock(clockItem);
                    if (result > 0 && main.ClockingList != null)
                    {
                        var existing = main.ClockingList.FirstOrDefault(i => i.ID == clockItem.ID);
                        if (existing != null)
                        {
                            existing.EndedAt = clockItem.EndedAt;
                            existing.Elapsed = clockItem.Elapsed;
                        }

                        main.ClockingBreakForm = main.ClockingBreakForm ?? new ClockingBreakDTO();
                        main.ClockingBreakForm.StartAt = DateTime.Now;
                        main.ClockingBreakForm.ClockingID = clockItem.ID;

                        var clockBreakId = await _apiClient.InsertClockBreak(main.ClockingBreakForm);
                        SaveMainModel(main);
                    }
                }
                else if (state == Status.ClockOut)
                {
                    clockItem.StatusID = (int)Status.ClockOut;
                    clockItem.EndedAt = DateTime.Now;
                    clockItem.Elapsed = (clockItem.Elapsed ?? TimeSpan.Zero) + (clockItem.EndedAt - clockItem.StartedAt);

                    var result = await _apiClient.UpdateClock(clockItem);
                    if (result > 0 && main.ClockingList != null)
                    {
                        var existing = main.ClockingList.FirstOrDefault(i => i.ID == clockItem.ID);
                        if (existing != null)
                        {
                            existing.EndedAt = clockItem.EndedAt;
                            existing.Elapsed = clockItem.Elapsed;
                        }

                        main?.LabourlinesSelectList?.RemoveAll(s => s.Value == main?.ClockingList?.FirstOrDefault(i => i.ID == clockItem.ID)?.RTSID.ToString());

                        main?.ClockingList.RemoveAll(i => i.ID == clockItem.ID);


                        SaveMainModel(main);
                    }
                }
                else if (state == Status.Working)
                {
                    clockItem.StatusID = (int)Status.Working;
                    clockItem.StartedAt = clockItem.StartedAt ?? DateTime.Now;

                    var result = await _apiClient.UpdateClock(clockItem);
                    if (result > 0 && main.ClockingList != null)
                    {
                        var existing = main.ClockingList.FirstOrDefault(i => i.ID == clockItem.ID);
                        if (existing != null)
                        {
                            existing.StatusID = clockItem.StatusID;
                            existing.StartedAt = clockItem.StartedAt;
                        }
                        SaveMainModel(main);
                    }

                    var lastBreak = await _apiClient.GetLastBreakByClockID(clockItem.ID ?? 0);
                    if (lastBreak != null)
                    {
                        lastBreak.EndAt = DateTime.Now;
                        await _apiClient.UpdateClockBreak(lastBreak);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BreakClock processing failed for ID={ID}", ID);
            }

            return RedirectToAction(nameof(Clocking));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBreakClock(ClockingModel dTO)
        {
            var main = GetMainModel();
            if (dTO?.ClockingBreakForm == null) return RedirectToAction(nameof(Clocking));

            main.ClockingBreakForm = dTO.ClockingBreakForm;
            SaveMainModel(main);
            await BreakClock(dTO.ClockingBreakForm.ClockingID ?? 0, (int)Status.Break);
            
            return RedirectToAction(nameof(Clocking));
        }

        [HttpPost]
        public async Task<IActionResult> ClockAllOut()
        {
            var main = GetMainModel();

            if (main.ClockingList != null)
            {
                foreach (var item in main.ClockingList.ToList())
                {
                    await BreakClock(item.ID ?? 0, (int)Status.ClockOut);
                    main?.LabourlinesSelectList?.RemoveAll(s => s.Value == item.RTSID.ToString());
                }

              

                SaveMainModel(main);
            }

            return Json(new { success = true });
        }


        [HttpGet]
        public async Task<IEnumerable<TechnicianDTO>> GetTechniciansPIN()
        {
            var relativePath = _configuration["FileUpload:DirectoryPath"] ?? "Uploads";
            var filterTechnician = new FilterTechnicianDTO { PageNumber = 1, Email = string.Empty, Name = string.Empty, WorkshopId = BranchId };

            var technicians = await _apiClient.GetAllPINTechniciansAsync(filterTechnician);
            foreach (var item in technicians ?? Enumerable.Empty<TechnicianDTO>())
            {
                var fullURL = Path.Combine(relativePath, item.FilePath ?? string.Empty).Replace("\\", "/");
                item.FilePath = fullURL;
            }

            return technicians ?? Enumerable.Empty<TechnicianDTO>();
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> CheckNumericPIN([FromBody] CheckPinRequest request)
        {
            if (request == null || request.Technician == null)
            {
                _logger.LogWarning("CheckNumericPIN called with null request or technician");
                return BadRequest(false);
            }

            try
            {
                var techId = request.Technician.Id;
                if (techId == 0)
                {
                    _logger.LogWarning("CheckNumericPIN called without Technician Id");
                    return Ok(false);
                }

                // Retrieve authoritative technician data from API/repository
                var storedTech = await _apiClient.GetTechnicianByIdAsync(techId);
                if (storedTech == null)
                {
                    _logger.LogWarning("CheckNumericPIN: technician not found. Id={TechId}", techId);
                    return Ok(false);
                }

                var storedPin = storedTech.PIN ?? -1;
                var providedPin = request.PIN;

                var result = storedPin == providedPin;

                _logger.LogInformation("CheckNumericPIN attempt for TechnicianId={TechId} result={Result}", techId, result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CheckNumericPIN failed");
                return StatusCode(500, false);
            }
        }

        [HttpGet]
        [CustomAuthorize(Permissions.TechnicianDashboard.Clocking)]
        public async Task<IActionResult> Clocking(ClockingModel? clockingModel)
        {
            var main = GetMainModel();

            var aClockingModel = new ClockingModel
            {
                ClockingForm = new ClockingDTO { TechnicianID = 0, WIPID = 0, RTSID = 0, TechnicianName = string.Empty },
                ClockingList = new List<ClockingDTO>(),
                Labourlines = new List<CreateWIPServiceDTO>(),
                Reasons = new List<LookupDetailsDTO>(),
                ClockingBreakForm = new ClockingBreakDTO { StartAt = DateTime.Now, EndAt = DateTime.Now }
            };

            if (main == null || main.ClockingForm == null)
            {
                main = aClockingModel;
            }

            try
            {
                var technicians = await _apiClient.GetTechniciansDDL(BranchId);
                var wipSchedules = await _apiClient.GetClockingFilter();
                var reasons = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(5, CompanyId);
                var rtsCodes = await _apiClient.GetAllRTSCodesDDLAsync();

                // Prefer the authoritative API list for the dropdown. If the session contains a previously
                // selected single technician, ensure that technician appears in the list as well so it shows in the dropdown.
                var apiTechnicians = technicians?.ToList() ?? new List<TechnicianDTO>();

                if (main?.Technicians != null)
                {
                    var techList = main.Technicians as IList<TechnicianDTO> ?? main.Technicians.ToList();
                    if (techList.Count == 1)
                    {
                        var selected = techList[0];
                        if (selected != null && !apiTechnicians.Any(t => t.Id == selected.Id))
                        {
                            apiTechnicians.Insert(0, selected);
                        }
                    }
                }
                if ((main.Technicians == null))
                {
                    var defaultTechnicianID = HttpContext.Session.GetInt32("TechnicianID") ?? 0;
                    if (defaultTechnicianID != 0)
                    {
                        var defaultTechnician = await _apiClient.GetTechnicianByIdAsync(defaultTechnicianID);
                        main.Technicians = new List<TechnicianDTO> { defaultTechnician };// defaultTechnician ?? apiTechnicians;

                    }
                    else
                    {
                        main.Technicians = apiTechnicians;
                    }
                }
                main.WIPSSelectList = main.WIPSSelectList ?? new List<SelectListItem>();
                main.LabourlinesSelectList = main.LabourlinesSelectList ?? new List<SelectListItem>();
                main.RTSCodes = rtsCodes ?? Enumerable.Empty<RTSCodeDTO>();
                main.Reasons = reasons ?? new List<LookupDetailsDTO>();

                var clocks = (await _apiClient.GetClocksAsync())?.ToList() ?? new List<ClockingDTO>();
                var clocksBreaks = (await _apiClient.GetAllClocksBreaksDDL())?.ToList() ?? new List<ClockingBreakDTO>();
                var shifts = (await _apiClient.GetAllShiftsAsync())?.ToList() ?? new List<ShiftDTO>();

                main.ClockingList = clocks.Where(i => !i.StatusID.Equals((int)Status.ClockOut) && main.Technicians.Any(t => t.Id == i.TechnicianID)).ToList();
                main.ClockingHistory = clocks.Where(i => i.StatusID.Equals((int)Status.ClockOut) && main.Technicians.Any(t => t.Id == i.TechnicianID)).ToList();
                main.ClockingBreakForm = main.ClockingBreakForm ?? new ClockingBreakDTO();
                foreach (var item in main.ClockingList.ToList())
                {
                    item.TechnicianName = technicians?.FirstOrDefault(t => item.TechnicianID == t.Id)?.PrimaryName;
                    item.WIPName = wipSchedules?.FirstOrDefault(w => w.WIPId == item.WIPID) is var w && w != null ? "WIP - " + w.WIPId : null;
                    item.RTSName = rtsCodes?.FirstOrDefault(r => r.Id == item.RTSID) is var rts && rts != null ? (lang == "en" ? rts.PrimaryDescription : rts.SecondaryDescription) : null;

                    item.StatusName = Enum.TryParse<Status>(item.StatusID.ToString(), out var st) ? st : default;
                    item.LastBreak = clocksBreaks.Where(b => b.ClockingID == item.ID).OrderByDescending(d => d.StartAt).FirstOrDefault();
                    item.ClockingBreaksLogs = clocksBreaks.Where(w => w.ClockingID == item.ID).ToList();
                    item.Technician = technicians?.FirstOrDefault(t => t.Id == item.TechnicianID);

                    foreach(var itemBreak in item.ClockingBreaksLogs)
                    {
                        itemBreak.ReasonString = reasons?.FirstOrDefault(i => i.Id == itemBreak.Reason) is var reason && reason != null ? (lang == "en" ? reason.PrimaryName : reason.SecondaryName) : null;
                    }

                    if (item.Technician != null)
                    {
                        item.TechnicianDefaultShift = shifts.FirstOrDefault(s => s.Id == item.Technician.FK_ShiftId);
                    }

                    if (item.TechnicianDefaultShift != null && item.TechnicianDefaultShift.StartBreakTime != null && item.TechnicianDefaultShift.EndBreakTime != null)
                    {
                        try
                        {
                            var start = item.TechnicianDefaultShift.StartBreakTime.Value;
                            var end = item.TechnicianDefaultShift.EndBreakTime.Value;
                            var dow = item.TechnicianDefaultShift.DOW ?? "*"; // e.g., "MON-FRI"

                            string breakCron = $"{start.Minutes} {start.Hours} * * {dow}";
                            string workingCron = $"{end.Minutes} {end.Hours} * * {dow}";

                            ClockingModel dTO = new ClockingModel { ClockingBreakForm = new ClockingBreakDTO() };
                            dTO.ClockingBreakForm.ClockingID = item.ID;
                            dTO.ClockingBreakForm.Reason = -1;
                            dTO.ClockingBreakForm.Note = "Scheduled Job";
                            dTO.ClockingBreakForm.Hint = "Scheduled Job";

                            _recurringJobManager.AddOrUpdate<TechnicianDashboardController>($"BreakJob_{item.TechnicianID}", controller => controller.EditBreakClock(dTO), breakCron);
                            _recurringJobManager.AddOrUpdate<TechnicianDashboardController>($"WorkingJob_{item.TechnicianID}", controller => controller.EditBreakClock(dTO), workingCron);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error scheduling jobs for TechnicianID {TechnicianID}", item.TechnicianID);
                        }
                    }
                }

                foreach (var item in main.ClockingHistory)
                {
                    item.TechnicianName = technicians?.FirstOrDefault(t => item.TechnicianID == t.Id)?.PrimaryName;
                    item.WIPName = wipSchedules?.FirstOrDefault(w => w.WIPId == item.WIPID) is var w2 && w2 != null ? "WIP - " + w2.WIPId : null;
                    item.RTSName = rtsCodes?.FirstOrDefault(r => r.Id == item.RTSID) is var r2 && r2 != null ? (lang == "en" ? r2.PrimaryDescription : r2.SecondaryDescription) : null;

                    item.StatusName = Enum.TryParse<Status>(item.StatusID.ToString(), out var st2) ? st2 : default;
                    item.Elapsed = item.EndedAt - item.StartedAt;
                    item.LastBreak = clocksBreaks.Where(b => b.ClockingID == item.ID).OrderByDescending(d => d.StartAt).FirstOrDefault();
                    item.ClockingBreaksLogs = clocksBreaks.Where(w => w.ClockingID == item.ID).ToList();
                    foreach (var itemBreak in item.ClockingBreaksLogs)
                    {
                        itemBreak.ReasonString = reasons?.FirstOrDefault(i => i.Id == itemBreak.Reason) is var reason && reason != null ? (lang == "en" ? reason.PrimaryName : reason.SecondaryName) : null;
                    }
                }

                SaveMainModel(main);
                return View(main);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Clocking load failed");
                return View(main ?? aClockingModel);
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.TechnicianDashboard.Clocking)]
        public async Task<IActionResult> Clocking([FromBody] TechnicianDTO request)
        {
            try
            {
                var model = new ClockingModel
                {
                    ClockingForm = new ClockingDTO { TechnicianID = request.Id, WIPID = 0, RTSID = 0, TechnicianName = string.Empty },
                    ClockingBreakForm = new ClockingBreakDTO(),
                    ClockingList = new List<ClockingDTO>(),
                    Labourlines = new List<CreateWIPServiceDTO>(),
                    ClockingHistory = new List<ClockingDTO>()
                };

                model.WIPS = (await _apiClient.GetWIPDDL())?.ToList() ?? new List<WIPDTO>();
                if (request != null)
                    model.Technicians = new List<TechnicianDTO> { request };
                SaveMainModel(model);
                if (model?.Technicians?.Count() == 1 && request != null)
                    await SelectTechnician(request.Id);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Clocking (POST) failed");
                return View(new ClockingModel { ClockingForm = new ClockingDTO(), Technicians = new List<TechnicianDTO>(), Labourlines = new List<CreateWIPServiceDTO>() });
            }
        }

        [HttpPost]
        public IActionResult GetUpdatedElapsed(int id)
        {
            var main = GetMainModel();
            var values = main.ClockingList?.Where(m => m.TechnicianID == id).Select(m => m.Elapsed) ?? Enumerable.Empty<TimeSpan?>();
            return Json(values);
        }

        [HttpPost]
        public async Task<IEnumerable<ClockingBreakDTO>> GetLogs([FromBody] int ID)
        {
            var main = GetMainModel();
            main.LogIndex = ID;
            SaveMainModel(main);
            var result = await _apiClient.GetBreaksByClockID(ID);

            var reasons = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(5, CompanyId);
            foreach (var itemBreak in result)
            {
                itemBreak.ReasonString = reasons?.FirstOrDefault(i => i.Id == itemBreak.Reason) is var reason && reason != null ? (lang == "en" ? reason.PrimaryName : reason.SecondaryName) : null;
            }

            return result ?? Enumerable.Empty<ClockingBreakDTO>();
        }

    }
}
