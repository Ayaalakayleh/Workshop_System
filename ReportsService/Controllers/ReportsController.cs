using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ReportsService.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Helpers;
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

        //[HttpGet]
        //[Route("api/reports/test")]
        //public IHttpActionResult Test()
        //{
        //    return Ok(new
        //    {
        //        message = "ReportsService is running!",
        //        timestamp = DateTime.Now,
        //        baseUrl = _apiBaseUrl
        //    });
        //}

        //[HttpGet]
        //[Route("api/reports/wip/{id}")]
        //public HttpResponseMessage GetWipReport(int id)
        //{
        //    try
        //    {
        //        var reportDocument = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
        //        var reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/CrystalReport1.rpt");
                
        //        if (!System.IO.File.Exists(reportPath))
        //        {
        //            return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Report file not found.");
        //        }

        //        reportDocument.Load(reportPath);

        //        // Attempt to set connection credentials - Update these to match your DB
        //        // Ideally reading from config
        //        // reportDocument.SetDatabaseLogon("acs@workshop", "ACS@Worksh0p_#2025_", "94.249.88.254,1433", "DB_WorkshopCore");
        //        // Because connection handling in Crystal can be tricky depending on how the report was designed (OLEDB vs ODBC etc)
        //        // We will try standard Initialize but often need concrete Logon info.
                
        //        try
        //        {
        //             reportDocument.SetDatabaseLogon("acs@workshop", "ACS@Worksh0p_#2025_", "94.249.88.254,1433", "DB_WorkshopCore");
        //        }
        //        catch
        //        {
        //            // Ignore or log if logon fails (might be using saved data or integrated security)
        //        }

        //        // Try setting parameter 'Id' if it exists
        //        if (reportDocument.ParameterFields["Id"] != null) 
        //            reportDocument.SetParameterValue("Id", id);
        //        else if (reportDocument.ParameterFields["@Id"] != null)
        //             reportDocument.SetParameterValue("@Id", id);

        //        var stream = reportDocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                
        //        var result = new HttpResponseMessage(HttpStatusCode.OK)
        //        {
        //            Content = new StreamContent(stream)
        //        };
        //        result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        //        result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
        //        {
        //            FileName = $"WIP_Report_{id}.pdf"
        //        };

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //}

        [HttpPost]
        [Route("api/reports/wip/")]
        public HttpResponseMessage GetWipReport(RepairOrderRequestReportModel model )
        {
            ReportDocument rpt = new ReportDocument();
            
            try
            {
                var ds = new DataSet("RepairOrderDataSet");
                var dt = new DataTable("DataTable1");

                dt.Columns.Add("AccountNo", typeof(string));
                dt.Columns.Add("Date", typeof(DateTime));           
                dt.Columns.Add("TimeReceived", typeof(string));    
                dt.Columns.Add("Company", typeof(string));
                dt.Columns.Add("InsuranceExpDate", typeof(string));
                dt.Columns.Add("CustomerName", typeof(string));
                dt.Columns.Add("EstimaraExpDate", typeof(string));
                dt.Columns.Add("CustomerMobileNumber", typeof(string));
                dt.Columns.Add("MVPIExpDate", typeof(string));
                dt.Columns.Add("Complaint", typeof(string));
                dt.Columns.Add("Make", typeof(string));
                dt.Columns.Add("Model", typeof(string));
                dt.Columns.Add("ContractExpDate", typeof(string));
                dt.Columns.Add("WIPId", typeof(int));
                dt.Columns.Add("FuelLevel", typeof(string));
                dt.Columns.Add("RegistrationExpDate", typeof(string));
                dt.Columns.Add("CreatedBy", typeof(string));
                dt.Columns.Add("UserPhoeNo", typeof(string));
                dt.Columns.Add("CreatedDate", typeof(string));
                dt.Columns.Add("RepeatRepair", typeof(string));
                dt.Columns.Add("DateIn", typeof(string));
                dt.Columns.Add("TimeIn", typeof(string));
                dt.Columns.Add("DateOut", typeof(string));
                dt.Columns.Add("TimeOut", typeof(string));
                dt.Columns.Add("MovementId", typeof(int));

                var r = dt.NewRow();

                r["AccountNo"] = model.AccountNo ?? "";
                r["Date"] = (object)model.Date ?? DBNull.Value; 
                r["TimeReceived"] = model.TimeReceived?.ToString(@"hh\:mm") ?? "";
                r["Company"] = model.Company ?? model.CompanyName ?? ""; 
                r["InsuranceExpDate"] = model.InsuranceExpDate ?? "";
                r["CustomerName"] = model.CustomerName ?? "";
                r["EstimaraExpDate"] = model.EstimaraExpDate ?? "";
                r["CustomerMobileNumber"] = model.CustomerMobileNumber ?? "";
                r["MVPIExpDate"] = model.MVPIExpDate ?? "";
                r["Complaint"] = model.Complaint ?? "";

                r["Make"] = model.VehicleInfo?.Make ?? "";
                r["Model"] = model.VehicleInfo?.Model ?? "";

                r["ContractExpDate"] = model.ContractExpDate ?? "";
                r["WIPId"] = model.WIPId ?? 0;
                r["FuelLevel"] = model.FuelLevel ?? "";
                r["RegistrationExpDate"] = model.RegistrationExpDate ?? "";
                r["CreatedBy"] = model.CreatedBy ?? "";
                r["UserPhoeNo"] = model.UserPhoeNo ?? "";
                r["CreatedDate"] = model.CreatedDate ?? "";
                r["RepeatRepair"] = model.RepeatRepair ?? "";
                r["DateIn"] = model.DateIn ?? "";
                r["TimeIn"] = model.TimeIn ?? "";
                r["DateOut"] = model.DateOut ?? "";
                r["TimeOut"] = model.TimeOut ?? "";
                r["MovementId"] = model.MovementId ?? 0;

                dt.Rows.Add(r);

                ds.Tables.Add(dt);


                // Vehicle Checklist Table ==============================
                var dtVehicle = new DataTable("Checklist"); 
                dtVehicle.Columns.Add("LookupPrimaryDescription", typeof(string));
                dtVehicle.Columns.Add("LookupSecondaryDescription", typeof(string));
                dtVehicle.Columns.Add("Pass", typeof(bool));
                dtVehicle.Columns.Add("Description", typeof(string));

                foreach (var v in model.VehicleCkecklist ?? new List<VehicleChecklist>())
                {
                    dtVehicle.Rows.Add(
                        v.LookupPrimaryDescription ?? "",
                        v.LookupSecondaryDescription ?? "",
                        v.Pass,
                        v.Description ?? ""
                    );
                }

                ds.Tables.Add(dtVehicle);
                // End Vehicle Checklist Table 

                // Vehicle Service Table ==============================
                var dtService = new DataTable("Services");
                dtService.Columns.Add("Description", typeof(string));
                dtService.Columns.Add("StandardHours", typeof(decimal));

                foreach (var v in model.Services ?? new List<CreateWIPServiceModel>())
                {
                    dtService.Rows.Add(
                        v.Description ?? "",
                        v.StandardHours 
                    );
                }

                ds.Tables.Add(dtService);
                // End Vehicle Service Table 

                // Vehicle Items Table ==============================
                var dtItems = new DataTable("Items");
                dtItems.Columns.Add("Code", typeof(string));
                dtItems.Columns.Add("PrimaryName", typeof(string));
                dtItems.Columns.Add("Qty", typeof(decimal));
                dtItems.Columns.Add("UnitPrimaryName", typeof(string));

                foreach (var v in model.Items ?? new List<ItemModel>())
                {
                    dtItems.Rows.Add(
                        v.Code ?? "",
                        v.PrimaryName ?? "",
                        v.Qty,
                        v.UnitPrimaryName ?? ""
                    );
                }

                ds.Tables.Add(dtItems);
                // End Vehicle Items Table 

                // Vehicle Tires Checklist Table ========================
                var dtTires = new DataTable("Tires");
                dtTires.Columns.Add("LookupPrimaryDescription", typeof(string));
                dtTires.Columns.Add("Brand", typeof(string));
                dtTires.Columns.Add("DOT", typeof(string));
                dtTires.Columns.Add("WearLevel", typeof(decimal));

                foreach (var v in model.TyreCkecklist ?? new List<TyreChecklist>())
                {
                    dtTires.Rows.Add(
                        v.LookupPrimaryDescription ?? "",
                        v.Brand ?? "",
                        v.DOT ?? "",
                        v.WearLevel
                    );
                }
                ds.Tables.Add(dtTires);
                // End Vehicle Tires Checklist Table

                rpt.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "RepairOrderReport.rpt"));
                
                foreach (CrystalDecisions.CrystalReports.Engine.Table t in rpt.Database.Tables)
                {
                    t.SetDataSource(ds.Tables[t.Name]);
                }

                //Checklist Subreport
                var sub = rpt.OpenSubreport("SubRepairChecklistReport.rpt");
                sub.Database.Tables["Checklist"].SetDataSource(ds.Tables["Checklist"]);

                //Services Subreport
                var sub_Service = rpt.OpenSubreport("SubRepairServicesReport.rpt");
                sub_Service.Database.Tables["Services"].SetDataSource(ds.Tables["Services"]);

                //Items Subreport
                var sub_Items = rpt.OpenSubreport("SubRepairItemsReport.rpt");
                sub_Items.Database.Tables["Items"].SetDataSource(ds.Tables["Items"]);

                //Tires Subreport
                var sub_Tires = rpt.OpenSubreport("SubTiresChecklistReport.rpt");
                sub_Tires.Database.Tables["Tires"].SetDataSource(ds.Tables["Tires"]);

                string contentType;
                string fileName;
                Stream stream;

                stream = rpt.ExportToStream(ExportFormatType.PortableDocFormat);
                contentType = "application/pdf";
                fileName = $"Wip_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";


                stream.Position = 0;

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();
                }
                var resp = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };

                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                resp.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("inline")
                {
                    FileName = fileName
                };

                resp.Headers.CacheControl = new CacheControlHeaderValue
                {
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true
                };
                resp.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));

                return resp;


            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(ex.ToString())
                };
            }
            finally
            {
                rpt.Close();
                rpt.Dispose();
            }
        }

    }
}
