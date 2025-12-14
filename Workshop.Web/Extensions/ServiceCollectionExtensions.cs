using Workshop.Web.Services;

namespace Workshop.Web.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["ApiSettings:BaseApiUrl"]!;
            var erpUrl = configuration["ApiSettings:ERPApiUrl"]!;
            var accountingUrl = configuration["ApiSettings:AccountingApiUrl"]!;
            var vehicleUrl = configuration["ApiSettings:VehicleApiUrl"]!;
            var inventoryUrl = configuration["ApiSettings:InventoryApiUrl"]!;

            // Internal API clients (using BaseUrl)
            services.AddHttpClient<WorkshopApiClient>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            });

            // External API clients (using specific URLs)
            services.AddHttpClient<ERPApiClient>(client =>
            {
                client.BaseAddress = new Uri(erpUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<AccountingApiClient>(client =>
            {
                client.BaseAddress = new Uri(accountingUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });


            services.AddHttpClient<VehicleApiClient>(client =>
            {
                client.BaseAddress = new Uri(vehicleUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });


            services.AddHttpClient<InventoryApiClient>(client =>
            {
                client.BaseAddress = new Uri(inventoryUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            return services;
        }
    }
}
