using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Core.Interfaces.IServices;
using Workshop.Core.Services;
using Workshop.Domain.Entities;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class ReservationsController : BaseController
    {
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly WorkshopApiClient _workshopapiClient;
        private readonly ERPApiClient _eRPApiClient;
        private readonly AccountingApiClient _accountingApiClient;

        private readonly string lang; 

        public ReservationsController(VehicleApiClient vehicleApiClient, WorkshopApiClient workshopApiClient,
            ERPApiClient eRPApiClient, AccountingApiClient accountingApiClient, IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache,configuration, env)
        {
            _vehicleApiClient = vehicleApiClient;
            _workshopapiClient = workshopApiClient;
            _eRPApiClient = eRPApiClient;
            _accountingApiClient = accountingApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }
       
        [CustomAuthorize(Permissions.Reservations.View)]
        public async Task<IActionResult> Index()
        {
            //var technicians = await _workshopapiClient.GetTechniciansDDL(BranchId);
            //ViewBag.Technicians = technicians.Select(t => new SelectListItem
            //{
            //    Value = t.Id.ToString(),
            //    Text = lang == "ar" ? t.SecondaryName : t.PrimaryName
            //}).ToList();

            var cacheKey = string.Format(CacheKeys.Users, CompanyId);

            if (!cache.TryGetValue(cacheKey, out List<User> users))
            {
                users = await _eRPApiClient.Get_UsersByCompanyId(CompanyId);

                cache.Set(
                    cacheKey,
                    users,
                    TimeSpan.FromHours(24)
                );
            }

            ViewBag.Users = users;
            var filter = new ReservationFilterDTO();
            var reservations = await _workshopapiClient.GetAllReservationsAsync(filter);
            foreach (var r in reservations)
            {
                r.UserName = users
                    .FirstOrDefault(u => u.UserID == r.CreatedBy)
                    ?.Name;
            }
            ViewBag.Reservaions = reservations;

            var vehicleCustomers = await _vehicleApiClient.Get_CustomerInformation(BranchId, "en", null);
            ViewData["vehicleCustomers"] = vehicleCustomers;



            var internalVehicles = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            var externalVehicles = await _vehicleApiClient.GetExteralVehicleName(lang);

            // Pass raw data to view for JavaScript
            ViewBag.InternalVehicles = internalVehicles;
            ViewBag.ExternalVehicles = externalVehicles;

            return View();
        }
        [HttpPost]
        [CustomAuthorize(Permissions.Reservations.Create)]
        public async Task<JsonResult> InsertReservation([FromBody] ReservationDTO reservationDTO)
        {
            try
            {
                var isActiveReservation = await _workshopapiClient.CheckIfVehicleHasActiveReservation(reservationDTO.VehicleId);
                if(isActiveReservation == 1)
                {
                    return Json(new
                    {
                        isSuccess = true,
                        isActive = true
                    });
                }
                reservationDTO.CreatedBy = UserId;
                reservationDTO.CompanyId = CompanyId;
                var result = await _workshopapiClient.InsertReservationAsync(reservationDTO);

                if (result <= 0)
                {
                    throw new InvalidOperationException("Failed to insert the reservation. No ID was returned.");
                }

                return Json(new
                {
                    isSuccess = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while inserting the reservation.", ex);
            }
        }



        [HttpGet]
        public async Task<IActionResult> LoadScheduleDialog()
        {

            //List<VehicleNams> vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            /*            ViewBag.Companies = await _eRPApiClient.GetCompaniesInfo(100);
            */
            ViewBag.Vehicles = new SelectList(Enumerable.Empty<SelectListItem>());
            var isCompanyCenterialized = 1;
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            ViewBag.Customers = allCustomers;
            ViewBag.VehicleType = Enum.GetValues(typeof(VehicleTypeId))
             .Cast<VehicleTypeId>()
             .Select(v => new SelectListItem
             {
                 Text = v.ToString(),        // "Internal", "External", etc.
                 Value = ((int)v).ToString() // "1", "2", "3"
             })
             .ToList();
            var vehicleCustomers = await _vehicleApiClient.Get_CustomerInformation(BranchId, "en", null);
            ViewData["vehicleCustomers"] = vehicleCustomers;



            var model = new WIPSChedule();
            return PartialView("_ScheduleModal", model);
        }

        [HttpGet]
        [CustomAuthorize(Permissions.Reservations.View)]
        public async Task<JsonResult> GetVehicleDefentionById(int id, string lang)
               {
                   try
                   {
                       var vehicle = await _vehicleApiClient.VehicleDefinitions_Find(id);
                return Json(new
                       {
                           success = true,
                           data = new
                           {
                               Vehicle = vehicle,
                           }
                });
                   }
                   catch (Exception ex)
                   {
                       return Json(new { success = false, message = ex.Message });
                   }
               }
        /*[HttpGet]
        public async Task<JsonResult> GetAvailableTechnicians(DateTime date, decimal duration)
        {
            try
            {
                var avaliableTime = await _workshopapiClient.GetAvailableTechniciansAsync(
                   date,
                   duration
               );
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        AvaliableTime = avaliableTime
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }
*/
       /* public async Task<JsonResult> GetStanderAndMenuRtsCodes()
        {
            try
            {
                var standardRtsCodes = await _workshopapiClient.GetAllRTSCodesDDLAsync();
                var menuRtsCodes = await _workshopapiClient.GetAllMenuDDL();



                return Json(new { success = true, data = standardRtsCodes });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }*/
        public async Task<JsonResult> UpdatedReservationStatus(ReservationStatusUpdateDTO reservationStatusUpdateDTO)
        {
            try
            {
                var result = await _workshopapiClient.UpdatedReservationStatusAsync(reservationStatusUpdateDTO);
                return Json(new { success = true, data = result });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }


        }
        [HttpPost]
        public async Task<IActionResult> GetAllReservationsFiltered([FromBody] ReservationFilterDTO reservationFilter)
        {
            try
            {
                var cacheKey = string.Format(CacheKeys.Users, CompanyId);

                if (!cache.TryGetValue(cacheKey, out List<User> users))
                {
                    users = await _eRPApiClient.Get_UsersByCompanyId(CompanyId);

                    cache.Set(
                        cacheKey,
                        users,
                        TimeSpan.FromHours(24)
                    );
                }

                var reservations = await _workshopapiClient.GetAllReservationsAsync(reservationFilter);
                foreach (var r in reservations)
                {
                    r.UserName = users
                        .FirstOrDefault(u => u.UserID == r.CreatedBy)
                        ?.Name;
                }
                return PartialView("_ReservationsTablePartial", reservations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllReservationsFilteredJSON(DateTime dateFrom, DateTime? dateTo)
        {
            try
            {
                var reservations = await _workshopapiClient.GetAllActiveReservationsFilteredAsync(dateFrom, dateTo);
                return Json(new
                {
                    sucess = true,
                    data = reservations
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        /* [HttpGet]
         public async Task<JsonResult> GetTechniciansSchedule(DateTime date, DateTime? DateTo)
         {
             try
             {
                 var techWorkingHours = await _workshopapiClient.GetTechniciansSchedule(date, DateTo);
                 return Json(new { sucess = true, data = techWorkingHours });
             }
             catch(Exception ex)
             {
                 return Json(new { success = false, message = ex.Message });
             }


         }*/
        [HttpGet]
        [CustomAuthorize(Permissions.Reservations.View)]
        public async Task<JsonResult> GetReservationsByIds(string ids)
        {
            try
            {
                var isCompanyCenterialized = 1;
                var result = await _workshopapiClient.GetReservationsByIdsAsync(ids);

                var internalVehicles = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
                var externalVehicles =  await _vehicleApiClient.GetChassiDDL(CompanyId,2);

                foreach (var res in result) {
                if(res.CustomerId > 0)
                    {
                        var customerinfo = await _vehicleApiClient.GetCustomerData(res.CustomerId ?? 0);
                        res.CustomerName = customerinfo.CustomerPrimaryName;
                    }
                if(res.vehicleTypeId == 1) // => Internal :)
                    {
                        res.Chassis = internalVehicles.FirstOrDefault(x => x.id == res.VehicleId).ChassisNo;
                    }
                    else
                    {
                        res.Chassis = externalVehicles.FirstOrDefault(x => x.Id == res.VehicleId).ChassisNo;

                    }

                }

                return Json(new
                {
                    isSuccess = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        public async Task<JsonResult> VehicleDefinitionsGetByChassisNo(string chassisNo)
        {
            try
            {
                var result = await _vehicleApiClient.VehicleDefinitionsGetByChassisNo(chassisNo);

                if (result == null)
                    return Json(new { success = false, message = "Vehicle not found." });

                return Json(new
                {
                    success = true,
                    plateNumber = result.PlateNumber,
                    vehicleId = result.Id
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public async Task<IActionResult> VehicleList(int VehicleTypeId)
        {
            var filter = new VehicleAdvancedFilter();
            var colVehicleDefinitions = new List<SelectListItem>();
            string language = "en";
            filter.VehicleTypeId = VehicleTypeId;
            filter.CompanyId = CompanyId;


            if (filter.VehicleTypeId == 1) // internal
            {
                colVehicleDefinitions = (await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId))
                    .Select(item => new SelectListItem
                    {
                        Text = lang == "en" ? item.VehicleName : item.VehicleName,
                        Value = item.id.ToString()
                    })
                    .ToList();
            }
            else if (filter.VehicleTypeId == 2) // external
            {
                colVehicleDefinitions = (await _vehicleApiClient.GetExteralVehicleName(lang))
                .Select(item => new SelectListItem
                {
                    Text = lang == "en" ? item.VehicleName : item.VehicleName,
                    Value = item.id.ToString()
                })
                .ToList();
            }

            else
            {
                colVehicleDefinitions = new List<SelectListItem>();
            }

            return Json(colVehicleDefinitions);
        }
        [HttpGet]
        public async Task<IActionResult> GetChassisByVehicleType(int vehicleTypeId)
        {
            try
            {
                var chassisList = await _vehicleApiClient.GetChassiDDL(
              CompanyId,
              vehicleTypeId
          );

                return Json(
                    chassisList.Select(c => new
                    {
                        id = c.Id,
                        text = c.ChassisNo
                    })
                );
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });

            }

        }

        public async Task<JsonResult> UpdateReservation([FromBody] ReservationDTO reservationDTO)
        {
            try
            {
                if (reservationDTO.VehicleId == 0 || reservationDTO.ChassisId == 0) {
                return Json(new { isSuccess = false, message = "Please check the data passed" });
                }
                var result = await _workshopapiClient.UpdateReservation(reservationDTO);
                return Json(new { isSuccess = true, message = result });

            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }

        public async Task<JsonResult> GetOpenAgreementInfoByCustomerIdOrVehicleId(int? customerId, int? vehicleId)
        {   
            if (!customerId.HasValue && !vehicleId.HasValue)
            {
                return Json(new
                {
                    isSuccess = false,
                    message = "CustomerId or VehicleId is required"
                });
            }

            try
            {
                    var data = await _vehicleApiClient.M_GetOpenAgreementByVehicleOrCustomer(customerId, vehicleId);

                    return Json(new
                {
                    isSuccess = true,
                    data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet]
        public async Task<bool> HasRecall(string chassis)
        {
            var result = await _workshopapiClient.GetActiveRecallsByChassis(chassis);
            if(result != null)
            return result.HasActiveRecall;

            return false;
        }


    }
}
