using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.IO;
using Workshop.Core.DTOs.General;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly IWebHostEnvironment _env;
        protected readonly IMemoryCache cache;
        protected int CompanyId { get; private set; }
        protected int BranchId { get; private set; }
        protected int UserId { get; private set; }
        protected int CurrencyId { get; private set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            CompanyId = HttpContext.Session.GetInt32("CompanyId") ?? 0;
            BranchId = HttpContext.Session.GetInt32("BranchId") ?? 0;
            UserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var branchInfoStr = HttpContext.Session.GetString("BranchInfo");
            CurrencyId = !string.IsNullOrEmpty(branchInfoStr)
                ? System.Text.Json.JsonSerializer.Deserialize<CompanyBranch>(branchInfoStr)?.CurrencyIDH ?? 0
                : 0;
            base.OnActionExecuting(context);
        }
        public BaseController(IMemoryCache memoryCache, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _env = environment;
            cache = memoryCache;
        }

        public ActionResult GetImage(string Name, int D, string BasePath = "", bool IsExternal = false)
        {
            try
            {
                string imageDirectory = string.Empty;
                string defaultimage = string.Empty;
                string type = string.Empty;

                switch (D)
                {
                    case 1:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("VehicleImage", BasePath) : "VehicleImage";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 3:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("MovementImages", BasePath) : "MovementImages";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 4:
                        imageDirectory = "Logos";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 5:
                        imageDirectory = "CustomerProfile";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 6:
                        imageDirectory = "CustomerDocuments";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 8:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("VehicleAccidents", BasePath) : "VehicleAccidents";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 9:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("VehicleImage", BasePath) : "VehicleImage";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 11:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("Damage", BasePath) : "Damage";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 12:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("DamageReport", BasePath) : "DamageReport";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 13:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("ClaimDocument", BasePath) : "ClaimDocument";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 14:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("ChaufferDocuments", BasePath) : "ChaufferDocuments";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 15:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("ExternalWorkshopInvoice", BasePath) : "ExternalWorkshopInvoice";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 16:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("ExternalVehicles", BasePath) : "ExternalVehicles";
                        defaultimage = "wwwroot/images/default_car_260.jpeg";
                        break;
                    case 17:
                        imageDirectory = !string.IsNullOrEmpty(BasePath) ? Path.Combine("Uploads", BasePath) : "Uploads";
                        defaultimage = "wwwroot/images/default_user2.png";
                        break;
                }

                string root = "";
                if (!IsExternal)
                {
                    root = _configuration["FileUpload:DirectoryReadPath"] ?? "wwwroot/uploads/";
                }
                else
                {
                    root = _configuration["VehicleApplicationPath"] ?? "";
                }

                if (string.IsNullOrEmpty(Name))
                {
                    var defaultPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), defaultimage);
                    if (System.IO.File.Exists(defaultPhysicalPath))
                    {
                        return PhysicalFile(defaultPhysicalPath, GetMimeType(defaultPhysicalPath));
                    }
                    return NotFound();
                }

                var path = Path.Combine(root, imageDirectory, Name);

                if (!IsExternal && !Path.IsPathRooted(path))
                {
                    path = Path.Combine(Directory.GetCurrentDirectory(), path);
                }

                if (!IsExternal)
                {
                    if (!System.IO.File.Exists(path))
                    {
                        var defaultPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), defaultimage);
                        if (System.IO.File.Exists(defaultPhysicalPath))
                        {
                            return PhysicalFile(defaultPhysicalPath, GetMimeType(defaultPhysicalPath));
                        }
                        return NotFound();
                    }
                }

                if (!System.IO.File.Exists(path))
                {
                    var defaultPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), defaultimage);
                    if (System.IO.File.Exists(defaultPhysicalPath))
                    {
                        return PhysicalFile(defaultPhysicalPath, GetMimeType(defaultPhysicalPath));
                    }
                    return NotFound();
                }

                return PhysicalFile(path, GetMimeType(path), Name);
            }
            catch
            {
                var dummyPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/default_car_260.jpeg");
                if (System.IO.File.Exists(dummyPath))
                {
                    return PhysicalFile(dummyPath, "image/png");
                }
                return NotFound();
            }
        }

        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

    }
}
