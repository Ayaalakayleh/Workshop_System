using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Domain.Enum;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class ReportsController : BaseController
    {
        private readonly WorkshopApiClient _apiClient;
        private readonly ERPApiClient _erpClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly InventoryApiClient _inventoryApiClient;
        public readonly string lang;
        private readonly ReportsServiceApiClient _reportsServiceApiClient;
        public ReportsController(
            WorkshopApiClient apiClient,
            IConfiguration configuration,
            ERPApiClient eRPApiClient,
            AccountingApiClient accountingApiClient,
            VehicleApiClient vehicleApiClient,
            ReportsServiceApiClient repo,
            InventoryApiClient inventoryApiClient,
            IWebHostEnvironment env) : base(null, configuration, env)
        {
            _apiClient = apiClient;
            _erpClient = eRPApiClient;
            _accountingApiClient = accountingApiClient;
            _vehicleApiClient = vehicleApiClient;
            _reportsServiceApiClient = repo;
            _inventoryApiClient = inventoryApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }


        public async Task<IActionResult> MonthlyRepairCost()
        {
            var isCompanyCenterialized = 1;
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            ViewBag.Customers = allCustomers?.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CustomerName
                //Selected = selectedCustomerId.HasValue && c.Id == selectedCustomerId.Value
            }).ToList() ?? new List<SelectListItem>();

            var model = new MonthlyRepairCostReportViewModel
            {
                Filter = new MonthlyRepairCostReportFilterDTO(),
                ReportData = new List<MonthlyRepairCostReportDTO>()
            };

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> GetMonthlyRepairCostReport([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            try
            {
                //var data = await _apiClient.GetMonthlyRepairCostReportAsync(filter);
                var isCompanyCenterialized = 1;
                // Call API to generate Excel report
                var data = await _apiClient.GetMonthlyRepairCostReportAsync(filter);
                var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(15, CompanyId);
                foreach (var item in data)
                {

                    item.OPName = await GetUserFullNameAsync(item.OPNumber);
                    var move = await _apiClient.GetVehicleMovementByIdAsync(item.MovementId ?? 0);
                    item.Account = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.AccountNo).FirstOrDefault().ToString();
                    item.CustomerName = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.CustomerName).FirstOrDefault();
                    item.CompanyCode = (await GetBranchNameAsync(item.workshopId)).ToString();
                    item.Millage = move.ReceivedMeter;
                    item.VehServiceCode = VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.Code;
                    item.VehServiceDesc = lang == "en" ? VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.PrimaryName : VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.SecondaryName;

                    if (item.IsExternal ?? false)
                    {
                        var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                        item.VIN = vehicleDetails.ChassisNo;

                    }
                    else
                    {
                        var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(item.VehicleId ?? 0)) ?? new CreateVehicleDefinitionsModel();
                        item.VIN = vehicleDetails.ChassisNo;

                    }
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ReportList", data);
                }

                var model = new MonthlyRepairCostReportViewModel
                {
                    Filter = filter,
                    ReportData = data.ToList()
                };

                return View("MonthlyRepairCost", model);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]

        public async Task<IActionResult> PrintMonthlyRepairCostReport([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            try
            {
                // Call ReportsService to generate PDF using Crystal Report
                using (var httpClient = new HttpClient())
                {
                    // Get ReportsService URL from configuration or use default
                    var reportsServiceUrl = _configuration["ReportsService:BaseUrl"] ?? "https://localhost:44332";

                    // First test if ReportsService is accessible
                    try
                    {
                        var testResponse = await httpClient.GetAsync($"{reportsServiceUrl}/api/reports/test");
                        if (!testResponse.IsSuccessStatusCode)
                        {
                            return Json(new { success = false, message = $"ReportsService is not accessible at {reportsServiceUrl}. Please ensure the ReportsService is running." });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = $"Cannot connect to ReportsService at {reportsServiceUrl}: {ex.Message}" });
                    }

                    var response = await httpClient.PostAsJsonAsync($"{reportsServiceUrl}/api/reports/monthlyrepaircost", filter);

                    if (response.IsSuccessStatusCode)
                    {
                        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                        return File(pdfBytes, "application/pdf", $"MonthlyRepairCostReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = $"Failed to generate PDF report: {errorContent}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]

        public async Task<IActionResult> ExportMonthlyRepairCostReportToExcel([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            try
            {
                // Call API to generate Excel report
                var excelBytes = await _apiClient.GetMonthlyRepairCostExcelReportAsync(filter);



                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MonthlyRepairCostReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]

        public async Task<IActionResult> ExportMonthlyRepairCostReportToExcelT([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            try
            {
                var isCompanyCenterialized = 1;
                // Call API to generate Excel report
                var data = await _apiClient.GetMonthlyRepairCostReportAsync(filter);
                var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(15, CompanyId);
                foreach (var item in data)
                {

                    item.OPName = await GetUserFullNameAsync(item.OPNumber);
                    var move = await _apiClient.GetVehicleMovementByIdAsync(item.MovementId ?? 0);
                    item.Account = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.AccountNo).FirstOrDefault().ToString();
                    item.CustomerName = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.CustomerName).FirstOrDefault();
                    item.CompanyCode =  (await GetBranchNameAsync(item.workshopId)).ToString();
                    item.Millage = move.ReceivedMeter;
                    item.VehServiceCode = VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.Code;
                    item.VehServiceDesc = lang == "en" ? VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.PrimaryName: VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.SecondaryName;
                    if (item.IsExternal ?? false)
                    {
                        var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                        item.VIN = vehicleDetails.ChassisNo;
   
                    }
                    else
                    {
                        var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(item.VehicleId ?? 0)) ?? new CreateVehicleDefinitionsModel();
                        item.VIN = vehicleDetails.ChassisNo;

                    }
                }

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Monthly Repair Cost Report");
                if (lang == "en")
                {
                    // Add headers
                    worksheet.Cell(1, 1).Value = "WIP";
                    worksheet.Cell(1, 2).Value = "Invoice Date";
                    worksheet.Cell(1, 3).Value = "Invoice Number";
                    worksheet.Cell(1, 4).Value = "Company Code";
                    worksheet.Cell(1, 5).Value = "Customer Name";
                    worksheet.Cell(1, 6).Value = "Total Amount";
                    worksheet.Cell(1, 7).Value = "Total Labours";
                    worksheet.Cell(1, 8).Value = "Total Parts";
                    worksheet.Cell(1, 9).Value = "VIN";
                    worksheet.Cell(1, 10).Value = "Plate Number";
                    worksheet.Cell(1, 11).Value = "Manufacture Year";
                    worksheet.Cell(1, 12).Value = "Millage";
                    worksheet.Cell(1, 13).Value = "OP Number";
                    worksheet.Cell(1, 14).Value = "OP Name";
                    worksheet.Cell(1, 15).Value = "Service Code";
                    worksheet.Cell(1, 16).Value = "Service Description";
                }
                else
                {
                    worksheet.Cell(1, 1).Value = "أمر العمل";
                    worksheet.Cell(1, 2).Value = "تاريخ الفاتورة";
                    worksheet.Cell(1, 3).Value = "رقم الفاتورة";
                    worksheet.Cell(1, 4).Value = "رمز الشركة";
                    worksheet.Cell(1, 7).Value = "اسم العميل";
                    worksheet.Cell(1, 8).Value = "إجمالي المبلغ";
                    worksheet.Cell(1, 9).Value = "إجمالي الأجور";
                    worksheet.Cell(1, 10).Value = "إجمالي القطع";
                    worksheet.Cell(1, 11).Value = "رقم تعريف المركبة";
                    worksheet.Cell(1, 12).Value = "رقم اللوحة";
                    worksheet.Cell(1, 13).Value = "سنة الصنع";
                    worksheet.Cell(1, 13).Value = "العداد";
                    worksheet.Cell(1, 14).Value = "رقم الشخص المسؤول";
                    worksheet.Cell(1, 15).Value = "اسم الشخص المسؤول";
                    worksheet.Cell(1, 16).Value = "رمز الخدمة";
                    worksheet.Cell(1, 17).Value = "وصف الخدمة";
                }

                    int row = 2;
                foreach (var item in data)
                {
                    worksheet.Cell(row, 1).Value = item.WIP;
                    worksheet.Cell(row, 2).Value = item.InvoiceDate;
                    worksheet.Cell(row, 3).Value = item.InvoiceNumber;
                    worksheet.Cell(row, 4).Value = item.CompanyCode;
                    //worksheet.Cell(row, 5).Value = item.Department;
                    worksheet.Cell(row, 5).Value = item.CustomerName;
                    worksheet.Cell(row, 6).Value = item.TotalAmount;
                    worksheet.Cell(row, 7).Value = item.TotalLabours;
                    worksheet.Cell(row, 8).Value = item.TotalParts;
                    worksheet.Cell(row, 9).Value = item.VIN;
                    worksheet.Cell(row, 10).Value = item.PlateNumber;
                    worksheet.Cell(row, 11).Value = item.ManufactureYear;
                    worksheet.Cell(row, 12).Value = item.Millage;
                    worksheet.Cell(row, 13).Value = item.OPNumber;
                    worksheet.Cell(row, 14).Value = item.OPName;
                    worksheet.Cell(row, 15).Value = item.VehServiceCode;
                    worksheet.Cell(row, 16).Value = item.VehServiceDesc;
                    row++;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MonthlyRepairCostReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                return File(
                stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"MonthlyRepairCostReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task<string> GetUserFullNameAsync(int? userId)
        {
            if (userId is null) return string.Empty;

            var user = await _erpClient.GetUserInfoById(userId.Value);
            var first = user?.FirstName?.Trim();
            var last = user?.LastName?.Trim();

            return string.Join(" ",
                new[] { first, last }.Where(x => !string.IsNullOrWhiteSpace(x))
            );
        }
        private async Task<string> GetUserNameAsync(int? userId)
        {
            if (userId is null) return string.Empty;

            var user = await _erpClient.GetUserInfoById(userId.Value);
            var userName = user?.Username?.Trim();

            return userName;
        }
        private async Task<int> GetBranchNameAsync(int? branchId)
        {
            if (branchId is null) return 0;

            var user = await _erpClient.GetBranchById(branchId ?? 0);
            var branch = user?.BranchNumber ?? 0;

            return branch;
            
        }
        public async Task<IActionResult> MonthlyRepairCostBranchWise()
        {
            var isCompanyCenterialized = 1;
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            ViewBag.Customers = allCustomers?.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CustomerName
            }).ToList() ?? new List<SelectListItem>();
            var model = new MonthlyRepairCostBranchWiseReportViewModel
            {
                Filter = new MonthlyRepairCostReportFilterDTO(),
                ReportData = new List<MonthlyRepairCostBranchWiseReportDTO>()
            };

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> GetMonthlyRepairCostBranchWiseReport([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            try
            {
                var isCompanyCenterialized = 1;
                var data = await _apiClient.GetMonthlyRepairCostBranchWiseReportAsync(filter);
                var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                var allManufacturers = await GetMakes();
                foreach (var item in data)
                {

                    item.OPName = await GetUserFullNameAsync(item.OPNumber);
                    item.OPUserName = await GetUserNameAsync(item.OPNumber);
                    item.Account = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.AccountNo).FirstOrDefault().ToString();
                    item.CustomerName = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.CustomerName).FirstOrDefault();
                    //item.TotalAmount = item.TotalLabours + item.TotalParts;
                    item.Sublet = item.LabourCost + item.PartsCost;
                    item.CompanyCode = (await GetBranchNameAsync(item.workshopId)).ToString();
                    if (item.IsExternal ?? false)
                    {
                        var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                        item.VIN = vehicleDetails.ChassisNo;
                        item.ManufacturerId = vehicleDetails.ManufacturerId;
                        item.Manufacturer = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerPrimaryName).FirstOrDefault();
                    }
                    else
                    {
                        var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(item.VehicleId ?? 0)) ?? new CreateVehicleDefinitionsModel();
                        item.VIN = vehicleDetails.ChassisNo;
                        item.ManufacturerId = vehicleDetails.ManufacturerId;
                        item.Manufacturer = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerPrimaryName).FirstOrDefault();
                    }
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ReportListBranchWise", data);
                }

                var model = new MonthlyRepairCostBranchWiseReportViewModel
                {
                    Filter = filter,
                    ReportData = data.ToList()
                };

                return View("MonthlyRepairCostBranchWise", model);
            }
            catch (Exception ex)
            {
                // Handle error appropriately
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]

        public async Task<IActionResult> PrintMonthlyRepairCostBranchWiseReport([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            try
            {
                // Call ReportsService to generate PDF using Crystal Report
                using (var httpClient = new HttpClient())
                {
                    // Get ReportsService URL from configuration or use default
                    var reportsServiceUrl = _configuration["ReportsService:BaseUrl"] ?? "https://localhost:44332";

                    // First test if ReportsService is accessible
                    try
                    {
                        var testResponse = await httpClient.GetAsync($"{reportsServiceUrl}/api/reports/test");
                        if (!testResponse.IsSuccessStatusCode)
                        {
                            return Json(new { success = false, message = $"ReportsService is not accessible at {reportsServiceUrl}. Please ensure the ReportsService is running." });
                        }
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = $"Cannot connect to ReportsService at {reportsServiceUrl}: {ex.Message}" });
                    }

                    var response = await httpClient.PostAsJsonAsync($"{reportsServiceUrl}/api/reports/monthlyrepaircostbranchwise", filter);

                    if (response.IsSuccessStatusCode)
                    {
                        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                        return File(pdfBytes, "application/pdf", $"MonthlyRepairCostBranchWiseReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = $"Failed to generate PDF report: {errorContent}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]

        public async Task<IActionResult> ExportMonthlyRepairCostBranchWiseReportToExcel([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            try
            {
                // Call API to generate Excel report
                var excelBytes = await _apiClient.GetMonthlyRepairCostBranchWiseExcelReportAsync(filter);

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MonthlyRepairCostBranchWiseReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]

        public async Task<IActionResult> ExportMonthlyRepairCostBranchWiseReportToExcelT([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {

            // Call API to generate Excel report
            var isCompanyCenterialized = 1;
            //var data = await _apiClient.GetMonthlyRepairCostBranchWiseReportAsync(filter);
            //    var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            //    foreach (var item in data)
            //    {

            //        item.OPName = await GetUserFullNameAsync(item.OPNumber);
            //        item.Account = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.AccountNo).FirstOrDefault().ToString();
            //        item.CustomerName = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.CustomerName).FirstOrDefault();
            //        item.TotalAmount = item.TotalLabours + item.TotalParts;
            //    }

            var data = await _apiClient.GetMonthlyRepairCostBranchWiseReportAsync(filter);
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            var allManufacturers = await GetMakes();
            foreach (var item in data)
            {

                item.OPName = await GetUserFullNameAsync(item.OPNumber);
                item.OPUserName = await GetUserNameAsync(item.OPNumber);
                item.Account = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.AccountNo).FirstOrDefault().ToString();
                item.CustomerName = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.CustomerName).FirstOrDefault();
                //item.TotalAmount = item.TotalLabours + item.TotalParts;
                item.Sublet = item.LabourCost + item.PartsCost;
                item.CompanyCode = (await GetBranchNameAsync(item.workshopId)).ToString();
                if (item.IsExternal ?? false)
                {
                    var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                    item.VIN = vehicleDetails.ChassisNo;
                    item.ManufacturerId = vehicleDetails.ManufacturerId;
                    item.Manufacturer = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerPrimaryName).FirstOrDefault();
                }
                else
                {
                    var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(item.VehicleId ?? 0)) ?? new CreateVehicleDefinitionsModel();
                    item.VIN = vehicleDetails.ChassisNo;
                    item.ManufacturerId = vehicleDetails.ManufacturerId;
                    item.Manufacturer = allManufacturers?.Where(i => i.Id == vehicleDetails?.ManufacturerId).Select(s => s.ManufacturerPrimaryName).FirstOrDefault();
                }
            }


            using var workbook = new XLWorkbook();
                try
                {

                    var worksheet = workbook.Worksheets.Add("Monthly Repair Cost Report");
                if (lang == "en")
                {
                    // Add headers
                    worksheet.Cell(1, 1).Value = "WIP";
                    worksheet.Cell(1, 2).Value = "Invoice Date";
                    worksheet.Cell(1, 3).Value = "Invoice Number";
                    worksheet.Cell(1, 4).Value = "Company Code";
                    worksheet.Cell(1, 5).Value = "Customer Name";
                    worksheet.Cell(1, 6).Value = "Total Amount";
                    worksheet.Cell(1, 7).Value = "Total Labours";
                    worksheet.Cell(1, 8).Value = "Sublet";
                    worksheet.Cell(1, 9).Value = "Labours Discount";
                    worksheet.Cell(1, 10).Value = "Total Parts";
                    worksheet.Cell(1, 11).Value = "Parts Discount";
                    worksheet.Cell(1, 12).Value = "Total Lubes";
                    worksheet.Cell(1, 13).Value = "Total Paints";
                    worksheet.Cell(1, 14).Value = "VIN";
                    worksheet.Cell(1, 15).Value = "Plate Number";
                    worksheet.Cell(1, 16).Value = "Franchise";
                    worksheet.Cell(1, 17).Value = "OP Number";
                    worksheet.Cell(1, 18).Value = "OP Name";
                }
                else
                {
                    worksheet.Cell(1, 1).Value = "أمر العمل";
                    worksheet.Cell(1, 2).Value = "تاريخ الفاتورة";
                    worksheet.Cell(1, 3).Value = "رقم الفاتورة";
                    worksheet.Cell(1, 4).Value = "رمز الشركة";
                    worksheet.Cell(1, 5).Value = "الحساب";
                    worksheet.Cell(1, 6).Value = "القسم";
                    worksheet.Cell(1, 7).Value = "معرف العميل";
                    worksheet.Cell(1, 8).Value = "اسم العميل";
                    worksheet.Cell(1, 9).Value = "إجمالي المبلغ";
                    worksheet.Cell(1, 10).Value = "إجمالي الأجور";
                    worksheet.Cell(1, 11).Value = "إجمالي القطع";
                    worksheet.Cell(1, 12).Value = "إجمالي الزيوت";
                    worksheet.Cell(1, 13).Value = "إجمالي الدهانات";
                    worksheet.Cell(1, 14).Value = "رقم تعريف المركبة";
                    worksheet.Cell(1, 15).Value = "رقم اللوحة";
                    worksheet.Cell(1, 16).Value = "الشركة المصنعة";
                    worksheet.Cell(1, 17).Value = "رقم الشخص المسؤول";
                    worksheet.Cell(1, 18).Value = "اسم الشخص المسؤول";
                }
                    int row = 2;
                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.WIP;
                        worksheet.Cell(row, 2).Value = item.InvoiceDate;
                        worksheet.Cell(row, 3).Value = item.InvoiceNumber;
                        worksheet.Cell(row, 4).Value = item.CompanyCode;
                        //worksheet.Cell(row, 5).Value = item.Account;
                        //worksheet.Cell(row, 5).Value = item.Department;
                        worksheet.Cell(row, 5).Value = item.CustomerName;
                        worksheet.Cell(row, 6).Value = item.TotalAmount;
                        worksheet.Cell(row, 7).Value = item.TotalLabours;
                        worksheet.Cell(row, 8).Value = item.TotalAmount;
                        worksheet.Cell(row, 9).Value = item.TotalLaboursDiscount;
                        worksheet.Cell(row, 10).Value = item.TotalParts;
                        worksheet.Cell(row, 11).Value = item.TotalPartsDiscount;
                        worksheet.Cell(row, 12).Value = item.TotalLubes;
                        worksheet.Cell(row, 13).Value = item.TotalPaints;
                        worksheet.Cell(row, 14).Value = item.VIN;
                        worksheet.Cell(row, 15).Value = item.PlateNumber;
                        worksheet.Cell(row, 16).Value = item.Manufacturer;
                        worksheet.Cell(row, 17).Value = item.OPUserName;
                        worksheet.Cell(row, 18).Value = item.OPName;
                        row++;
                    }
                   
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"MonthlyRepairCostBranchWiseReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<AccountDTO> GetAccountDetailsAsync(int wipId)
        {
            try
            {
                return await _apiClient.WIP_GetAccountById(wipId) ?? new AccountDTO();
            }
            catch (Exception ex)
            {
                return new AccountDTO();
            }
        }
        private async Task<List<Manufacturers>> GetMakes()
        {
            var makes = await _vehicleApiClient.GetAllManufacturers(lang);
            return makes;
        }

        #region Consumption Report
        public async Task<IActionResult> ConsumptionReport()
        {
            var isCompanyCenterialized = 1;
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            //var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(16, CompanyId);
            var Types = await _inventoryApiClient.GetAllSubCategoriesAsync();
            ViewBag.Customers = allCustomers?.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CustomerName
            }).ToList() ?? new List<SelectListItem>();

            ViewBag.SubCategories = Types?.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = lang == "en" ? c.primaryName : c.secondaryName

            }).ToList() ?? new List<SelectListItem>();

            var model = new ConsumptionReportViewModel
            {
                Filter = new ConsumptionReportFilterDTO(),
                ReportData = new List<ConsumptionReportDTO>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetConsumptionReport([FromBody] ConsumptionReportFilterDTO filter)
        {
            try
            {
                //filter.TypeId ??= 0;
                filter.SubCategories = (filter.TypeIds == null || filter.TypeIds.Count == 0) ? null : string.Join(",", filter.TypeIds);
                var data = await _apiClient.GetConsumptionReportAsync(filter);
                
                var isCompanyCenterialized = 1;
                
                var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(15, CompanyId);

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.OPName = await GetUserFullNameAsync(item.OPNumber);
                        if (item.MovementId.HasValue)
                        {
                            var move = await _apiClient.GetVehicleMovementByIdAsync(item.MovementId.Value);
                            if (move != null) item.Millage = move.ReceivedMeter;
                        }

                        if (item.CustomerId.HasValue)
                        {
                            var customer = allCustomers.FirstOrDefault(c => c.Id == item.CustomerId);
                            item.Account = customer?.AccountNo.ToString();
                            item.CustomerName = customer?.CustomerName;
                        }
                       
                        item.CompanyCode = (await GetBranchNameAsync(item.workshopId)).ToString();

                        item.VehServiceCode = VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.Code;
                        item.VehServiceDesc = lang == "en" ? VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.PrimaryName : VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.SecondaryName;
                        if(item.TypeId != null)
                        item.Type = (await _apiClient.GetLookupDetailByIdAsync(item.TypeId ?? 0, 15, CompanyId))?.Code;

                        if (item.IsExternal ?? false)
                        {
                            var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                            item.VIN = vehicleDetails.ChassisNo;
                        }
                        else
                        {
                           // var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(item.VehicleId ?? 0)) ?? new CreateVehicleDefinitionsModel();
                           // External vehicle logic might differ, assuming VehicleDefinitions_Find handles it or similar
                            var vehicleDetails = await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0);
                            item.VIN = vehicleDetails?.ChassisNo;
                        }
                        var inventoryItem = await _inventoryApiClient.GetItemByIdAsync(item.ItemId ?? 0);
                        item.ItemName = lang == "en" ? inventoryItem.PrimaryName : inventoryItem.SecondaryName;
                    }
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ReportListConsumption", data);
                }

                var model = new ConsumptionReportViewModel
                {
                    Filter = filter,
                    ReportData = data?.ToList() ?? new List<ConsumptionReportDTO>()
                };

                return View("ConsumptionReport", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PrintConsumptionReport([FromBody] ConsumptionReportFilterDTO filter)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var reportsServiceUrl = _configuration["ReportsService:BaseUrl"] ?? "https://localhost:44332";
                    
                    var response = await httpClient.PostAsJsonAsync($"{reportsServiceUrl}/api/reports/consumption", filter);

                    if (response.IsSuccessStatusCode)
                    {
                        var pdfBytes = await response.Content.ReadAsByteArrayAsync();
                        return File(pdfBytes, "application/pdf", $"ConsumptionReport_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return Json(new { success = false, message = $"Failed to generate PDF report: {errorContent}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportConsumptionReportToExcel([FromBody] ConsumptionReportFilterDTO filter)
        {
            try
            {
                filter.TypeId ??= 0;
                var data = await _apiClient.GetConsumptionReportAsync(filter);
                var isCompanyCenterialized = 1;
                var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(15, CompanyId);

                if (data != null) 
                {
                    foreach (var item in data)
                    {
                        item.OPName = await GetUserFullNameAsync(item.OPNumber);
                        if (item.MovementId.HasValue)
                        {
                            var move = await _apiClient.GetVehicleMovementByIdAsync(item.MovementId.Value);
                            if (move != null) item.Millage = move.ReceivedMeter;
                        }
                        if (item.CustomerId.HasValue)
                        {
                            var customer = allCustomers.FirstOrDefault(c => c.Id == item.CustomerId);
                            item.Account = customer?.AccountNo.ToString();
                            item.CustomerName = customer?.CustomerName;
                        }
                        item.CompanyCode = (await GetBranchNameAsync(item.workshopId)).ToString();
                        item.VehServiceCode = VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.Code;
                        item.VehServiceDesc = lang == "en" ? VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.PrimaryName : VehcileServices?.FirstOrDefault(v => v.Id == item.VehServiceId)?.SecondaryName;
                        if (item.TypeId != null)
                            item.Type = (await _apiClient.GetLookupDetailByIdAsync(item.TypeId ?? 0, 15, CompanyId))?.Code;
                        if (item.IsExternal ?? false)
                        {
                            var vehicleDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                            item.VIN = vehicleDetails.ChassisNo;
                        }
                        else
                        {
                            var vehicleDetails = await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0);
                            item.VIN = vehicleDetails?.ChassisNo;
                        }
                        var inventoryItem = await _inventoryApiClient.GetItemByIdAsync(item.ItemId ?? 0);
                        item.ItemName = lang == "en" ? inventoryItem.PrimaryName : inventoryItem.SecondaryName;
                    }
                }

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Consumption Report");
                if (lang == "en")
                {
                    worksheet.Cell(1, 1).Value = "WIP";
                    worksheet.Cell(1, 2).Value = "Invoice Date";
                    worksheet.Cell(1, 3).Value = "Invoice Number";
                    worksheet.Cell(1, 4).Value = "Company Code";
                    worksheet.Cell(1, 5).Value = "Account";
                    worksheet.Cell(1, 6).Value = "Department";
                    worksheet.Cell(1, 7).Value = "Part No";
                    worksheet.Cell(1, 8).Value = "QTY";
                    worksheet.Cell(1, 9).Value = "Customer Name";
                    worksheet.Cell(1, 10).Value = "Cost";
                    worksheet.Cell(1, 11).Value = "Fran";
                    worksheet.Cell(1, 12).Value = "VIN";
                    worksheet.Cell(1, 13).Value = "Plate Number";
                    worksheet.Cell(1, 14).Value = "Manufacture Year";
                    worksheet.Cell(1, 15).Value = "OP Number";
                    worksheet.Cell(1, 16).Value = "OP Name";
                    worksheet.Cell(1, 17).Value = "Service Code";
                    worksheet.Cell(1, 18).Value = "Service Description";
                }
                else
                {
                    worksheet.Cell(1, 1).Value = "أمر العمل";
                    worksheet.Cell(1, 2).Value = "تاريخ الفاتورة";
                    worksheet.Cell(1, 3).Value = "رقم الفاتورة";
                    worksheet.Cell(1, 4).Value = "رمز الشركة";
                    worksheet.Cell(1, 5).Value = "الحساب";
                    worksheet.Cell(1, 6).Value = "القسم";
                    worksheet.Cell(1, 7).Value = "رقم القطعة";
                    worksheet.Cell(1, 8).Value = "الكمية";
                    worksheet.Cell(1, 9).Value = "اسم العميل";
                    worksheet.Cell(1, 10).Value = "التكلفة";
                    worksheet.Cell(1, 11).Value = "النوع";
                    worksheet.Cell(1, 12).Value = "رقم تعريف المركبة";
                    worksheet.Cell(1, 13).Value = "رقم اللوحة";
                    worksheet.Cell(1, 14).Value = "سنة الصنع";
                    worksheet.Cell(1, 15).Value = "رقم الشخص المسؤول";
                    worksheet.Cell(1, 16).Value = "اسم الشخص المسؤول";
                    worksheet.Cell(1, 17).Value = "رمز الخدمة";
                    worksheet.Cell(1, 18).Value = "وصف الخدمة";
                }

                int row = 2;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.WIP;
                        worksheet.Cell(row, 2).Value = item.InvoiceDate;
                        worksheet.Cell(row, 3).Value = item.InvoiceNumber;
                        worksheet.Cell(row, 4).Value = item.CompanyCode;
                        worksheet.Cell(row, 5).Value = item.Account;
                        worksheet.Cell(row, 6).Value = item.Department;
                        worksheet.Cell(row, 7).Value = item.ItemName;
                        worksheet.Cell(row, 8).Value = item.Quantity;
                        worksheet.Cell(row, 9).Value = item.CustomerName;
                        worksheet.Cell(row, 10).Value = item.TotalCost;
                        worksheet.Cell(row, 11).Value = item.Type;
                        worksheet.Cell(row, 12).Value = item.VIN;
                        worksheet.Cell(row, 13).Value = item.PlateNumber;
                        worksheet.Cell(row, 14).Value = item.ManufactureYear;
                        worksheet.Cell(row, 15).Value = item.OPNumber;
                        worksheet.Cell(row, 16).Value = item.OPName;
                        worksheet.Cell(row, 17).Value = item.VehServiceCode;
                        worksheet.Cell(row, 18).Value = item.VehServiceDesc;
                        row++;
                    }
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ConsumptionReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region WIP Report
        public async Task<IActionResult> WIPReport()
        {
            var isCompanyCenterialized = 1;
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            ViewBag.Customers = allCustomers?.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CustomerName
            }).ToList() ?? new List<SelectListItem>();

             try
            {
                var status = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(8, CompanyId);
                ViewBag.Status = status?.Select(t => new SelectListItem
                {
                    Text = lang == "en" ? t.Code + " - " + t.PrimaryName : t.SecondaryName,
                    Value = t.Id.ToString()
                }).ToList() ?? new List<SelectListItem>();
            }
            catch (Exception ex)
            {
                ViewBag.Status = new List<SelectListItem>();
            }

            var model = new WIPReportViewModel
            {
                Filter = new WIPReportFilterDTO(),
                ReportData = new List<WIPReportDTO>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetWIPReport([FromBody] WIPReportFilterDTO filter)
        {
            try
            {

                var isCompanyCenterialized = 1;
                var data = await _apiClient.GetWIPReportAsync(filter);
                var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(15, CompanyId);
                var statuses = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(8, CompanyId);
                
                foreach (var item in data)
                {

                    //var move = await _apiClient.GetVehicleMovementByIdAsync(item.MovementId ?? 0);
                    item.Account = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.AccountNo).FirstOrDefault().ToString();
                    item.CustomerName = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.CustomerName).FirstOrDefault();
                    item.Status = lang == "en" ? statuses?.Where(c => c.Id == item.StatusId)?.FirstOrDefault()?.PrimaryName: statuses?.Where(c => c.Id == item.StatusId)?.FirstOrDefault()?.SecondaryName;
                    
                     if (!item.IsExternal ?? false)
                    {
                        var vehicleDetails = (await _vehicleApiClient.GetVehicleDetails(item.VehicleId ?? 0,lang)) ?? new VehicleDefinitions();
                        var vehicleVINDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                        item.VIN = vehicleVINDetails.ChassisNo;
                        item.MakeId = vehicleDetails.ManufacturerId;
                        item.ModelId = vehicleDetails.VehicleModelId;
                        item.Make = lang == "en" ? vehicleDetails.RefManufacturers.ManufacturerPrimaryName : vehicleDetails.RefManufacturers.ManufacturerSecondaryName;
                        item.Model = lang == "en" ? vehicleDetails.RefVehicleModels.VehicleModelPrimaryName : vehicleDetails.RefVehicleModels.VehicleModelSecondaryName;
                    }
                    else
                    {
                        var vehicleDetails = (await _vehicleApiClient.GetExternalVehicleDetails(item.VehicleId ?? 0, lang)) ?? new VehicleDefinitions();
                        var vehicleVINDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(item.VehicleId ?? 0)) ?? new CreateVehicleDefinitionsModel();
                        item.VIN = vehicleVINDetails.ChassisNo;
                        item.MakeId = vehicleDetails.ManufacturerId;
                        item.ModelId = vehicleDetails.VehicleModelId;
                        item.Make = lang == "en" ? vehicleDetails.RefManufacturers.ManufacturerPrimaryName : vehicleDetails.RefManufacturers.ManufacturerSecondaryName;
                        item.Model = lang == "en" ? vehicleDetails.RefVehicleModels.VehicleModelPrimaryName : vehicleDetails.RefVehicleModels.VehicleModelSecondaryName;
                    }

                    var wip = await _apiClient.GetWIPByIdAsync(item.WIP);
                    var movementDetails = await GetMovementOperatorsAsync(item.MovementId, wip);
                    item.DueIn = movementDetails.DueInDate;
                    item.DueOut = movementDetails.DueOutDate;
                    if(item.DueOut == null)
                    {
                        item.VehicleInOut = lang == "en" ? "In" : "داخل";
                    }
                    else
                        item.VehicleInOut = lang == "en" ? "out" : "خارج";
                    item.ServiceAdvisor  = await GetUserFullNameAsync(item.ServiceAdvisorId);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return PartialView("_ReportListWIP", data);
                }

                var model = new WIPReportViewModel
                {
                    Filter = filter,
                    ReportData = data?.ToList() ?? new List<WIPReportDTO>()
                };

                return View("WIPReport", model);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportWIPReportToExcel([FromBody] WIPReportFilterDTO filter)
        {
            try
            {
                var isCompanyCenterialized = 1;
                var data = await _apiClient.GetWIPReportAsync(filter);
                var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                var VehcileServices = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(15, CompanyId);
                var statuses = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(8, CompanyId);

                foreach (var item in data)
                {

                    //var move = await _apiClient.GetVehicleMovementByIdAsync(item.MovementId ?? 0);
                    item.Account = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.AccountNo).FirstOrDefault().ToString();
                    item.CustomerName = allCustomers.Where(c => c.Id == item.CustomerId).Select(s => s.CustomerName).FirstOrDefault();
                    item.Status = lang == "en" ? statuses?.Where(c => c.Id == item.StatusId)?.FirstOrDefault()?.PrimaryName : statuses?.Where(c => c.Id == item.StatusId)?.FirstOrDefault()?.SecondaryName;

                    if (!item.IsExternal ?? false)
                    {
                        var vehicleDetails = (await _vehicleApiClient.GetVehicleDetails(item.VehicleId ?? 0, lang)) ?? new VehicleDefinitions();
                        var vehicleVINDetails = (await _vehicleApiClient.VehicleDefinitions_Find(item.VehicleId ?? 0)) ?? new VehicleDefinitions();
                        item.VIN = vehicleVINDetails.ChassisNo;
                        item.MakeId = vehicleDetails.ManufacturerId;
                        item.ModelId = vehicleDetails.VehicleModelId;
                        item.Make = lang == "en" ? vehicleDetails.RefManufacturers.ManufacturerPrimaryName : vehicleDetails.RefManufacturers.ManufacturerSecondaryName;
                        item.Model = lang == "en" ? vehicleDetails.RefVehicleModels.VehicleModelPrimaryName : vehicleDetails.RefVehicleModels.VehicleModelSecondaryName;
                    }
                    else
                    {
                        var vehicleDetails = (await _vehicleApiClient.GetExternalVehicleDetails(item.VehicleId ?? 0, lang)) ?? new VehicleDefinitions();
                        var vehicleVINDetails = (await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicleById(item.VehicleId ?? 0)) ?? new CreateVehicleDefinitionsModel();
                        item.VIN = vehicleVINDetails.ChassisNo;
                        item.MakeId = vehicleDetails.ManufacturerId;
                        item.ModelId = vehicleDetails.VehicleModelId;
                        item.Make = lang == "en" ? vehicleDetails.RefManufacturers.ManufacturerPrimaryName : vehicleDetails.RefManufacturers.ManufacturerSecondaryName;
                        item.Model = lang == "en" ? vehicleDetails.RefVehicleModels.VehicleModelPrimaryName : vehicleDetails.RefVehicleModels.VehicleModelSecondaryName;
                    }

                    var wip = await _apiClient.GetWIPByIdAsync(item.WIP);
                    var movementDetails = await GetMovementOperatorsAsync(item.MovementId, wip);
                    item.DueIn = movementDetails.DueInDate;
                    item.DueOut = movementDetails.DueOutDate;
                    if (item.DueOut == null)
                    {
                        item.VehicleInOut = lang == "en" ? "In" : "داخل";
                    }
                    else
                        item.VehicleInOut = lang == "en" ? "out" : "خارج";
                    item.ServiceAdvisor = await GetUserFullNameAsync(item.ServiceAdvisorId);
                }



                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("WIP Report");
                
                if (lang == "en")
                {
                    worksheet.Cell(1, 1).Value = "WIP";
                    worksheet.Cell(1, 2).Value = "Status";
                    worksheet.Cell(1, 3).Value = "Created Date";
                    worksheet.Cell(1, 4).Value = "Due In";
                    worksheet.Cell(1, 5).Value = "Due Out";
                    worksheet.Cell(1, 6).Value = "Ageing";
                    worksheet.Cell(1, 7).Value = "Customer Name";
                    worksheet.Cell(1, 8).Value = "Total Amount";
                    worksheet.Cell(1, 9).Value = "Total Labours";
                    worksheet.Cell(1, 10).Value = "Total Parts";
                    worksheet.Cell(1, 11).Value = "VIN";
                    worksheet.Cell(1, 12).Value = "Plate Number";
                    worksheet.Cell(1, 13).Value = "Make";
                    worksheet.Cell(1, 14).Value = "Model";
                    worksheet.Cell(1, 15).Value = "Service Advisor";
                    worksheet.Cell(1, 16).Value = "Branch";
                    worksheet.Cell(1, 17).Value = "Vehcile In/Out";
                }
                else
                {
                    worksheet.Cell(1, 1).Value = "أمر العمل";
                    worksheet.Cell(1, 2).Value = "الحالة";
                    worksheet.Cell(1, 3).Value = "تاريخ الانشاء";
                    worksheet.Cell(1, 4).Value = "تاريخ الدخول";
                    worksheet.Cell(1, 5).Value = "تاريخ الخروج";
                    worksheet.Cell(1, 6).Value = "العمر";
                    worksheet.Cell(1, 7).Value = "اسم العميل";
                    worksheet.Cell(1, 8).Value = "إجمالي المبلغ";
                    worksheet.Cell(1, 9).Value = "إجمالي الأجور";
                    worksheet.Cell(1, 10).Value = "إجمالي القطع";
                    worksheet.Cell(1, 11).Value = "رقم الهيكل";
                    worksheet.Cell(1, 12).Value = "رقم اللوحة";
                    worksheet.Cell(1, 13).Value = "الشركة المصنعة";
                    worksheet.Cell(1, 14).Value = "الموديل";
                    worksheet.Cell(1, 15).Value = "مستشار الخدمة";
                    worksheet.Cell(1, 16).Value = "الفرع";
                    worksheet.Cell(1, 17).Value = "المركبة داحل/خارج";
                }

                int row = 2;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.WIP;
                        worksheet.Cell(row, 2).Value = item.Status;
                        worksheet.Cell(row, 3).Value = item.CreatedDate;
                        worksheet.Cell(row, 4).Value = item.DueIn;
                        worksheet.Cell(row, 5).Value = item.DueOut;
                        worksheet.Cell(row, 6).Value = item.Ageing;
                        worksheet.Cell(row, 7).Value = item.CustomerName;
                        worksheet.Cell(row, 8).Value = item.TotalAmount;
                        worksheet.Cell(row, 9).Value = item.TotalLabours;
                        worksheet.Cell(row, 10).Value = item.TotalParts;
                        worksheet.Cell(row, 11).Value = item.VIN;
                        worksheet.Cell(row, 12).Value = item.PlateNumber;
                        worksheet.Cell(row, 13).Value = item.Make;
                        worksheet.Cell(row, 14).Value = item.Model;
                        worksheet.Cell(row, 15).Value = item.ServiceAdvisor;
                        worksheet.Cell(row, 16).Value = item.Branch;
                        worksheet.Cell(row, 16).Value = item.VehicleInOut;
                        row++;
                    }
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"WIPReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion


        private async Task<MovementOperatorsViewModel> GetMovementOperatorsAsync(int? movementId, WIPDTO dto)
        {
            try
            {
                var result = new MovementOperatorsViewModel();

                // current movement (IN)
                VehicleMovement currentMovement = null;

                if (movementId.HasValue && movementId > 0)
                {
                    currentMovement = await _apiClient.GetVehicleMovementByIdAsync(movementId.Value);

                }
                else if (dto?.MovementId > 0)
                {
                    currentMovement = await _apiClient.GetVehicleMovementByIdAsync(dto.MovementId);
                }

                if (currentMovement != null)
                {
                    result.DueInDate = currentMovement.CreatedAt?.ToString("yyyy-MM-dd");
                    result.ReceivedMeter = currentMovement.ReceivedMeter;
                    result.CreatingOperator = await GetUserFullNameAsync(currentMovement.CreatedBy);

                }

                // last movement (OUT)
                if (dto?.VehicleId > 0)
                {
                    var lastMovement = await _apiClient.GetLastVehicleMovementByVehicleIdAsync(dto.VehicleId);
                    if (lastMovement?.MovementOut == true)
                    {
                        result.DueOutDate = lastMovement.CreatedAt?.ToString("yyyy-MM-dd");
                        result.BookedOutOperator = await GetUserFullNameAsync(lastMovement.CreatedBy);
                    }
                }

                // invoicing operator
                if (dto?.Status == 2032 && dto.ClosedBy > 0)
                {
                    result.InvoicingOperator = await GetUserFullNameAsync(dto.ClosedBy);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new MovementOperatorsViewModel();
            }

        }


        public IActionResult PartsSalesSummary()
        {
            ViewBag.AccountTypes = Enum.GetValues(typeof(AccountTypeEnum))
                .Cast<AccountTypeEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();

            var model = new PartsSalesSummaryReport();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> PartsSalesSummary(PartsSummaryFilterDTO filter)
        {
            var data = await _apiClient.GetPartsSummaryReport(filter);
            var companyInfo = await _erpClient.GetCompanyById(CompanyId);
            var CompanyName = lang=="en" ? companyInfo.CompanyPrimaryName : companyInfo.CompanySecondaryName;

             Byte[] Logo = companyInfo.Img;

            var bytes = await _reportsServiceApiClient.PartsSalesSummaryReportReportAsync(data, CompanyName, Logo);

            Response.Headers["Content-Disposition"] = "inline; filename=Wip.pdf";
            return File(bytes, "application/pdf");



        }



    }
}
