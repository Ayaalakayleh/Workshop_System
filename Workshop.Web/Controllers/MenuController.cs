using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Domain.Entities;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]

    public class MenuController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly InventoryApiClient _inventoryApiClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly string lang;
        public MenuController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, InventoryApiClient inventoryApiClient,
            IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            _inventoryApiClient = inventoryApiClient;
            _configuration = configuration;
            _env = env;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.Menu.View)]
        public async Task<IActionResult> Index([FromQuery] FilterMenuDTO oFilterMenuDTO)
        {
            oFilterMenuDTO ??= new FilterMenuDTO();
            oFilterMenuDTO.PageNumber = oFilterMenuDTO.PageNumber ?? 1;
            
            var data = await _apiClient.GetAllMenusAsync(oFilterMenuDTO);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_MenuList", data);
            }
            return View(data);
        }

        [CustomAuthorize(Permissions.Menu.Create)]
        public async Task<IActionResult> Edit(int? Id)
        {
            var oMenuDTO = new MenuDTO();
            if (Id != null)
            {
                oMenuDTO = await _apiClient.GetMenuByIdAsync((int)Id);
                var items = await _apiClient.GetMenuItemsByIdAsync((int)Id);
                ViewBag.MenuItems = items;

            }

            if (oMenuDTO == null) return RedirectToAction(nameof(Index));
            var dto = new MenuDTO
            {
                GroupCode = oMenuDTO.GroupCode,
                GroupName = oMenuDTO.GroupName,
                PrimaryDescription = oMenuDTO.PrimaryDescription,
                SecondaryDescription = oMenuDTO.SecondaryDescription,
                PricingMethod = oMenuDTO.PricingMethod,
                Price = oMenuDTO.Price,
                TotalTime = oMenuDTO.TotalTime,
                EffectiveDate = oMenuDTO.EffectiveDate,
                IsActive = oMenuDTO.IsActive,

            };

            FilterRTSCodeDTO filter = new FilterRTSCodeDTO();
            filter.PageNumber = filter.PageNumber ?? 1;
            var RTSCodes = await _apiClient.GetAllRTSCodesAsync(filter);
            ViewBag.RTSCodes = RTSCodes.Select(t => new SelectListItem { Text = t.Code, Value = t.Id.ToString() }).ToList();
            var fK_GroupId = 0;
            var fK_CategoryId = 0;
            var fK_SubCategoryId = 0;
            
            List<ItemDTO> PartsDDL = await _inventoryApiClient.GetAllItemsAsync(fK_GroupId, fK_CategoryId, fK_SubCategoryId);
            ViewBag.Parts = PartsDDL.Select(t => new SelectListItem { Text = (lang=="en"?t.PrimaryName:t.SecondaryName), Value = t.Id.ToString() }).ToList();

            List<GroupDTO> GroupDDL = await _inventoryApiClient.GetAllGroupsAsync();
            ViewBag.Groups = GroupDDL.Select(t => new SelectListItem { Text = (lang == "en" ? t.primaryName : t.secondaryName), Value = t.Id.ToString() }).ToList();

            List<CategoryDTO> CategoryDDL = await _inventoryApiClient.GetAllCategoriesAsync();
            ViewBag.Categories = CategoryDDL.Select(t => new SelectListItem { Text = (lang == "en" ? t.primaryName : t.secondaryName), Value = t.Id.ToString() }).ToList();

            List<SubCategoryDTO> SubCategoryDDL = await _inventoryApiClient.GetAllSubCategoriesAsync();
            ViewBag.SubCategories = SubCategoryDDL.Select(t => new SelectListItem { Text = (lang == "en" ? t.primaryName : t.secondaryName), Value = t.Id.ToString() }).ToList();

            return View(dto);
        }
       
        [HttpPost]
        [CustomAuthorize(Permissions.Menu.Create)]
        public async Task<IActionResult> Edit(MenuDTO dto)
        {
            //if (!ModelState.IsValid)
            //    return View(dto);

            int? result;

            if (!string.IsNullOrEmpty(dto.ServicesToSave))
            {
                dto.MenuGroup = JsonSerializer.Deserialize<IEnumerable<MenuGroupDTO>>(dto.ServicesToSave);
            }



            if (dto.Id == 0)
            {
                var createDto = MapToCreateDto(dto);
                createDto.CreatedBy = UserId;
                result = await _apiClient.AddMenuAsync(createDto);

                if (!result.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Failed to create menu.");
                    return RedirectToAction("Index");
                }
            }
            else
            {
                var updateDto = MapToUpdateDto(dto);
                updateDto.UpdatedBy = UserId;
                var success = await _apiClient.UpdateMenuAsync(updateDto);

                result = dto.Id;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<JsonResult> GetItemFilteration(int fK_GroupId, int fK_CategoryId, int fK_SubCategoryId)
        {
            List<ItemDTO> PartsDDL = await _inventoryApiClient.GetAllItemsAsync(fK_GroupId, fK_CategoryId, fK_SubCategoryId);
            return Json(PartsDDL);
        }
        public async Task<JsonResult> GetItemById(int Id)
        {
            ItemDTO Part = await _inventoryApiClient.GetItemByIdAsync(Id);
            return Json(Part);
        }

        [HttpGet]
        public async Task<JsonResult> GetRTSCodeById(int Id)
        {
            var result = await _apiClient.GetRTSCodeByIdAsync(Id);
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                DeleteMenuDTO oDeleteMenuDTO = new DeleteMenuDTO();
                oDeleteMenuDTO.Id = id;
                oDeleteMenuDTO.UpdatedBy = UserId;

                var result = await _apiClient.DeleteMenuAsync(oDeleteMenuDTO);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private CreateMenuDTO MapToCreateDto(MenuDTO dto)
        {
            return new CreateMenuDTO
            {
                GroupCode = dto.GroupCode,
                GroupName = dto.GroupName,
                PrimaryDescription = dto.PrimaryDescription,
                SecondaryDescription = dto.SecondaryDescription,
                PricingMethod = dto.PricingMethod,
                Price = dto.Price,
                TotalTime = dto.TotalTime,
                EffectiveDate = dto.EffectiveDate,
                IsActive = dto.IsActive,
                MenuGroup = dto.MenuGroup
            };
        }

        private UpdateMenuDTO MapToUpdateDto(MenuDTO dto)
        {
            return new UpdateMenuDTO
            {
                Id = dto.Id,
                GroupCode = dto.GroupCode,
                GroupName = dto.GroupName,
                PrimaryDescription = dto.PrimaryDescription,
                SecondaryDescription = dto.SecondaryDescription,
                PricingMethod = dto.PricingMethod,
                Price = dto.Price,
                TotalTime = dto.TotalTime,
                EffectiveDate = dto.EffectiveDate,
                IsActive = dto.IsActive,
                MenuGroup=dto.MenuGroup

            };
        }

        
    }
}
