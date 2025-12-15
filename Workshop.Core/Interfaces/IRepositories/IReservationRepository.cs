using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IReservationRepository
    {
        Task<int> InsertReservation(ReservationDTO reservationDTO);
        Task<IEnumerable<ReservationListItemDTO>> GetAllReservationsAsync(ReservationFilterDTO filterDTO);
        Task<int> UpdatedReservationStatus(ReservationStatusUpdateDTO reservationStatusUpdate);
        Task<IEnumerable<ReservationListItemDTO>> GetAllActiveReservationsFilteredAsync(DateTime? dateFrom, DateTime? dateTo);
        Task<IEnumerable<ReservationListItemDTO>> GetReservationsByIdsAsync(string ids);
        Task<int> CheckIfVehicleHasActiveReservation(int vehicleId);
        Task<int> UpdateReservation(ReservationDTO reservationDTO);
    }
}
