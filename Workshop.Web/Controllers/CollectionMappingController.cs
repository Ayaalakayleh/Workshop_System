using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Data;
using Workshop.Core.DTOs.ExternalWorkshopExp;
using Workshop.Core.DTOs.TempData;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]

    public class CollectionMappingController : BaseController
    {
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly WorkshopApiClient _workshopApiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly IFileService _fileService;
        private readonly IFileValidationService _fileValidationService;
        private readonly string lang;

        public CollectionMappingController(
            VehicleApiClient vehicleApiClient,
            WorkshopApiClient workshopApiClient,
            AccountingApiClient accountingApiClient,
            IFileService fileService,
            IFileValidationService fileValidationService,
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _vehicleApiClient = vehicleApiClient;
            _workshopApiClient = workshopApiClient;
            _accountingApiClient = accountingApiClient;
            _fileService = fileService;
            _fileValidationService = fileValidationService;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        #region Collection Mapping 

        [CustomAuthorize(Permissions.CollectionMapping.View)]
        public async Task<IActionResult> CollectionMappingIndex()
        {
            try
            {

                MExcelMappingDTO filter = new MExcelMappingDTO();

                var externalWorkshops = await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId);
                ViewBag.WorkshopList = externalWorkshops?.Select(r => new SelectListItem { Text = /*GetCurrentBilanguage(r.SecondaryName, r.PrimaryName)*/ lang == "en" ? r.SecondaryName : r.PrimaryName, Value = r.Id.ToString() }).ToList();
                return View(filter);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.CollectionMapping.View)]
        public async Task<IActionResult> CollectionMappingList([FromBody] ExcelMappingFilterDTO filter)
        {

            try
            {
                filter ??= new ExcelMappingFilterDTO();
                List<MExcelMappingDTO> Excel_Mappin = new List<MExcelMappingDTO>();
                filter.CompanyId = CompanyId;
                Excel_Mappin = (await _workshopApiClient.GetExcelMappingAsync(filter))?.ToList();
                return PartialView("_CollectionMappingList", Excel_Mappin);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [CustomAuthorize(Permissions.CollectionMapping.Create)]
        public async Task<IActionResult> CollectionMapping(int? Id)
        {
            MExcelMappingDTO mExcelMapping = new MExcelMappingDTO();

            ExcelMappingFilterDTO filter = new ExcelMappingFilterDTO() { CompanyId = CompanyId, BranchId = BranchId };


            if (Id != null)
            {
                filter.WorkshopId = 0;
                filter.Id = Id ?? 0;
                mExcelMapping = (await _workshopApiClient.GetExcelMappingAsync(filter))?.FirstOrDefault();
                mExcelMapping.ExcelMappingColumnsList = (await _workshopApiClient.GetExcelMappingColumnsAsync())?.ToList();
                mExcelMapping.DExcelMappingList = (await _workshopApiClient.GetExcelMappingDetailsByIdAsync(Id, null))?.ToList();
                var _External_Workshop = (await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId))?.Where(a => a.Id == mExcelMapping.WorkshopId).ToList();
                ViewBag.WorkshopList = _External_Workshop.Select(r => new SelectListItem { Text = /*GetCurrentBilanguage(r.SecondaryName, r.PrimaryName)*/ lang == "en" ? r.SecondaryName : r.PrimaryName, Value = r.Id.ToString() }).ToList();
            }
            else
            {
                var WorkshopIds = (await _workshopApiClient.GetExcelMappingAsync(filter))?.Select(a => a.WorkshopId).ToList();
                var _External_Workshop = (await _workshopApiClient.WorkshopGetAllAsync(CompanyId, BranchId))?.Where(a => a.Id.HasValue && !WorkshopIds.Contains(a.Id.Value)).ToList();
                ViewBag.WorkshopList = _External_Workshop.Select(r => new SelectListItem { Text = /*GetCurrentBilanguage(r.SecondaryName, r.PrimaryName)*/ lang == "en" ? r.SecondaryName : r.PrimaryName, Value = r.Id.ToString() }).ToList();
            }

            mExcelMapping.ExcelMappingColumnsList = (await _workshopApiClient.GetExcelMappingColumnsAsync())?.ToList();

            return View(mExcelMapping);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.CollectionMapping.Create)]
        public async Task<IActionResult> CollectionMapping(MExcelMappingDTO mExcelMapping, IFormFile File)
        {
            TempData result = new TempData();
            try
            {
                var validationResult = _fileValidationService.CheckFileTypeAndSize(File);
                mExcelMapping.DExcelMappingList = JsonConvert.DeserializeObject<List<DExcelMappingDTO>>(mExcelMapping.jsonList);

                if (File != null && File.Length > 0)
                {
                    if (validationResult.IsSuccess)
                    {
                        var (filePath, fileName) = await _fileService.SaveFileAsync(File, "Collection_Mapping");
                        mExcelMapping.FilePath = filePath;
                        mExcelMapping.FileName = fileName;
                    }
                    else
                    {
                        result.IsSuccess = false;
                        result.Message = validationResult.Message;
                        return Json(result);
                    }
                }

                mExcelMapping.BranchId = BranchId;
                mExcelMapping.CompanyId = CompanyId;

                foreach (var item in mExcelMapping?.DExcelMappingList)
                {
                    try
                    {
                        int StringIndex = item.Mapping_ColumnDB.IndexOf('-');
                        item.Mapping_ColumnDB = item.Mapping_ColumnDB.Substring(0, StringIndex).Trim();
                    }
                    catch
                    {
                        // Keep original value if parsing fails
                    }
                }

                if (mExcelMapping.Id.HasValue)
                {
                    mExcelMapping.UpdatedBy = UserId;
                    UpdateExcelMappingDTO updateExcelMappingDTO = new UpdateExcelMappingDTO();
                    updateExcelMappingDTO.MapToUpdateExcelMappingDTO(mExcelMapping);
                    await _workshopApiClient.UpdateExcelMappingAsync(updateExcelMappingDTO);
                }
                else
                {
                    mExcelMapping.CreatedBy = UserId;

                    CreateExcelMappingDTO createExcelMappingDTO = new CreateExcelMappingDTO();
                    createExcelMappingDTO.MapToCreateExcelMappingDTO(mExcelMapping);

                    await _workshopApiClient.InsertExcelMappingAsync(createExcelMappingDTO);
                }

                result.IsSuccess = true;
                return Json(result);
            }
            catch (Exception ex)
            {
                // Log error
                result.IsSuccess = false;
                result.Message = "ErrorHappend";
                return Json(result);
            }
        }

        public async Task<JsonResult> DownloadTemplate(int workShopId)
        {
            try
            {
                ExcelMappingFilterDTO filter = new ExcelMappingFilterDTO { CompanyId = CompanyId, WorkshopId = workShopId };

                var workShop = (await _workshopApiClient.GetExcelMappingAsync(filter))?.FirstOrDefault();

                if (workShop == null || string.IsNullOrEmpty(workShop.FilePath))
                {
                    return Json(null);
                }
                string fullPath = _fileService.GetFileFullPath(workShop.FilePath);
                return Json(fullPath);
            }
            catch (Exception)
            {

                throw;
            }

        }

        private string GetCurrentBilanguage(string ar, string en)
        {
            if (lang == "en")
            {
                return en;
            }
            else
            {
                return ar;
            }
        }
        #endregion

    }
}
