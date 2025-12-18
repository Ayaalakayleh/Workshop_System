using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Core.DTOs;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class ShiftsController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly IConfiguration _configuration;
        public readonly string lang;
        public ShiftsController(WorkshopApiClient apiClient, IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _apiClient = apiClient;
            _configuration = configuration;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.Shifts.View)]
        public async Task<IActionResult> Index([FromQuery] ShiftFilterDTO filter)
        {
            filter ??= new ShiftFilterDTO();
            filter.Page = filter.Page ?? 1;
            filter.PageSize = filter.PageSize ?? 25;

            if (filter.CompanyId <= 0)
            {
                filter.CompanyId = GetCompanyId();
            }

            var data = await _apiClient.ShiftGetAllAsync(filter);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ShiftsList", data);
            }

            return View(filter);
        }

        // GET: Shifts/GetShift/5 - For modal editing
        [CustomAuthorize(Permissions.Shifts.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var shift = await _apiClient.ShiftGetByIdAsync(id);
                if (shift == null)
                {
                    return Json(new { success = false, message = "Shift not found" });
                }

                return Json(new { success = true, data = shift });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Shifts/Save - For modal save (both create and update)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Permissions.Shifts.Edit)]
        public async Task<IActionResult> Edit(ShiftDTO dto)
        {
            try
            {
                dto.UpdateDOW();
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Invalid data", errors });
                }

                var userId = GetCurrentUserId();
                dto.CompanyId = GetCompanyId();

                int result;

                if (dto.Id == 0)
                {
                    var createDto = MapToCreateDto(dto);
                    createDto.CreatedBy = userId;
                    result = await _apiClient.ShiftCreateAsync(createDto);
                }
                else
                {
                    var updateDto = MapToUpdateDto(dto);
                    updateDto.UpdatedBy = userId;
                    var success = (await _apiClient.ShiftUpdateAsync(updateDto)).IsSuccess;
                    result = success ? dto.Id : 0;
                }

                if (result > 0)
                {
                    return Json(new { success = true, message = "Shift saved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save shift" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.Shifts.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _apiClient.ShiftDeleteAsync(id);
                return Json(new { success = result, message = result.IsSuccess ? "Shift deleted successfully" : "Failed to delete shift" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private CreateShiftDTO MapToCreateDto(ShiftDTO dto)
        {
            return new CreateShiftDTO
            {
                Code = dto.Code,
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                Short = dto.Short,
                Color = dto.Color,
                SpanMidnight = dto.SpanMidnight,
                Sun = dto.Sun,
                Mon = dto.Mon,
                Tue = dto.Tue,
                Wed = dto.Wed,
                Thu = dto.Thu,
                Fri = dto.Fri,
                Sat = dto.Sat,
                DOW = dto.DOW,
                CompanyId = dto.CompanyId,
                WorkingFromTime = dto.WorkingFromTime,
                WorkingToTime = dto.WorkingToTime,
                BreakFromTime = dto.BreakFromTime,
                BreakToTime = dto.BreakToTime
                
            };
        }

        private UpdateShiftDTO MapToUpdateDto(ShiftDTO dto)
        {
            return new UpdateShiftDTO
            {
                Id = dto.Id,
                Code = dto.Code,
                PrimaryName = dto.PrimaryName,
                SecondaryName = dto.SecondaryName,
                Short = dto.Short,
                Color = dto.Color,
                SpanMidnight = dto.SpanMidnight,
                Sun = dto.Sun,
                Mon = dto.Mon,
                Tue = dto.Tue,
                Wed = dto.Wed,
                Thu = dto.Thu,
                Fri = dto.Fri,
                Sat = dto.Sat,
                DOW = dto.DOW,
                CompanyId = dto.CompanyId,
                WorkingFromTime = dto.WorkingFromTime,
                WorkingToTime = dto.WorkingToTime,
                BreakFromTime = dto.BreakFromTime,
                BreakToTime = dto.BreakToTime

            };
        }

        private int GetCompanyId()
        {
            return CompanyId;
        }

        private int GetCurrentUserId()
        {
            return _configuration.GetValue<int>("DefaultUserId", UserId);
        }
    }
}