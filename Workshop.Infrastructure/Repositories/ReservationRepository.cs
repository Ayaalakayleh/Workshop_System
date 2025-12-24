using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.Interfaces.IRepositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Workshop.Infrastructure.Repositories
{
    public class ReservationRepository: IReservationRepository
    {
        private readonly Database _database;
        public ReservationRepository(Database database)
        {
            _database = database;
        }
        public async Task<IEnumerable<ReservationListItemDTO>> GetAllReservationsAsync(ReservationFilterDTO filterDTO)
        {
            var parameters = new
            {
                VehicleId = filterDTO.VehicleId,
                Status = filterDTO.Status,
                DateFrom = filterDTO.DateFrom,
                DateTo = filterDTO.DateTo,
                PageNumber = filterDTO.PageNumber,
                PageSize = filterDTO.PageSize,
                ChassisId = filterDTO.ChassisId,
                CustomerId = filterDTO.CustomerId
            };

            var result = await _database.ExecuteGetAllStoredProcedure<ReservationListItemDTO>("M_Reservations_GetFiltered", parameters);
            return result;
        }

        public async Task<int> InsertReservation(ReservationDTO reservationDTO)
        {
            var parameter = new
            {
                Date = reservationDTO.Date,
                VehicleId = reservationDTO.VehicleId,
                Plate_Number = reservationDTO.PlateNumber,
                Start_Time = reservationDTO.Start_Time,
                End_Time = reservationDTO.End_Time,
                Duration = reservationDTO.Duration,
                Status = reservationDTO.Status,
                CreatedBy = reservationDTO.CreatedBy,
                Description = reservationDTO.Description,
                Chassis = reservationDTO.Chassis,
                CompanyId = reservationDTO.CompanyId,
                CustomerName = reservationDTO.CustomerName,
                ChassisId = reservationDTO.ChassisId,
                VehicleTypeId = reservationDTO.vehicleTypeId,
                customerId = reservationDTO.CustomerId

            };

            var result = await _database.ExecuteAddStoredProcedure<int>("M_Reservations_Insert", parameter);
            return result;
        }
        public async Task<int> UpdatedReservationStatus(ReservationStatusUpdateDTO reservationStatusUpdate)
        {
            var parameters = new
            {
                ReservationId = reservationStatusUpdate.ReservationId,
                StatusId = reservationStatusUpdate.StatusId,
                UpdatedBy = reservationStatusUpdate.UpdatedBy
            };
            var result = await _database.ExecuteUpdateProcedure<int>("M_Reservation_UpdateStatus", parameters);
            return result;


        }
        public async Task<IEnumerable<ReservationListItemDTO>> GetAllActiveReservationsFilteredAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            var parameters = new
            {
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var result = await _database.ExecuteGetAllStoredProcedure<ReservationListItemDTO>(
                "M_Reservations_GetAllActive",
                parameters
            );

            return result;
        }
        public async Task<IEnumerable<ReservationListItemDTO>> GetReservationsByIdsAsync(string ids)
        {
            var parameters = new { Ids = ids };

            var result = await _database.ExecuteGetAllStoredProcedure<ReservationListItemDTO>(
                "M_Reservations_GetByIds",
                parameters
            );

            return result;
        }
        public async Task<int> CheckIfVehicleHasActiveReservation(int vehicleId)
        {
            var parameters = new
            {
                VehicleId = vehicleId
            };

            var result = await _database.ExecuteGetByIdProcedure<int>(
                "M_Reservations_CheckVehicleCanReserve",
                parameters
            );

            return result;
        }
        public async Task<int> UpdateReservation(ReservationDTO reservationDTO)
        {
            var parameters = new
            {
                Id = reservationDTO.Id,
                Date = reservationDTO.Date,
                VehicleId = reservationDTO.VehicleId,
                Plate_Number = reservationDTO.PlateNumber,
                Start_Time = reservationDTO.Start_Time,
                End_Time = reservationDTO.End_Time,
                Duration = reservationDTO.Duration,
                Status = reservationDTO.Status,
                UpdatedBy = reservationDTO.CreatedBy,
                Description = reservationDTO.Description,
                Chassis = reservationDTO.Chassis,
                CustomerName = reservationDTO.CustomerName,
                ChassisId = reservationDTO.ChassisId,
                VehicleTypeId = reservationDTO.vehicleTypeId

            };

            var result = await _database.ExecuteUpdateProcedure<int>(
                "M_Reservations_Update",
                parameters
            );

            return result;
        }






    }
}
