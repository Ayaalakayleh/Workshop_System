using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ReportsService.Controllers
{
    public class ReportsController : ApiController
    {
        private readonly string _apiBaseUrl;
        private readonly HttpClient _httpClient;

        public ReportsController()
        {
            _apiBaseUrl = System.Configuration.ConfigurationManager.AppSettings["MainApiBaseUrl"];
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_apiBaseUrl);
        }

        [HttpGet]
        [Route("api/reports/test")]
        public IHttpActionResult Test()
        {
            return Ok(new
            {
                message = "ReportsService is running!",
                timestamp = DateTime.Now,
                baseUrl = _apiBaseUrl
            });
        }
    }
}
