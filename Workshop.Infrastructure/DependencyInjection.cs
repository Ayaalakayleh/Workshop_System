using Microsoft.Extensions.DependencyInjection;
using Workshop.Core.Interfaces.IRepositories;
using Workshop.Infrastructure.Contexts;
using Workshop.Infrastructure.Repositories;

namespace Workshop.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            //Add repositories
            services.AddScoped<Database>();
            services.AddScoped<IDTechnicianRepository, DTechnicianRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<ITechnicianWorkScheduleRepository, TechnicianWorkScheduleRepository>();
            services.AddScoped<IRTSCodeRepository, RTSCodeRepository>();
			services.AddScoped<IRecallRepository, RecallREpository>();
			services.AddScoped<IWorkShopRepository, WorkShopRepository>();
            services.AddScoped<ILabourRateRepository, LabourRateRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();
            services.AddScoped<IExternalWorkshopExpRepository, ExternalWorkshopExpRepository>();
            services.AddScoped<IWorkshopMovementRepository, WorkshopMovementRepository>();
            services.AddScoped<IMWorkOrderRepository, MWorkOrderRepository>();
            services.AddScoped<IShiftRepository, ShiftRepository>();
            services.AddScoped<IAllowedTimeRepository, AllowedTimeRepository>();
            services.AddScoped<IExternalWorkshopInvoiceRepository, ExternalWorkshopInvoiceRepository>();
            services.AddScoped<IPriceMatrixRepository, PriceMatrixRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
			services.AddScoped<IWIPRepository, WIPRepository>();
            services.AddScoped<IMWorkOrderRepository, MWorkOrderRepository>();
            services.AddScoped<IClockingRepository, ClockingRepository>();
			services.AddScoped<IClaimReportRepository, ClaimReportRepository>();
			services.AddScoped<IMaintenanceHistoryRepository, MaintenanceHistoryRepository>();
			services.AddScoped<IWorkshopLoadingRepository, WorkshopLoadingRepository>();
            services.AddScoped<IRTSCodeRepository>(provider =>
			{
				var context = provider.GetRequiredService<DapperContext>();
				return new RTSCodeRepository(context);
			});
            services.AddScoped<IMaintenanceCardRepository, MaintenanceCardRepository>();
            services.AddScoped<IJobCardRepository, JobCardRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IServiceReminderRepository, ServiceReminderRepository>();
            services.AddScoped<IAccountDefinitionRepository, AccountDefinitionRepository>();


            return services;
        }
    }
}
