using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            ERPApiClient eRPApiClient, AccountingApiClient accountingApiClient, IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
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
            var technicians = await _workshopapiClient.GetTechniciansDDL(BranchId);
            ViewBag.Technicians = technicians.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = lang == "ar" ? t.SecondaryName : t.PrimaryName
            }).ToList();


            var filter = new ReservationFilterDTO();
            var reservations = await _workshopapiClient.GetAllReservationsAsync(filter);
            ViewBag.Reservaions = reservations;


            List<VehicleNams> vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
            ViewBag.Vehicles = new SelectList(vehicleNams, "id", "VehicleName");

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

            List<VehicleNams> vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
/*            ViewBag.Companies = await _eRPApiClient.GetCompaniesInfo(100);
*/            ViewBag.Vehicles = new SelectList(vehicleNams, "id", "VehicleName");
            var isCompanyCenterialized = 1;
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
            ViewBag.Customers = allCustomers;
            var chassisList = await _vehicleApiClient.GetChassiDDL(CompanyId);
            ViewBag.Chassis = new SelectList(chassisList, "Id", "ChassisNo");


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
                       var agreements = await _vehicleApiClient.GetAgreementbyVehicleId(id);
                string customerName = agreements?.FirstOrDefault()?.CustomerName ?? "";
                return Json(new
                       {
                           success = true,
                           data = new
                           {
                               Vehicle = vehicle,
                               CustomerName = customerName
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
                var reservations = await _workshopapiClient.GetAllReservationsAsync(reservationFilter);
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
               /* var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, isCompanyCenterialized, lang);
                foreach (var r in result)
                {
                    var customer = allCustomers.FirstOrDefault(c => c.Id == r.CompanyId);
                    r.CompanyName = customer?.CustomerName;   
                }*/

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


    }
}
