using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using NPOI.HPSF;
using Workshop.Core.DTOs;
using Workshop.Infrastructure;
using Workshop.Web.Models;
using Workshop.Web.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class TechnicianProfileDefinitionController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpApiClient;
        //private readonly IConfiguration _configuration;
        //private readonly IWebHostEnvironment _env;
        public readonly string lang;
        public TechnicianProfileDefinitionController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            //_configuration = configuration;
            //_env = env;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }


        [CustomAuthorize(Permissions.TechnicianProfile.View)]
        public async Task<IActionResult> Index([FromQuery] FilterTechnicianDTO? oFilterTechnicianDTO)
        {
            oFilterTechnicianDTO ??= new FilterTechnicianDTO();
            oFilterTechnicianDTO.WorkshopId = BranchId;
            oFilterTechnicianDTO.PageNumber = oFilterTechnicianDTO.PageNumber ?? 1;
            var technicians = await _apiClient.GetAllTechniciansAsync(oFilterTechnicianDTO);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_TechniciansProfileList", technicians);
            }
            return View(technicians);
        }

        [CustomAuthorize(Permissions.TechnicianProfile.Edit)]
        public async Task<IActionResult> Edit(int? id)
        {
            // var lang = "ar";
            var technician = new TechnicianDTO();
            if (id != null)
            {
                var relativePath = base._configuration["FileUpload:DirectoryPath"] ?? "Uploads";
                technician = await _apiClient.GetTechnicianByIdAsync((int)id);
                var fullURL = Path.Combine(relativePath, technician.FilePath ?? string.Empty).Replace("\\", "/");
                technician.FilePath = fullURL;
            }

            if (technician == null) return NotFound();
            var dto = new TechnicianDTO
            {
                PrimaryName = technician.PrimaryName,
                SecondaryName = technician.SecondaryName,
                Email = technician.Email,
                //Phone = technician.Phone,
                //PrimaryAddress = technician.PrimaryAddress,
                //SecondaryAddress = technician.SecondaryAddress,
                //BirthDate = technician.BirthDate,
                WorkshopId = technician.WorkshopId,
                FK_SkillId = technician.FK_SkillId,
                Code = technician.Code,
                //HourCost = technician.HourCost,
                IsResigned = technician.IsResigned,
                FordPID = technician.FordPID,
                PIN = technician.PIN,
                ResignedDate = technician.ResignedDate,
                FK_ShiftId = technician.FK_ShiftId,
                IsActive = technician.IsActive,
                Id = technician.Id,
                FilePath = technician.FilePath,
                FileName = technician.FileName,
                Type = technician.Type,
                Teams = technician.Teams,


            };

            ViewBag.departments = await _erpApiClient.GetAllDepartmentAsync(CompanyId, "en");
            /*
            var JobTypeList = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(1, CompanyId);
            ViewBag.JobTypes = JobTypeList.Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();
            */
            var SkillsList = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(1, CompanyId);
            ViewBag.Skill = SkillsList.Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();
            ViewBag.SkillsValue = SkillsList.Where(t => dto.FK_SkillId != null && dto.FK_SkillId.Contains(t.Id)).ToList();

            var TeamsList = await _apiClient.GetAllTeamsDDLAsync();
            ViewBag.Teams = TeamsList.Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();
            ViewBag.TeamsValue = TeamsList.Where(t => dto.Teams != null && dto.Teams.Contains(t.Id)).ToList();

            var ShiftsList = await _apiClient.GetAllShiftsAsync();
            ViewBag.Shift = ShiftsList.Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName + " " + "(" + t.WorkingFromTime.Value.ToString(@"hh\:mm") + "-" + t.WorkingToTime.Value.ToString(@"hh\:mm") + ")" : t.SecondaryName + " " + "(" + t.WorkingFromTime.Value.ToString(@"hh\:mm") + "-" + t.WorkingToTime.Value.ToString(@"hh\:mm") + ")", Value = t.Id.ToString() }).ToList();
            ViewBag.ShiftValue = ShiftsList.Where(t => dto.FK_ShiftId != null && dto.FK_ShiftId.Equals(t.Id)).ToList();


            return View(dto);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.TechnicianProfile.Edit)]
        public async Task<IActionResult> Edit(TechnicianDTO dto, IFormFile TechnicianPhoto)
        {
            /*  if (!ModelState.IsValid)
                  return View(dto);
            */
            dto.WorkshopId = BranchId;
            int? technicianId;

            if (TechnicianPhoto != null && TechnicianPhoto.Length > 0)
            {
                if (TechnicianPhoto.FileName != "blob")
                {
                    var relativePath = base._configuration["FileUpload:DirectoryPath"];
                    var guid = Guid.NewGuid().ToString();

                    // Combine with wwwroot to get the absolute path
                    var folderPath = Path.Combine(base._env.WebRootPath, relativePath, "TechnicianPhoto", guid);

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var filename = $"{DateTime.Now.Ticks}{Path.GetExtension(TechnicianPhoto.FileName)}";
                    var fullPath = Path.Combine(folderPath, filename);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await TechnicianPhoto.CopyToAsync(stream);
                    }

                    // Save only the relative part for later public access
                    dto.FilePath = Path.Combine("TechnicianPhoto", guid).Replace("\\", "/");
                    dto.FileName = filename;
                }
            }else if (TechnicianPhoto == null && dto.Id > 0)
            {
                dto.FilePath = null;
                dto.FileName = null;
            }

            if (dto.Id == 0)
            {
                var createDto = MapToCreateDto(dto);
                technicianId = await _apiClient.AddTechnicianAsync(createDto);

                if (!technicianId.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Failed to create technician.");
                    return View(dto);
                }
            }
            else
            {
                var updateDto = MapToUpdateDto(dto);
                var success = await _apiClient.UpdateTechnicianAsync(updateDto);

                technicianId = dto.Id;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [CustomAuthorize(Permissions.TechnicianProfile.Delete)]
        public ActionResult Delete(int Id)
        {
            try
            {
                DeleteDTechnicianDto oDeleteDTechnicianDto = new DeleteDTechnicianDto
                {
                    Id = Id,
                    UpdatedBy = UserId  //SessionManager.GetSessionUserInfo.UserID;
                };

                var result = _apiClient.DeleteTechnicianAsync(oDeleteDTechnicianDto);
                if (result == null)
                {
                    return Json(false);
                }
                else
                {
                    return Json(true);
                }
            }
            catch (Exception ex)
            {
                return Json(false);
                throw ex;
            }
        }


        private CreateDTechnicianDto MapToCreateDto(TechnicianDTO dto)
        {
            return new CreateDTechnicianDto
            {
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                Email = dto.Email,
                //Phone = dto.Phone,
                //PrimaryAddress = dto.PrimaryAddress,
                //SecondaryAddress = dto.SecondaryAddress,
                //BirthDate = dto.BirthDate,
                WorkshopId = dto.WorkshopId,
                FilePath = dto.FilePath,
                FileName = dto.FileName,
                FK_SkillId = dto.FK_SkillId,
                Teams = dto.Teams,
                Code = dto.Code,
                //HourCost = dto.HourCost,
                FordPID = dto.FordPID,
                PIN = dto.PIN,
                IsResigned = dto.IsResigned,
                ResignedDate = dto.ResignedDate,
                FK_ShiftId = dto.FK_ShiftId,
                IsActive = dto.IsActive,


            };
        }

        private UpdateDTechnicianDto MapToUpdateDto(TechnicianDTO dto)
        {
            return new UpdateDTechnicianDto
            {
                Id = dto.Id,
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                Email = dto.Email,
                //Phone = dto.Phone,
                //PrimaryAddress = dto.PrimaryAddress,
                //SecondaryAddress = dto.SecondaryAddress,
                //BirthDate = dto.BirthDate,
                WorkshopId = dto.WorkshopId,
                FilePath = dto.FilePath,
                FileName = dto.FileName,
                FK_SkillId = dto.FK_SkillId,
                IsActive = dto.IsActive,
                Teams = dto.Teams,
                //HourCost = dto.HourCost,
                FordPID = dto.FordPID,
                PIN = dto.PIN,
                IsResigned = dto.IsResigned,
                ResignedDate = dto.ResignedDate,
                FK_ShiftId = dto.FK_ShiftId,
                Code = dto.Code,
            };
        }

    }
}
