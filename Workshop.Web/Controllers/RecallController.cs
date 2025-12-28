using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class RecallController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        public readonly string lang;
        public RecallController(WorkshopApiClient apiClient, ERPApiClient erpApiClient, IConfiguration configuration, 
            IWebHostEnvironment env, VehicleApiClient vehicleApiClient, IMemoryCache cache) : base(cache, configuration, env)
        {
            _apiClient = apiClient;
            _erpApiClient = erpApiClient;
            _configuration = configuration;
            _env = env;
            _vehicleApiClient = vehicleApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.Recall.View)]
        public async Task<IActionResult> Index(RecallModel recall)
        {
            if (recall == null)
            {
                recall = new RecallModel();
            }
            recall.RecallFilter ??= new FilterRecallDTO();
            recall.RecallFilter.PageNumber = recall.RecallFilter.PageNumber ?? 1;
            recall.RecallFilter.Tittle = recall.RecallFilter.Tittle ?? string.Empty;
            recall.RecallFilter.Code = recall.RecallFilter.Code ?? string.Empty;

            var data = await _apiClient.GetAllRecallAsync(recall.RecallFilter);
            recall.Recalls = data ?? new List<RecallDTO>();

            return View(recall);
        }

        [CustomAuthorize(Permissions.Recall.Create)]
        public async Task<IActionResult> Edit(int Id)
        {
            var Makes = await GetMakes();
            var Vehicles = await GetVehicles();
            var Chassis = await GetChasses();
            var VehcileStatuses = await GetVehicleStatuses();
            ViewBag.Makes = Makes;
            ViewBag.Vehicles = Vehicles;
            ViewBag.ChassisNo = Chassis;
            ViewBag.VehcileStatuses = VehcileStatuses;
            if (Id > 0)
            {
                var recallItem = await _apiClient.GetRecallByIdAsync(Id);
                ViewBag.VehcilesRecall = recallItem?.Vehicles;
                return View(recallItem);
            }
            return View(new RecallDTO());
        }

        [HttpPost]
        [CustomAuthorize(Permissions.Recall.Create)]
        public async Task<IActionResult> Edit(RecallDTO recall, string recallJson)
        {
            var vehicleRecallJson = JsonConvert.DeserializeObject<List<VehicleRecallDTO>>(recallJson) ?? new List<VehicleRecallDTO>();

            ViewBag.Makes = await GetMakes();
            ViewBag.Vehicles = await GetVehicles();
            ViewBag.ChassisNo = await GetChasses();
            
            if (recall != null)
            {
                if (recall.Id > 0)
                {

                    foreach (var vehicleRecallItem in vehicleRecallJson)
                    {
                        if (recall.Vehicles == null)
                            recall.Vehicles = new List<VehicleRecallDTO>();

                        VehicleRecallDTO vehicleRecallDTO = new VehicleRecallDTO();
                        vehicleRecallDTO.Id = vehicleRecallItem.Id;
                        vehicleRecallDTO.MakeID = vehicleRecallItem.MakeID;
                        vehicleRecallDTO.ModelID = vehicleRecallItem.ModelID;
                        vehicleRecallDTO.Chassis = vehicleRecallItem.Chassis;
                        vehicleRecallDTO.RecallStatus = vehicleRecallItem.RecallStatus;
                        vehicleRecallDTO.RecallID = recall.Id;
                        recall.Vehicles.Add(vehicleRecallDTO);
                    }

                    var result = await _apiClient.UpdateRecallAsync(MapToUpdateRecall(recall));
                }
                else
                {
                    foreach (var vehicleRecallItem in vehicleRecallJson)
                    {
                        if (recall.Vehicles == null)
                            recall.Vehicles = new List<VehicleRecallDTO>();

                        VehicleRecallDTO vehicleRecallDTO = new VehicleRecallDTO();
                        vehicleRecallDTO.Id = vehicleRecallItem.Id;
                        vehicleRecallDTO.MakeID = vehicleRecallItem.MakeID;
                        vehicleRecallDTO.ModelID = vehicleRecallItem.ModelID;
                        vehicleRecallDTO.Chassis = vehicleRecallItem.Chassis;
                        vehicleRecallDTO.RecallStatus = vehicleRecallItem.RecallStatus;
                        vehicleRecallDTO.RecallID = recall.Id;
                        recall.Vehicles.Add(vehicleRecallDTO);
                    }
                    var result = await _apiClient.AddRecallAsync(MapToCreateRecall(recall));
                }

            }

            return RedirectToAction("Index");
        }

        private UpdateRecallDTO MapToUpdateRecall(RecallDTO recallDTO)
        {
            return new UpdateRecallDTO
            {
                Id = recallDTO.Id,
                Code = recallDTO.Code,
                Description = recallDTO.Description,
                Title = recallDTO.Title,
                StartDate = recallDTO.StartDate,
                EndDate = recallDTO.EndDate,
                IsActive = recallDTO.IsActive,
                Vehicles = recallDTO.Vehicles

            };
        }
        private CreateRecallDTO MapToCreateRecall(RecallDTO recallDTO)
        {
            return new CreateRecallDTO
            {
                Code = recallDTO.Code,
                Description = recallDTO.Description,
                Title = recallDTO.Title,
                StartDate = recallDTO.StartDate,
                EndDate = recallDTO.EndDate,
                IsActive = recallDTO.IsActive,
                Vehicles = recallDTO.Vehicles
            };
        }

        private async Task<List<SelectListItem>> GetMakes()
        {

            var makes = await _vehicleApiClient.GetAllManufacturers(lang);

            return makes.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = lang == "en" ? m.ManufacturerPrimaryName : m.ManufacturerSecondaryName

            }).ToList();
        }
        private async Task<List<VehicleModel>> GetVehicles()
        {
            var models = await _vehicleApiClient.GetAllVehicleModel(0, lang);

            return models.Select(m => new VehicleModel
            {
                Id = m.Id,
                Name = lang == "en" ? m.VehicleModelPrimaryName : m.VehicleModelSecondaryName,
                ManufacturerId = m.ManufacturerId
            }).ToList();
        }

        private async Task<List<VehicleNams>> GetChasses()
        {
            var models = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);

            return models.Select(m => new VehicleNams
            {
                id = m.id,
                ChassisNo = m.ChassisNo,
                ManufacturerName = m.ManufacturerName,
                ModelName = m.ModelName,

            }).ToList();
        }
        private async Task<List<SelectListItem>> GetVehicleStatuses()
        {
            var result = Enum.GetValues(typeof(VehicleRecallStatus))
                .Cast<VehicleRecallStatus>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                })
                .ToList();

            return result;
        }

        public async Task<VehicleDefinitions> GetVehicleById(int Id)
        {
            var models = await _vehicleApiClient.GetVehicleDetails(Id, lang);
            return models;
        }


        public async Task<IActionResult> DownloadTemplate(string GridData)
        {
            try
            {
                // Deserialize grid data
                var vehicles = JsonConvert.DeserializeObject<List<VehicleRecallDTO>>(GridData);
                if (vehicles == null || !vehicles.Any())
                    return View();

                // Get data for dropdowns
                var Makes = await GetMakes();       // List<Make> { Text, Value }
                var Models = await GetVehicles();   // List<Vehicle> { Name, Id }
                var ChassisList = await GetChasses(); // List<Chasse> { ChassisNo, Id }

                IWorkbook wb = new XSSFWorkbook();

                // ---------------------------
                // Hidden sheet for dropdown lists
                // ---------------------------
                ISheet listSheet = wb.CreateSheet("Lists");

                // Populate Makes (Columns A-B)
                for (int i = 0; i < Makes.Count; i++)
                {
                    var row = listSheet.GetRow(i) ?? listSheet.CreateRow(i);
                    row.CreateCell(0).SetCellValue(Makes[i].Text); 
                    row.CreateCell(1).SetCellValue(Makes[i].Value); 
                }

                // Populate Models (Columns C-D)
                for (int i = 0; i < Models.Count; i++)
                {
                    var row = listSheet.GetRow(i) ?? listSheet.CreateRow(i);
                    row.CreateCell(2).SetCellValue(Models[i].Name); 
                    row.CreateCell(3).SetCellValue(Models[i].Id);  
                }

                // Populate Chassis (Columns E-F)
                for (int i = 0; i < ChassisList.Count; i++)
                {
                    var row = listSheet.GetRow(i) ?? listSheet.CreateRow(i);
                    row.CreateCell(4).SetCellValue(ChassisList[i].ChassisNo);
                    row.CreateCell(5).SetCellValue(ChassisList[i].id);        
                }

                // Hide the Lists sheet
                wb.SetSheetHidden(wb.GetSheetIndex(listSheet), SheetVisibility.VeryHidden);

                // ---------------------------
                // Create named ranges for dropdowns
                // ---------------------------
                var makeName = wb.CreateName();
                makeName.NameName = "MakeList";
                makeName.RefersToFormula = $"Lists!$A$1:$A${Makes.Count}";

                var modelName = wb.CreateName();
                modelName.NameName = "ModelList";
                modelName.RefersToFormula = $"Lists!$C$1:$C${Models.Count}";

                var chassisName = wb.CreateName();
                chassisName.NameName = "ChassisList";
                chassisName.RefersToFormula = $"Lists!$E$1:$E${ChassisList.Count}";

                // ---------------------------
                // Main sheet
                // ---------------------------
                ISheet sheet = wb.CreateSheet("Recall");

                // Header row
                IRow header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("Id");
                header.CreateCell(1).SetCellValue("Make");
                header.CreateCell(2).SetCellValue("MakeID");   // hidden
                header.CreateCell(3).SetCellValue("Model");
                header.CreateCell(4).SetCellValue("ModelID");  // hidden
                header.CreateCell(5).SetCellValue("Chassis");
                header.CreateCell(6).SetCellValue("ChassisID");// hidden

                int rowCount = vehicles.Count;

                // Fill rows with existing data
                for (int i = 0; i < rowCount; i++)
                {
                    var vehicle = vehicles[i];
                    IRow row = sheet.CreateRow(i + 1);

                    // Vehicle ID
                    row.CreateCell(0).SetCellValue(vehicle.Id ?? 0);

                    // Make
                    string makeText = Makes.FirstOrDefault(m => m.Value == vehicle.MakeID?.ToString())?.Text ?? "";
                    row.CreateCell(1).SetCellValue(makeText);
                    row.CreateCell(2).SetCellFormula($"IF(B{i + 2}=\"\",\"\",VLOOKUP(B{i + 2},Lists!$A$1:$B${Makes.Count},2,FALSE))");

                    // Model
                    string modelText = Models.FirstOrDefault(m => m.Id == vehicle.ModelID)?.Name ?? "";
                    row.CreateCell(3).SetCellValue(modelText);
                    row.CreateCell(4).SetCellFormula($"IF(D{i + 2}=\"\",\"\",VLOOKUP(D{i + 2},Lists!$C$1:$D${Models.Count},2,FALSE))");

                    // Chassis
                    string chassisText = vehicle.Chassis ?? "";
                    row.CreateCell(5).SetCellValue(chassisText);
                    row.CreateCell(6).SetCellFormula($"IF(F{i + 2}=\"\",\"\",VLOOKUP(F{i + 2},Lists!$E$1:$F${ChassisList.Count},2,FALSE))");
                }

                // ---------------------------
                // Data validation (dropdowns with autocomplete)
                // ---------------------------
                var dvHelper = sheet.GetDataValidationHelper();

                // Make dropdown (Column B)
                var makeRange = new CellRangeAddressList(1, 2000, 2, 2);
                var makeConstraint = dvHelper.CreateFormulaListConstraint("MakeList");
                var makeValidation = dvHelper.CreateValidation(makeConstraint, makeRange);
                makeValidation.SuppressDropDownArrow = false;
                sheet.AddValidationData(makeValidation);

                // Model dropdown (Column D)
                var modelRange = new CellRangeAddressList(1, 2000, 4, 4);
                var modelConstraint = dvHelper.CreateFormulaListConstraint("ModelList");
                var modelValidation = dvHelper.CreateValidation(modelConstraint, modelRange);
                modelValidation.SuppressDropDownArrow = false;
                sheet.AddValidationData(modelValidation);

                // Chassis dropdown (Column F)
                var chassisRange = new CellRangeAddressList(1, 2000, 6, 6);
                var chassisConstraint = dvHelper.CreateFormulaListConstraint("ChassisList");
                var chassisValidation = dvHelper.CreateValidation(chassisConstraint, chassisRange);
                chassisValidation.SuppressDropDownArrow = false;
                sheet.AddValidationData(chassisValidation);

                // ---------------------------
                // Hide ID columns (C, E, G)
                // ---------------------------
                sheet.SetColumnHidden(2, true);
                sheet.SetColumnHidden(4, true);
                sheet.SetColumnHidden(6, true);

                // Auto-size columns
                for (int i = 0; i <= 6; i++)
                    sheet.AutoSizeColumn(i);

                // ---------------------------
                // Save workbook
                // ---------------------------
                var fileName = $"Recall_Dropdown_Template_{DateTime.Now:yyyyMMdd}.xlsx";
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    wb.Write(fs);

                wb.Close();
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "Error generating Excel file");
            }
        }

        [HttpPost]
        public async Task<ExcelImportModel> ImportTemplate(IFormFile ExcelVehicle)
        {
            if (ExcelVehicle == null)
                return null;

            try
            {
                // Get reference data
                var makes = await GetMakes();       // List<Make> { Text, Value }
                var models = await GetVehicles();   // List<Vehicle> { Name, Id }

                var rowsOut = new List<VehicleRecallDTO>();
                var ExcelImportModel = new ExcelImportModel();

                using (var wb = new XLWorkbook(ExcelVehicle.OpenReadStream()))
                {
                    var wsT = wb.Worksheet("Recall");
                    int startRow = 2;
                    int lastRow = wsT.LastRowUsed()?.RowNumber() ?? startRow;

                    for (int r = startRow; r <= lastRow; r++)
                    {
                        // Read all relevant cells as strings
                        var excelID = string.IsNullOrWhiteSpace(wsT.Cell(r, 1).GetString())
                            ? null
                            : wsT.Cell(r, 1).GetString().Trim();

                        var excelMakeText = string.IsNullOrWhiteSpace(wsT.Cell(r, 1).GetString())
                            ? null
                            : wsT.Cell(r, 1).GetString().Trim();

                        var excelModelText = string.IsNullOrWhiteSpace(wsT.Cell(r, 2).GetString())
                            ? null
                            : wsT.Cell(r, 2).GetString().Trim();

                        var excelChasse = string.IsNullOrWhiteSpace(wsT.Cell(r, 3).GetString())
                            ? null
                            : wsT.Cell(r, 3).GetString().Trim();

                        // Skip empty rows
                        if (string.IsNullOrWhiteSpace(excelID) &&
                            string.IsNullOrWhiteSpace(excelMakeText) &&
                            string.IsNullOrWhiteSpace(excelModelText) &&
                            string.IsNullOrWhiteSpace(excelChasse))
                            continue;


                        // Map dropdown text to IDs
                        int? makeID = null;
                        int? modelID = null;

                        if (!string.IsNullOrEmpty(excelMakeText))
                        {
                            var makeMatch = makes.FirstOrDefault(m => m.Text.Equals(excelMakeText, StringComparison.OrdinalIgnoreCase));
                            if (makeMatch != null && int.TryParse(makeMatch.Value, out var parsedMakeID))
                                makeID = parsedMakeID;
                        }

                        if (!string.IsNullOrEmpty(excelModelText))
                        {
                            var modelMatch = models.FirstOrDefault(m => m.Name.Equals(excelModelText, StringComparison.OrdinalIgnoreCase));
                            if (modelMatch != null)
                                modelID = modelMatch.Id;
                        }

                        // Parse vehicle ID
                        int vehicleID = 0;
                        if (!string.IsNullOrEmpty(excelID))
                            int.TryParse(excelID, out vehicleID);

                        // Add to output

                        if (makes.Any(g => g.Text == excelMakeText &&
                            models.Any(a => a.Name == excelModelText && g.Value == a.ManufacturerId.ToString())))
                        {
                            ExcelImportModel.ImportedRows.Add(new VehicleRecallDTO
                            {
                                Id = vehicleID,
                                MakeID = makeID,
                                ModelID = modelID,
                                Chassis = excelChasse
                            });
                        }
                        else
                        {
                            ExcelImportModel.RejectedRows.Add(new VehicleRecallDTO
                            {
                                Id = vehicleID,
                                MakeID = makeID,
                                ModelID = modelID,
                                Chassis = excelChasse
                            });
                        }

                    }
                }

                return ExcelImportModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }


        [HttpPost]
        public async Task<int> DeleteRecall(int Id)
        {
            DeleteRecallDTO deleteRecall = new DeleteRecallDTO();
            deleteRecall.Id = Id;
            deleteRecall.IsActive = false;
            deleteRecall.UpdatedBy = 1;
            var result = await _apiClient.DeleteRecallAsync(deleteRecall);
            return result;
        }

    }
}
