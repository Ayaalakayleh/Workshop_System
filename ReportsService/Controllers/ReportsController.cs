using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using ReportsService.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
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

       
        [HttpPost]
        [Route("api/reports/wip/")]
        public HttpResponseMessage GetWipReport(RepairOrderRequestReportModel model )
        {
            ReportDocument rpt = new ReportDocument();
            
            try
            {

                //Test Image
                //if (model.DamageImageBytes?.Length > 0)
                //{
                //    var dbg = Path.Combine(Path.GetTempPath(), "damage_debug.jpg");
                //    System.IO.File.WriteAllBytes(dbg, model.DamageImageBytes);
                //}

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
                dt.Columns.Add("Year", typeof(string));
                dt.Columns.Add("ContractExpDate", typeof(string));
                dt.Columns.Add("WIPId", typeof(int));
                dt.Columns.Add("FuelLevel", typeof(string));
                dt.Columns.Add("RegistrationExpDate", typeof(string));
                dt.Columns.Add("RegistrationNo", typeof(string));
                dt.Columns.Add("CreatedBy", typeof(string));
                dt.Columns.Add("UserPhoeNo", typeof(string));
                dt.Columns.Add("CreatedDate", typeof(string));
                dt.Columns.Add("RepeatRepair", typeof(string));
                dt.Columns.Add("DateIn", typeof(string));
                dt.Columns.Add("TimeIn", typeof(string));
                dt.Columns.Add("DateOut", typeof(string));
                dt.Columns.Add("TimeOut", typeof(string));
                dt.Columns.Add("MovementId", typeof(int));
                dt.Columns.Add("DamageImage", typeof(byte[]));
                dt.Columns.Add("DamageImage_Vertical", typeof(byte[]));
                dt.Columns.Add("RecallListText", typeof(string));
                dt.Columns.Add("PlateNumber", typeof(string));
                dt.Columns.Add("VIN", typeof(string));
                dt.Columns.Add("Mileage", typeof(decimal));
                dt.Columns.Add("ColorName", typeof(string));
                dt.Columns.Add("Note", typeof(string));


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
                r["Year"] = model.VehicleInfo?.Year ?? 0;
                r["ContractExpDate"] = model.ContractExpDate ?? "";
                r["WIPId"] = model.WIPId ?? 0;
                r["FuelLevel"] = model.FuelLevel ?? "";
                r["RegistrationExpDate"] = model.RegistrationExpDate ?? "";
                r["RegistrationNo"] = model.RegistrationNo ?? "";
                r["CreatedBy"] = model.CreatedBy ?? "";
                r["UserPhoeNo"] = model.UserPhoeNo ?? "";
                r["CreatedDate"] = model.CreatedDate ?? "";
                r["RepeatRepair"] = model.RepeatRepair ?? "";
                r["DateIn"] = model.DateIn ?? "";
                r["TimeIn"] = model.TimeIn ?? "";
                r["DateOut"] = model.DateOut ?? "";
                r["TimeOut"] = model.TimeOut ?? "";
                r["MovementId"] = model.MovementId ?? 0;
                r["DamageImage"] = (object)model.DamageImageBytes ?? DBNull.Value;
                r["DamageImage_Vertical"] = (object)model.DamageImageBytes_Vertical ?? DBNull.Value;
                r["RecallListText"] = model.RecallListText ?? "";
                r["PlateNumber"] = model.VehicleInfo.PlateNumber ?? "";
                r["VIN"] = model.VehicleInfo.VIN ?? "";
                r["Mileage"] = model.VehicleInfo.Mileage ?? 0;
                r["ColorName"] = model.VehicleInfo.ColorName ?? "";
                r["Note"] = model.Note ?? "";


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
                dtService.Columns.Add("Code", typeof(string));
                dtService.Columns.Add("Description", typeof(string));
                dtService.Columns.Add("LongDescription", typeof(string));
                dtService.Columns.Add("KeyId", typeof(int));
                dtService.Columns.Add("StandardHours", typeof(decimal));

                foreach (var v in model.Services ?? new List<CreateWIPServiceModel>())
                {
                    dtService.Rows.Add(
                        v.Code ?? "",
                        v.Description ?? "",
                        v.LongDescription ?? "",
                        v.KeyId,
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
                dtTires.Columns.Add("WearLevel", typeof(string));

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

                // CompanyData Table ========================
                var dtCompanyData = new DataTable("CompanyData");
                dtCompanyData.Columns.Add("CompanyPrimaryName", typeof(string));
                dtCompanyData.Columns.Add("Branch", typeof(string));
                dtCompanyData.Columns.Add("Img", typeof(byte[]));
                dtCompanyData.Columns.Add("Title", typeof(string));

                dtCompanyData.Rows.Add(
                    model.CompanyData.CompanyPrimaryName ?? "",
                    model.CompanyData.Branch ?? "",
                    (object)model.CompanyData.Img ?? DBNull.Value,
                    "OPERATIONAL LEASE - OPL REPAIR ORDER REQUEST"

                );
                ds.Tables.Add(dtCompanyData);
                // End CompanyData Table

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
                //var sub_Items = rpt.OpenSubreport("SubRepairItemsReport.rpt");
                //sub_Items.Database.Tables["Items"].SetDataSource(ds.Tables["Items"]);

                //Tires Subreport
                var sub_Tires = rpt.OpenSubreport("SubTiresChecklistReport.rpt");
                sub_Tires.Database.Tables["Tires"].SetDataSource(ds.Tables["Tires"]);
               
                //Header Subreport
                var sub_Header = rpt.OpenSubreport("CryHeaderEn.rpt");
                sub_Header.Database.Tables["CompanyData"].SetDataSource(ds.Tables["CompanyData"]);

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
                string logDirectory = @"C:\LogFiles";
                Directory.CreateDirectory(logDirectory);

                string logFile = Path.Combine(
                    logDirectory,
                    "Log_" + DateTime.Now.ToString("dd_MM_yyyy", new CultureInfo("en-us")) + ".txt"
                );

                var sLogFormat =
                    DateTime.Now.ToString("dd/MM/yyyy", new CultureInfo("en-us")) + " " +
                    DateTime.Now.ToString("HH:mm:ss", new CultureInfo("en-us")) + " ==> ";

                using (StreamWriter sw = new StreamWriter(logFile, true))
                {
                    sw.WriteLine(sLogFormat + ex.Message + " " + ex.StackTrace);
                }

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
