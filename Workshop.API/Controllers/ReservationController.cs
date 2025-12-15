using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }
        [HttpPost("GetAllReservationsAsync")]
        public async Task<IActionResult> GetAllReservationsAsync(ReservationFilterDTO filterDTO)
        {
            var reservations = await _reservationService.GetAllReservationsAsync(filterDTO);

            //if (reservations == null)
            //    return NotFound("No reservations found.");

            return Ok(reservations);
        }

        [HttpPost("InsertReservation")]
        public async Task<IActionResult> InsertReservation(ReservationDTO reservationDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newId = await _reservationService.InsertReservation(reservationDTO);
            return Ok(new { ReservationId = newId });
        }
        [HttpPut("UpdatedReservationStatus")]
        public async Task<IActionResult> UpdatedReservationStatus(ReservationStatusUpdateDTO reservationStatusUpdateDTO)
        {
            var result = await _reservationService.UpdatedReservationStatus(reservationStatusUpdateDTO);
            return Ok(result);

        }
        [HttpGet("GetAllActiveReservationsFilteredAsync")]
        public async Task<IActionResult> GetAllActiveReservationsFilteredAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            var result = await _reservationService.GetAllActiveReservationsFilteredAsync(dateFrom, dateTo);
            return Ok(result);
        }
        [HttpGet("GetReservationsByIdsAsync")]
        public async Task<IActionResult> GetReservationsByIdsAsync(string ids)
        {
            var result = await _reservationService.GetReservationsByIdsAsync(ids);
            return Ok(result);

        }
        [HttpGet("VehicleHasActiveReservation")]
        public async Task<IActionResult> CheckIfVehicleHasActiveReservation(int vehicleId)
        {
            var result = await _reservationService.CheckIfVehicleHasActiveReservation(vehicleId);
            return Ok(result);
        }
        [HttpPut("UpdateReservation")]
        public async Task<IActionResult> UpdateReservation(ReservationDTO reservationDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedId = await _reservationService.UpdateReservation(reservationDTO);
            return Ok(new { ReservationId = updatedId });
        }


    }
}
