using ReportsService.Models;
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

        [HttpGet]
        [Route("api/reports/wip/{id}")]
        public HttpResponseMessage GetWipReport(int id)
        {
            try
            {
                var reportDocument = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
                var reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/CrystalReport1.rpt");
                
                if (!System.IO.File.Exists(reportPath))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Report file not found.");
                }

                reportDocument.Load(reportPath);

                // Attempt to set connection credentials - Update these to match your DB
                // Ideally reading from config
                // reportDocument.SetDatabaseLogon("acs@workshop", "ACS@Worksh0p_#2025_", "94.249.88.254,1433", "DB_WorkshopCore");
                // Because connection handling in Crystal can be tricky depending on how the report was designed (OLEDB vs ODBC etc)
                // We will try standard Initialize but often need concrete Logon info.
                
                try
                {
                     reportDocument.SetDatabaseLogon("acs@workshop", "ACS@Worksh0p_#2025_", "94.249.88.254,1433", "DB_WorkshopCore");
                }
                catch
                {
                    // Ignore or log if logon fails (might be using saved data or integrated security)
                }

                // Try setting parameter 'Id' if it exists
                if (reportDocument.ParameterFields["Id"] != null) 
                    reportDocument.SetParameterValue("Id", id);
                else if (reportDocument.ParameterFields["@Id"] != null)
                     reportDocument.SetParameterValue("@Id", id);

                var stream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"WIP_Report_{id}.pdf"
                };

                return result;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("api/reports/wip/")]
        public HttpResponseMessage GetWipReport(WIPModel wipModel)
        {
            try
            {
                var reportDocument = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
                var reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/CrystalReport1.rpt");

                if (!System.IO.File.Exists(reportPath))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Report file not found.");
                }

                reportDocument.Load(reportPath);

                // Attempt to set connection credentials - Update these to match your DB
                // Ideally reading from config
                // reportDocument.SetDatabaseLogon("acs@workshop", "ACS@Worksh0p_#2025_", "94.249.88.254,1433", "DB_WorkshopCore");
                // Because connection handling in Crystal can be tricky depending on how the report was designed (OLEDB vs ODBC etc)
                // We will try standard Initialize but often need concrete Logon info.

                try
                {
                    reportDocument.SetDatabaseLogon("acs@workshop", "ACS@Worksh0p_#2025_", "94.249.88.254,1433", "DB_WorkshopCore");
                }
                catch
                {
                    // Ignore or log if logon fails (might be using saved data or integrated security)
                }

                // Try setting parameter 'Id' if it exists
                if (reportDocument.ParameterFields["Id"] != null)
                    reportDocument.SetParameterValue("Id", wipModel.Id);
                else if (reportDocument.ParameterFields["@Id"] != null)
                    reportDocument.SetParameterValue("@Id", wipModel.Id);

                reportDocument.SetDataSource(wipModel);

                var stream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(stream)
                };
                result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"WIP_Report_{wipModel.Id}.pdf"
                };

                return result;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

    }
}
