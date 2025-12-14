// Controllers/AllowedTimesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class AllowedTimeController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly string lang;

        public AllowedTimeController(WorkshopApiClient apiClient, VehicleApiClient vehicleApiClient, 
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _apiClient = apiClient;
            _vehicleApiClient = vehicleApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;

        }

        [CustomAuthorize(Permissions.AllowedTime.View)]
        public async Task<IActionResult> Index([FromQuery] AllowedTimeFilterDTO? filter)
        {
            filter ??= new AllowedTimeFilterDTO();
            filter.Page = filter.Page ?? 1;
            filter.PageSize = filter.PageSize ?? 25;

            var data = await _apiClient.AllowedTimeGetAllAsync(filter);

            await FillAllowedTimesProperties(data);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_AllowedTimesList", data);
            }

            ViewBag.Makes = await GetMakes();
            ViewBag.RTSCodes = await GetRTSCodeDTOs();

            return View(data);
        }

        [CustomAuthorize(Permissions.AllowedTime.Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                AllowedTimeDTO? allowedTime = null;
                if (id != 0)
                {
                    allowedTime = await _apiClient.AllowedTimeGetByIdAsync(id);
                    if (allowedTime == null)
                    {
                        //return Json(new { success = false, message = "Allowed time not found" });
                        return RedirectToAction("Index");
                    }
                }

                allowedTime ??= new AllowedTimeDTO();

                //return Json(new { success = true, data = allowedTime });

                ViewBag.Makes = await GetMakes();
                ViewBag.RTSCodes = await GetRTSCodeDTOs();

                return PartialView("Edit", allowedTime);
            }
            catch (Exception ex)
            {
                //return Json(new { success = false, message = ex.Message });
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Permissions.AllowedTime.Create)]
        public async Task<IActionResult> Edit(AllowedTimeDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = "Invalid data", errors });
                }


                int result;

                if (dto.Id == 0)
                {
                    var createDto = MapToCreateDto(dto);
                    createDto.CreatedBy = UserId;
                    result = await _apiClient.AllowedTimeCreateAsync(createDto);
                }
                else
                {
                    var updateDto = MapToUpdateDto(dto);
                    updateDto.UpdatedBy = UserId;
                    var success = (await _apiClient.AllowedTimeUpdateAsync(updateDto)).IsSuccess;
                    result = success ? dto.Id : 0;
                }

                if (result > 0)
                {
                    return Json(new { success = true, message = "Allowed time saved successfully" });
                }
                else
                {
                    return Json(new { success = false, message = "Failed to save allowed time" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.AllowedTime.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _apiClient.AllowedTimeDeleteAsync(id);
                return Json(new { success = result.IsSuccess, message = result.IsSuccess ? "Allowed time deleted successfully" : "Failed to delete allowed time" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private CreateAllowedTimeDTO MapToCreateDto(AllowedTimeDTO dto)
        {
            return new CreateAllowedTimeDTO
            {
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                RTSCode = dto.RTSCode,
                AllowedHours = dto.AllowedHours,
                SupervisorOverride = dto.SupervisorOverride
            };
        }

        private UpdateAllowedTimeDTO MapToUpdateDto(AllowedTimeDTO dto)
        {
            return new UpdateAllowedTimeDTO
            {
                Id = dto.Id,
                Make = dto.Make,
                Model = dto.Model,
                Year = dto.Year,
                RTSCode = dto.RTSCode,
                AllowedHours = dto.AllowedHours,
                SupervisorOverride = dto.SupervisorOverride
            };
        }


        #region Data methods
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
                return make != null ? lang =="en"? make.ManufacturerPrimaryName :make.ManufacturerSecondaryName : "N/A";
            };

            foreach (var item in items)
            {
                item.MakeName = makes != null ? makes.Where(m => m.Id == item.Make).Select(a => lang =="en"? a.ManufacturerPrimaryName : a.ManufacturerSecondaryName).FirstOrDefault() : "N/A";
                
                if (!keyValuePairs.ContainsKey(item.Make))
                {
                    var models = await _vehicleApiClient.GetAllVehicleModel(item.Make, GetCurrentLanguage());
                    keyValuePairs[item.Make] = models;
                }

                item.ModelName = keyValuePairs[item.Make] != null ? keyValuePairs[item.Make]?.Where(m => m.ManufacturerId == item.Make).Select(a => lang == "en" ? a.VehicleModelPrimaryName : a.VehicleModelSecondaryName).FirstOrDefault()  : "N/A";
                item.RTSCode_Code = RTSCodes != null ? RTSCodes.Where(r => r.Id == item.RTSCode)?.FirstOrDefault()?.Code : "N/A";
            }
        }

        private async Task<List<SelectListItem>> GetMakes()
        {

            var makes = await _vehicleApiClient.GetAllManufacturers(GetCurrentLanguage());

            return makes.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = lang == "en" ?  m.ManufacturerPrimaryName : m.ManufacturerSecondaryName
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
                Text = lang == "en"? r.PrimaryName : r.SecondaryName
            }).ToList();
        }

        private string GetCurrentLanguage()
        {
            return lang;
        }
        #endregion
    }
}