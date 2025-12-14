
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Infrastructure;

namespace Workshop.Core.DTOs
{
    public class JobCardDTO
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int workshopId { get; set; }
        public Guid MasterId { get; set; }
        public int Status { get; set; }

        // Fix nullability to match table
        public DateTime? IssueDate { get; set; }
        public DateTime? StartDate { get; set; } // Changed to nullable
        public TimeSpan? startTime { get; set; } // Changed to nullable
        public TimeSpan? CompleteTime { get; set; } // Changed to nullable
        public decimal? OdoMeter { get; set; } // Changed to nullable
        public decimal? ReceivedMeter { get; set; } // Changed to nullable
        public decimal? TotalParts { get; set; } // Changed to nullable
        public decimal? TotalTechnicians { get; set; } // Changed to nullable
        public decimal? Discount { get; set; } // Changed to nullable
        public decimal? Tax { get; set; } // Changed to nullable
        public decimal? TotalOrder { get; set; } // Changed to nullable
        public DateTime? CompleteDate { get; set; } // Changed to nullable

        public int Priority { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Fix property name typos
        public int? ModifyBy { get; set; } // Fixed spelling
        public DateTime? ModifyAt { get; set; } // Fixed spelling

        public string Description { get; set; }
        public string VehicleName { get; set; }
        public TechnicianDTO Technician { get; set; }
        public VehicleDefinitions RefVehicle { get; set; }
        public List<TechnicianDTO> oLTechnicians { get; set; }
        public List<Item> Services { get; set; }
        public List<Item> Parts { get; set; }
        public List<Category> Categories { get; set; }
        public List<VehicleNams> vehicleNams { get; set; }
        public List<MWorkOrder> workOrders { get; set; }
        public Int64 JobCardNo { get; set; }
        public bool IsExternal { get; set; }
        public bool OilChange { get; set; }
        public DateTime? NextOilChangeDate { get; set; }
        public decimal OilMeter { get; set; }
        public decimal OriginalMeter { get; set; }
        public int? WorkOrderId { get; set; }
    }
}
