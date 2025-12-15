using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public abstract class BaseReservationDTO
    {
        public DateTime Date { get; set; }

        public int VehicleId { get; set; }
        public string PlateNumber { get; set; }
        public string Description { get; set; }
        public string Chassis { get; set; }
        public int CompanyId { get; set; }

        public TimeSpan Start_Time { get; set; }

        public TimeSpan End_Time { get; set; }

        public decimal Duration { get; set; }

        public int? Status { get; set; }
        //public List<TechnicianDTO> Tech { get; set; }
        public string CustomerName { get; set; }
        public int ChassisId { get; set; }
        public int vehicleTypeId { get; set; }

    }
    public class ReservationDTO: BaseReservationDTO

    {
        public int? Id { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }


    }
    public class ReservationFilterDTO
    {

        public int? VehicleId { get; set; }
        public int? Status { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }


        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string? Chassis { get; set; }


    }
    public class ReservationListItemDTO
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int VehicleId { get; set; }
        public string Plate_Number { get; set; }
        public TimeSpan Start_Time { get; set; }
        public TimeSpan End_Time { get; set; }
        public decimal Duration { get; set; }
        public int Status { get; set; }
        public string StatusPrimaryName { get; set; }
        public string StatusSecondaryName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalPages { get; set; }
        public string Chassis { get; set; }
        public string Description { get; set; }
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string CustomerName { get; set; }
        public int ChassisId { get; set; }
        public int vehicleTypeId { get; set; }



    }
    public class ReservationStatusUpdateDTO
    {
        public int ReservationId { get; set; }
        public int StatusId { get; set; }
        public int UpdatedBy { get; set; }
    }

}
