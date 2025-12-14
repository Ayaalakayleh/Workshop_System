using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class WorkScheduleDefinitionController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public readonly string lang;

        public WorkScheduleDefinitionController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, 
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            _configuration = configuration;
            _env = env;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.WorkSchedule.View)]
        public async Task<IActionResult> Index([FromQuery] FilterTechnicianWorkScheduleDTO? oFilter)
        {
           
            oFilter ??= new FilterTechnicianWorkScheduleDTO();
            oFilter.WorkshopId = BranchId;
            oFilter.PageNumber = oFilter.PageNumber ?? 1;
            oFilter.lang = lang;
            var data = await _apiClient.GetAllTechnicianWorkSchedulesAsync(oFilter);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_TechnicianWorkScheduleList", data);
            }
            return View(data);
        }

        [CustomAuthorize(Permissions.WorkSchedule.Edit)]
        public async Task<IActionResult> Edit(int? id)
        {
           
            var lang = "en";
            FilterTechnicianDTO oFilterTechnicianDTO = new FilterTechnicianDTO
            {
                WorkshopId = BranchId,
                PageNumber = 1 
            };
            var technicians = await _apiClient.GetAllTechniciansAsync(oFilterTechnicianDTO);
            var allWorkSchedules = await _apiClient.GetTechniciansFromWorkSchedulesAsync();

            //Compare
            var assignedIds = new HashSet<int>(allWorkSchedules);
            var unassignedTechnicians = technicians.Where(t => !assignedIds.Contains(t.Id)).ToList();
            ViewBag.Technicians = unassignedTechnicians.Select(t => new SelectListItem { Text = t.PrimaryName, Value = t.Id.ToString() }).ToList();

            var WorkSchedule = new TechnicianWorkScheduleDTO();
            if (id != null)
            {
                WorkSchedule = await _apiClient.GetTechnicianWorkScheduleByIdAsync((int)id, lang);
            }

            if (WorkSchedule == null) return NotFound();
            var dto = new TechnicianWorkScheduleDTO
            {
                Name = WorkSchedule.Name,
                FK_TechnicianId = WorkSchedule.FK_TechnicianId,
                WorkshopId = WorkSchedule.WorkshopId,
                WorkingDateFrom = WorkSchedule.WorkingDateFrom,
                WorkingDateTo = WorkSchedule.WorkingDateTo,
                WorkingTimeFrom = WorkSchedule.WorkingTimeFrom,
                WorkingTimeTo = WorkSchedule.WorkingTimeTo,
                FK_SlotMinutes_Id = WorkSchedule.FK_SlotMinutes_Id
            };

            var slot = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(3,CompanyId);
            ViewBag.SlotMinutes = slot.Select(t => new SelectListItem { Text = lang=="en"?t.PrimaryName:t.SecondaryName, Value = t.Id.ToString() }).ToList();
            ViewBag.SelectedDate = new DateTime(2025, 8, 10);
            return View(dto);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.WorkSchedule.Edit)]
        public async Task<IActionResult> Edit(TechnicianWorkScheduleDTO dto)
        {
            dto.WorkshopId = BranchId;
            int? technicianId;

            if (dto.Id == 0)
            {
                var ids = string.Join(",", dto.TechnicianIds_List);
                dto.TechnicianIds = ids;
                dto.CreatedBy = UserId; // need fix it later

                var createDto = MapToCreateDto(dto);
                technicianId = await _apiClient.AddTechnicianWorkScheduleAsync(createDto);

            }
            else
            {
                dto.UpdatedBy = UserId;
                var updateDto = MapToUpdateDto(dto);
                var success = await _apiClient.UpdateTechnicianWorkScheduleAsync(updateDto);

                technicianId = dto.Id;
            }

            return RedirectToAction(nameof(Index));
        }

        private CreateTechnicianWorkScheduleDTO MapToCreateDto(TechnicianWorkScheduleDTO dto)
        {
            return new CreateTechnicianWorkScheduleDTO
            {
                TechnicianIds = dto.TechnicianIds,
                WorkshopId = dto.WorkshopId,
                WorkingDateFrom = dto.WorkingDateFrom,
                WorkingDateTo = dto.WorkingDateTo,
                WorkingTimeFrom = dto.WorkingTimeFrom,
                WorkingTimeTo = dto.WorkingTimeTo,
                FK_SlotMinutes_Id = dto.FK_SlotMinutes_Id,
                CreatedBy = dto.CreatedBy
            };
        }

        private UpdateTechnicianWorkScheduleDTO MapToUpdateDto(TechnicianWorkScheduleDTO dto)
        {
            return new UpdateTechnicianWorkScheduleDTO
            {
                Id = dto.Id,
                FK_TechnicianId = dto.FK_TechnicianId,
                TechnicianIds = "",
                WorkshopId = dto.WorkshopId,
                WorkingDateFrom = dto.WorkingDateFrom,
                WorkingDateTo = dto.WorkingDateTo,
                WorkingTimeFrom = dto.WorkingTimeFrom,
                WorkingTimeTo = dto.WorkingTimeTo,
                FK_SlotMinutes_Id = dto.FK_SlotMinutes_Id,
                UpdatedBy = dto.UpdatedBy
            };
        }
    }
}
