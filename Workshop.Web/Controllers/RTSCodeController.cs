using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class RTSCodeController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public readonly string lang;
        public RTSCodeController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, IConfiguration configuration, 
            IWebHostEnvironment env, VehicleApiClient vehicleApiClient) : base(configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            _configuration = configuration;
            _env = env;
            _vehicleApiClient = vehicleApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.RTSCode.View)]
        public async Task<IActionResult> Index([FromQuery] FilterRTSCodeDTO? oFilterRTSCodeDTO)
        {
            oFilterRTSCodeDTO ??= new FilterRTSCodeDTO();
            oFilterRTSCodeDTO.PageNumber = oFilterRTSCodeDTO.PageNumber ?? 1;

            var data = await _apiClient.GetAllRTSCodesAsync(oFilterRTSCodeDTO);

            RTSCodeWithAllowedTimeDTO rTSCodeWithAllowedTimeDTO = new RTSCodeWithAllowedTimeDTO();
            rTSCodeWithAllowedTimeDTO.RTSCodes = data;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_RTSCodeList", rTSCodeWithAllowedTimeDTO);
            }

            var filter = new AllowedTimeFilterDTO();
            filter.Page = filter.Page ?? 1;
            filter.PageSize = filter.PageSize ?? 25;

            var aData = await _apiClient.AllowedTimeGetAllAsync(filter);

            ViewBag.Makes = await GetMakes();
            ViewBag.RTSCodes = await GetRTSCodeDTOs();
            ViewBag.VehiclesModels = await GetVehicles();//get all vechiles
            ViewBag.VehiclesClass = await GetRTSCodeDTOs();

            rTSCodeWithAllowedTimeDTO.AllowedTimes = aData;

            return View(rTSCodeWithAllowedTimeDTO);
        }

        [CustomAuthorize(Permissions.RTSCode.Create)]
        public async Task<IActionResult> Edit(int? Id, int aId)
        {
            var oRTSCodeDTO = new RTSCodeDTO();
            if (Id != null)
            {
                oRTSCodeDTO = await _apiClient.GetRTSCodeByIdAsync((int)Id);
            }

            if (oRTSCodeDTO == null) return NotFound();

            var dto = new RTSCodeDTO
            {
                Code = oRTSCodeDTO.Code,
                PrimaryDescription = oRTSCodeDTO.PrimaryDescription,
                SecondaryDescription = oRTSCodeDTO.SecondaryDescription,
                PrimaryName = oRTSCodeDTO.PrimaryName,
                SecondaryName = oRTSCodeDTO.SecondaryName,
                FK_SkillId = oRTSCodeDTO.FK_SkillId,
                CompanyId = oRTSCodeDTO.CompanyId,
                FK_CategoryId = oRTSCodeDTO.FK_CategoryId,
                StandardHours = oRTSCodeDTO.StandardHours,
                EffectiveDate = oRTSCodeDTO.EffectiveDate,
                IsActive = true,
                Notes = oRTSCodeDTO.Notes,
                FranchiseIds = oRTSCodeDTO.FranchiseIds,
                VehicleClassIds = oRTSCodeDTO.VehicleClassIds,
                Id = Id ?? 0,
                DefaultRate = oRTSCodeDTO.DefaultRate,

            };
            var skills = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(1, CompanyId);
            ViewBag.Skills = skills.Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();

            var categories = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(7, CompanyId);
            ViewBag.category = categories.Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();

            var vehicles = await _vehicleApiClient.GetAllVehicleClass(lang);
            ViewBag.Vehicle = vehicles.Select(t => new SelectListItem { Text = lang == "en" ? t.VehicleClassPrimaryName : t.VehicleClassSecondaryName, Value = t.Id.ToString() }).ToList();
            ViewBag.vehiclesType = vehicles.Where(t => dto.VehicleClassIds != null && dto.VehicleClassIds.Contains(t.Id)).ToList();

            var makes = await _vehicleApiClient.GetAllManufacturers(lang);
            ViewBag.make = makes.Select(t => new SelectListItem { Text = lang == "en" ? t.ManufacturerPrimaryName : t.ManufacturerSecondaryName, Value = t.Id.ToString() }).ToList();
            ViewBag.Makes = makes.Where(t => dto.FranchiseIds != null && dto.FranchiseIds.Contains(t.Id)).ToList();

            RTSCodeWithAllowedTimeDTO rTSCodeWithAllowedTimeDTO = new RTSCodeWithAllowedTimeDTO();
            rTSCodeWithAllowedTimeDTO.RTSCode = dto;
            rTSCodeWithAllowedTimeDTO.AllowedTime = new AllowedTimeDTO();

            try
            {
                Id ??= 0;
                if (Id != 0)
                {

                    AllowedTimeFilterDTO allow = new AllowedTimeFilterDTO();
                    allow.RTSCode = Id;
                    rTSCodeWithAllowedTimeDTO.AllowedTimes = await _apiClient.AllowedTimeGetAllAsync(allow);
                    if (rTSCodeWithAllowedTimeDTO.AllowedTimes.Count > 0)
                    {
                        rTSCodeWithAllowedTimeDTO.AllowedTime = rTSCodeWithAllowedTimeDTO.AllowedTimes[0];
                        ViewBag.AllowedTimes = rTSCodeWithAllowedTimeDTO.AllowedTimes;

                    }
                    else
                    {
                        rTSCodeWithAllowedTimeDTO.AllowedTime = new AllowedTimeDTO();
                    }

                }

                ViewBag.Makes = await GetMakes();
                ViewBag.RTSCodes = await GetRTSCodeDTOs();

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
            ViewBag.VehiclesModels = await GetVehicles();
            return View(rTSCodeWithAllowedTimeDTO);
        }


        [HttpPost]
        [CustomAuthorize(Permissions.RTSCode.Create)]
        public async Task<IActionResult> Edit(RTSCodeWithAllowedTimeDTO dto, string AllowedTimesJson)
        {

            var allowedTimesJ = JsonConvert.DeserializeObject<List<AllowedTimeDTO>>(AllowedTimesJson) ?? new List<AllowedTimeDTO>();
            int? rtsId;
            if (dto.RTSCode.Id == 0)
            {
                var createDto = MapToCreateDto(dto.RTSCode);
                rtsId = await _apiClient.AddRTSCodeAsync(createDto);
                if (rtsId != null && rtsId != 0)
                {

                    foreach (var at in allowedTimesJ)
                    {
                        if (dto.AllowedTime == null)
                            dto.AllowedTime = new AllowedTimeDTO();
                        dto.AllowedTime.RTSCode = rtsId ?? 0;

                        CreateAllowedTimeDTO createAllowedTimeDTO = new CreateAllowedTimeDTO();
                        createAllowedTimeDTO.Make = at.Make;
                        createAllowedTimeDTO.Model = at.Model;
                        createAllowedTimeDTO.AllowedHours = at.AllowedHours;
                        createAllowedTimeDTO.Year = at.Year;
                        createAllowedTimeDTO.RTSCode = rtsId ?? 0;
                        createAllowedTimeDTO.VehicleClass = at.VehicleClass;
                        createAllowedTimeDTO.CreatedBy = UserId;

                        await _apiClient.AllowedTimeCreateAsync(createAllowedTimeDTO);
                    }
                }

                if (!rtsId.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Failed to create rts.");
                    return View(dto);
                }
            }
            else
            {
                var updateDto = MapToUpdateDto(dto.RTSCode);
                updateDto.UpdatedBy = UserId;
                var success = await _apiClient.UpdateRTSCodeAsync(updateDto);
                rtsId = dto.RTSCode.Id;

                foreach (var at in allowedTimesJ)
                {
                    if (at.Id > 0)
                    {
                        UpdateAllowedTimeDTO updateAllowedTimeDTO = new UpdateAllowedTimeDTO();
                        updateAllowedTimeDTO.Make = at.Make;
                        updateAllowedTimeDTO.Model = at.Model;
                        updateAllowedTimeDTO.AllowedHours = at.AllowedHours;
                        updateAllowedTimeDTO.Year = at.Year;
                        updateAllowedTimeDTO.RTSCode = rtsId ?? 0;
                        updateAllowedTimeDTO.UpdatedBy = UserId;
                        updateAllowedTimeDTO.Id = at.Id;
                        updateAllowedTimeDTO.VehicleClass = at.VehicleClass;
                        updateAllowedTimeDTO.UpdatedBy = UserId;
                        await _apiClient.AllowedTimeUpdateAsync(updateAllowedTimeDTO);
                    }

                    else
                    {
                        CreateAllowedTimeDTO cAllowedTimeDTO = new CreateAllowedTimeDTO();
                        cAllowedTimeDTO.Make = at.Make;
                        cAllowedTimeDTO.Model = at.Model;
                        cAllowedTimeDTO.AllowedHours = at.AllowedHours;
                        cAllowedTimeDTO.Year = at.Year;
                        cAllowedTimeDTO.RTSCode = rtsId ?? 0;
                        cAllowedTimeDTO.VehicleClass = at.VehicleClass;
                        cAllowedTimeDTO.CreatedBy = UserId;

                        await _apiClient.AllowedTimeCreateAsync(cAllowedTimeDTO);
                    }
                }

                AllowedTimeFilterDTO allow = new AllowedTimeFilterDTO();
                allow.RTSCode = dto.RTSCode.Id;
                var previousAllowedTimes = await _apiClient.AllowedTimeGetAllAsync(allow);
                if (previousAllowedTimes.Count > allowedTimesJ.Count)
                {
                    var records = previousAllowedTimes
                    .Where(e => !allowedTimesJ.Any(a => a.Id == e.Id))
                    .ToList();

                    foreach (var item in records)
                    {

                        await _apiClient.AllowedTimeDeleteAsync(item.Id);
                    }
                }

            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<List<AllowedTimeListItemDTO>> GetAllowedTimes(int rtsCodeId)
        {

            var allowedTimes = await _apiClient.AllowedTimeGetAllAsync(new AllowedTimeFilterDTO { RTSCode = rtsCodeId });
            return allowedTimes;
        }

        [HttpGet]
        public async Task<IEnumerable<VehicleModel>> FillVehicleModel(int id)
        {

            var result = (await _vehicleApiClient.GetAllVehicleModel(id, lang));
            return result;

        }

        private CreateRTSCodeDTO MapToCreateDto(RTSCodeDTO dto)
        {
            return new CreateRTSCodeDTO
            {
                Code = dto.Code,
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                PrimaryDescription = dto.PrimaryDescription,
                SecondaryDescription = dto.SecondaryDescription,
                FK_SkillId = dto.FK_SkillId,
                StandardHours = dto.StandardHours,
                FK_CategoryId = dto.FK_CategoryId,
                Notes = dto.Notes,
                IsActive = dto.IsActive,
                EffectiveDate = dto.EffectiveDate,
                FranchiseIds = dto.FranchiseIds,
                VehicleClassIds = dto.VehicleClassIds,
                CompanyId = CompanyId,
                DefaultRate = dto.DefaultRate,

            };
        }

        private UpdateRTSCodeDTO MapToUpdateDto(RTSCodeDTO dto)
        {
            return new UpdateRTSCodeDTO
            {
                Id = dto.Id,
                Code = dto.Code,
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                PrimaryDescription = dto.PrimaryDescription,
                SecondaryDescription = dto.SecondaryDescription,
                FK_SkillId = dto.FK_SkillId,
                StandardHours = dto.StandardHours,
                FK_CategoryId = dto.FK_CategoryId,
                CompanyId = CompanyId,
                Notes = dto.Notes,
                IsActive = dto.IsActive,
                EffectiveDate = dto.EffectiveDate,
                FranchiseIds = dto.FranchiseIds,
                VehicleClassIds = dto.VehicleClassIds,
                DefaultRate = dto.DefaultRate

            };
        }

        private async Task<List<SelectListItem>> GetMakes()
        {

            var makes = await _vehicleApiClient.GetAllManufacturers(GetCurrentLanguage());

            return makes.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = lang == "en" ? m.ManufacturerPrimaryName : m.ManufacturerSecondaryName

            }).ToList();
        }
        private async Task<List<VehicleModel>> GetVehicles()
        {
            var models = await _vehicleApiClient.GetAllVehicleModel(0, GetCurrentLanguage());

            return models.Select(m => new VehicleModel
            {
                Id = m.Id, // keep numeric for matching
                Name = lang == "en" ? m.VehicleModelPrimaryName : m.VehicleModelSecondaryName,
                ManufacturerId = m.ManufacturerId // <-- not m.Id
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetRTSCodeDTOs()
        {
            var RTSCodes = await _apiClient.GetAllRTSCodesAsync(new FilterRTSCodeDTO { PageNumber = 1 });

            if (RTSCodes == null)
            {
                return new List<SelectListItem>();
            }

            return RTSCodes.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = lang == "en" ? r.PrimaryName : r.SecondaryName
            }).ToList();
        }

        private async Task FillAllowedTimesProperties(List<AllowedTimeListItemDTO> items)
        {

            if (items == null || items.Count == 0)
            {
                return;
            }

            var makes = await _vehicleApiClient.GetAllManufacturers(GetCurrentLanguage());
            var RTSCodes = await _apiClient.GetAllRTSCodesAsync(new FilterRTSCodeDTO { PageNumber = 1 });

            Dictionary<int, List<VehicleModel>> keyValuePairs = new Dictionary<int, List<VehicleModel>>();
            Func<int, Task<string>> GetMakeNameById = async (int makeId) =>
            {
                var make = makes.FirstOrDefault(m => m.Id == makeId);
                return make != null ? lang == "en" ? make.ManufacturerPrimaryName : make.ManufacturerSecondaryName : "N/A";
            };

            foreach (var item in items)
            {
                item.MakeName = makes != null ? makes.Where(m => m.Id == item.Make).Select(a => lang == "en" ? a.ManufacturerPrimaryName : a.ManufacturerSecondaryName).FirstOrDefault() : "N/A";

                if (!keyValuePairs.ContainsKey(item.Make))
                {
                    var models = await _vehicleApiClient.GetAllVehicleModel(item.Make, GetCurrentLanguage());
                    keyValuePairs[item.Make] = models;
                }

                item.ModelName = keyValuePairs[item.Make] != null ? keyValuePairs[item.Make]?.Where(m => m.ManufacturerId == item.Make).Select(a => lang == "en" ? a.VehicleModelPrimaryName : a.VehicleModelSecondaryName).FirstOrDefault() : "N/A";
                item.RTSCode_Code = RTSCodes != null ? RTSCodes.Where(r => r.Id == item.RTSCode)?.FirstOrDefault()?.Code : "N/A";
            }
        }

        public async Task<int> DeleteCode(int Id)
        {
            DeleteRTSCodeDTO? codeDTO = new DeleteRTSCodeDTO();
            codeDTO.Id = Id;
            codeDTO.IsActive = false;
            codeDTO.UpdatedBy = UserId;

            return await _apiClient.DeleteRTSCodeAsync(codeDTO);

        }

        private string GetCurrentLanguage()
        {
            return lang;
        }

    }
}
