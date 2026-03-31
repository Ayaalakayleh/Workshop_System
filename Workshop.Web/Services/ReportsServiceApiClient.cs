using System.Net.Http.Json;
using Workshop.Core.DTOs;
using Workshop.Web.Models;

namespace Workshop.Web.Services
{
    public class ReportsServiceApiClient
    {
     
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ReportsServiceApiClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<byte[]> RepairOrderRequestReportAsync(RepairOrderRequestReportModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/reports/wip", model);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error Status: {response.StatusCode}");
                Console.WriteLine($"Error Content: {errorContent}");
                return Array.Empty<byte>();
            }

            return await response.Content.ReadAsByteArrayAsync();
        }

    }
}
