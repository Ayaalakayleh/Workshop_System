using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Core.Interfaces.IServices;

namespace Workshop.Core.Services
{
    public class ReservationService: IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        
            public ReservationService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }
        public async Task<IEnumerable<ReservationListItemDTO>> GetAllReservationsAsync(ReservationFilterDTO filterDTO)
        {
            return await _reservationRepository.GetAllReservationsAsync(filterDTO);

        }
        public async Task<int> InsertReservation(ReservationDTO reservationDTO)
        {
            return await _reservationRepository.InsertReservation(reservationDTO);

        }
        public async Task<int> UpdatedReservationStatus(ReservationStatusUpdateDTO reservationStatusUpdate)
        {
            return await _reservationRepository.UpdatedReservationStatus(reservationStatusUpdate);
        }
        public async Task<IEnumerable<ReservationListItemDTO>> GetAllActiveReservationsFilteredAsync(DateTime? dateFrom, DateTime? dateTo)
        {
            return await _reservationRepository.GetAllActiveReservationsFilteredAsync(dateFrom, dateTo);
        }
        public async Task<IEnumerable<ReservationListItemDTO>> GetReservationsByIdsAsync(string ids)
        {
            return await _reservationRepository.GetReservationsByIdsAsync(ids);
        }
        public async Task<int> CheckIfVehicleHasActiveReservation(int vehicleId)
        {
            return await _reservationRepository.CheckIfVehicleHasActiveReservation(vehicleId);
        }
        public async Task<int> UpdateReservation(ReservationDTO reservationDTO)
        {
            return await _reservationRepository.UpdateReservation(reservationDTO);
        }
    }
}
