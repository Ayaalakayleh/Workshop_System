using Microsoft.Extensions.DependencyInjection;
using Workshop.Core.Interfaces.IServices;
using Workshop.Core.Services;
using Workshop.Infrastructure;

namespace Workshop.Core
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            // Add services
            services.AddScoped<IDTechnicianService, DTechnicianService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<ITechnicianWorkScheduleService, TechnicianWorkScheduleService>();
            services.AddScoped<IRTSCodeService, RTSCodeService>();
            services.AddScoped<IWorkShopService, WorkShopService>();
            services.AddScoped<ILabourRateService, LabourRateService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IExternalWorkshopExpService, ExternalWorkshopExpService>();
            services.AddScoped<IWorkshopMovementService, WorkshopMovementService>();
            services.AddScoped<IMWorkOrderService, MWorkOrderService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<IAllowedTimeService, AllowedTimeService>();
            services.AddScoped<IExternalWorkshopInvoiceService, ExternalWorkshopInvoiceService>();
            services.AddScoped<IPriceMatrixService, PriceMatrixService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IWIPService, WIPService>();
            services.AddScoped<IRecallService, RecallService>();
            services.AddScoped<IMWorkOrderService, MWorkOrderService>();
            services.AddScoped<IClockingService, ClockingService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IAccountDefinitionService, AccountDefinitionService>();
            services.AddScoped<IWorkshopLoadingService, WorkshopLoadingService>();
            services.AddScoped<IMaintenanceCardService, MaintenanceCardService>();
            services.AddScoped<IJobCardService, JobCardService>();
			//services.AddScoped<IServiceReminderService, ServiceReminderService>();
			services.AddScoped<IClaimReportService, ClaimReportService>();
			services.AddScoped<IMaintenanceHistoryService, MaintenanceHistoryService>();
			//services.AddScoped<IDRTSCodeService, DRTSCodeService>();
			services.AddScoped<IMaintenanceCardService, MaintenanceCardService>();
            services.AddScoped<IJobCardService, JobCardService>();
            services.AddScoped<IServiceReminderService, ServiceReminderService>();
            services.AddScoped<IWorkshopLoadingService, WorkshopLoadingService>();
            return services;

        }
    }
}
