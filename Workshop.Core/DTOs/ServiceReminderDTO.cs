using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.Vehicle;

namespace Workshop.Core.DTOs
{
    using System;
    using System.Collections.Generic;

    public enum ServiceReminderTimeUnitEnum
    {
        //[LocalizedDescription("Days", typeof(Default))]
        Days = 1,
        //[LocalizedDescription("Weeks", typeof(Default))]
        Weeks = 2,
        //[LocalizedDescription("Months", typeof(Default))]
        Months = 3,
       // [LocalizedDescription("Years", typeof(Default))]
        Years = 4,
    }
    public class ReminderStatus
    {

        public ReminderStatus(string statusName, int statusValue)
        {
            StatusName = statusName;
            StatusValue = statusValue;
        }

        public string StatusName { get; set; }
        public int StatusValue { get; set; }
    }
    public class ServiceScheduleModel
    {
        public int VehicleId { get; set; }
        public int DamageId { get; set; }
        public decimal Meter { get; set; }
        public DateTime Date { get; set; }
    }

    public class ServiceReminderDue
    {
        public int StatusCount { get; set; }
        public int ReminderStatus { get; set; }
    }
    public class ServiceReminderDTO
    {
        public int Id { get; set; }
        public int? ManufacturingYear { get; set; }
        public List<Manufacturers> ColManufacturers { get; set; } = new List<Manufacturers>();
        public int? ManufacturerId { get; set; }
        public int? VehicleGroupId { get; set; }
        public int? VehicleModelId { get; set; }
        public string? VehicleName { get; set; } 
        public string? ServiceName { get; set; } 
        public int? VehicleId { get; set; }
        public Int64? ItemId { get; set; }
        public List<VehicleNams> vehicleNams { set; get; } = new List<VehicleNams>();
        public List<Item> Services { get; set; } = new List<Item>();
        public int Repates { get; set; }
        public int TimeInterval { get; set; }
        public int TimeIntervalUnit { get; set; }
        public int TimeDue { get; set; }
        public int TimeDueUnit { get; set; }
        public int PrimaryMeterInterval { get; set; }
        public int PrimaryMeterDue { get; set; } // in ServiceReminders
        public bool IsManually { get; set; }
        public DateTime? ManualDate { get; set; } 
        public int ManualPrimaryMeter { get; set; }
        public bool HasNotification { get; set; }
        public DateTime? LastCompleted { get; set; } = new DateTime();
        public int? ReminderStatus { get; set; }
        public string ReminderStatusPrimaryName { get; set; } = String.Empty;
        public string ReminderStatusSecondaryName { get; set; } = String.Empty;
        public string ReminderStatusName { get; set; } = String.Empty;
        public string ServiceTimeInterval { get; set; } = String.Empty;
        public string ServiceMeterInterval { get; set; } = String.Empty;
        public decimal CurrentMeter { get; set; }
        public List<ServiceRemindersSchedule> ServiceRemindersSchedules { get; set; } = new List<ServiceRemindersSchedule>();
        public int TotalPages { get; set; }
        public DateTime? StartDate { get; set; }
        public decimal StartMeter { get; set; }
        public DateTime? NextDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public decimal NextPrimaryMeter { get; set; }
        public decimal NextDuePrimaryMeter { get; set; } // // in ServiceRemindersSchedule
        public bool UseSameStart { get; set; } = true; // For Edit
        public List<int> NotificationsGroupId { get; set; } = new List<int>();
        public string NotificationsGroup { get; set; } = String.Empty;
        public DateTime? Date { get; set; }
        public string? NotificationsGroupIdString { get; set; } 

    }
    public class CreateServiceReminderDTO : ServiceReminderDTO
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } 

    }
    public class UpdateServiceReminderDTO : ServiceReminderDTO
    {
        public int? ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
    }
    public class DeleteServiceReminderDTO : ServiceReminderDTO
    {
        public bool IsDeleted { get; set; }
    }
    public class GetServiceReminderDTO : ServiceReminderDTO
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class ServiceRemindersSchedule
    {
        public int Id { get; set; }
        public int ServiceReminderId { get; set; }
        public DateTime? Date { get; set; }
        public decimal? PrimaryMeter { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? DuePrimaryMeter { get; set; }
    }

    public enum ServiceReminderStatusEnum
    {
        Scheduled = 1,
        DueSoon = 2,
        Overdue = 3,
    }

}



