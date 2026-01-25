using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;
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
        public readonly string lang;
        public ReportsController(
            WorkshopApiClient apiClient,
            IConfiguration configuration,
            ERPApiClient eRPApiClient,
            AccountingApiClient accountingApiClient,
            VehicleApiClient vehicleApiClient,
            IWebHostEnvironment env) : base(null, configuration, env)
        {
            _apiClient = apiClient;
            _erpClient = eRPApiClient;
            _accountingApiClient = accountingApiClient;
            _vehicleApiClient = vehicleApiClient;
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
    }
}
