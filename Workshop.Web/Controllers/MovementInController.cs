using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]

    public class MovementInController : BaseController
    {

        private readonly WorkshopApiClient _workshopapiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly WorkshopApiClient _apiClient;
        private readonly IFileService _fileService;
        private readonly int areaId = 1; //((CompanyBranch)Session["branchInfo"]).AreaId;
        private readonly string lang;
        private readonly ILogger<MovementInController> _logger;


        public MovementInController(
            WorkshopApiClient workshopapiClient,
            VehicleApiClient vehicleApiClient,
            ERPApiClient erpApiClient,
            WorkshopApiClient workshopApiClient,
            IFileService fileService,
            ILogger<MovementInController> logger,
            IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _workshopapiClient = workshopapiClient;
            _vehicleApiClient = vehicleApiClient;
            _fileService = fileService;
            _erpApiClient = erpApiClient;
            _apiClient = workshopApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
            _logger = logger;

        }

        [CustomAuthorize(Permissions.MovementIn.View)]
        public async Task<IActionResult> Index(int? vehicleId, int? vehicleTypeId)
        {

            ViewBag.Makes = await GetMakes();
            ViewBag.SubStatuses = await GetSubStatuses();
            ViewBag.FromReservationVehicleId = vehicleId ?? 0;
            ViewBag.FromReservationVehicleTypeId = vehicleTypeId ?? 0;
            return View(new VehicleMovement());

        }


        [HttpPost]
        public async Task<IActionResult> VehicleList([FromBody] VehicleAdvancedFilter filter)
        {
            filter ??= new VehicleAdvancedFilter();
            var colVehicleDefinitions = new List<VehicleDefinitions>();

            filter.CompanyId = CompanyId;

            if (filter.VehicleTypeId == 1) // internal
            {
                colVehicleDefinitions = await _vehicleApiClient.GetWorkshopVehicles(filter);
            }
            else if (filter.VehicleTypeId == 2) // external
            {
                colVehicleDefinitions = await _vehicleApiClient.VehicleDefinitions_GetExternalWSVehicles(filter.PageNumber, filter.ManufacturerId == 0 ? default(int?) : filter.ManufacturerId, filter.PlateNumber, filter.VehicleModelId == 0 ? default(int?) : filter.VehicleModelId, filter.ChassisNo);
            }
            return PartialView("_VehicleSelectList", colVehicleDefinitions);
        }

        public async Task<IActionResult> GetLastOutMaintenanceCard(int vehicleId)
        {
            VehicleMovement movement = new VehicleMovement();
            movement.ColMaintenanceCard = new List<MaintenanceCardDTO>();

            movement = await _vehicleApiClient.GetLastMaintenanceMovement(vehicleId);

            WorkOrderFilterDTO workOrderFilter = new WorkOrderFilterDTO();
            workOrderFilter.VehicleID = vehicleId;
            workOrderFilter.CompanyId = CompanyId;
            workOrderFilter.language = lang;
            workOrderFilter.Id = movement.WorkOrderId;
            movement.WorkOrders = await _workshopapiClient.GetMWorkOrdersAsync(workOrderFilter);
            movement.Services = new List<Item>();
            return PartialView("OutMaintenaceCard", movement);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.MovementIn.Create)]
        public async Task<IActionResult> VehicleMovementInInsert(
    [FromForm] VehicleMovement movement,
    [FromForm] IEnumerable<IFormFile> Photos,
    [FromForm] string driverSignatureBase64,
    [FromForm] string employeeSignatureBase64,
    [FromForm] string EType,
    [FromForm] DateTime GregorianMovementDate
            )
        {
            var resultJson = new TempData();
            string checkpoint = "START";
            try
            {
                bool IsExternal = EType == "1" ? false : true;

                // Handle signatures 
                if (!string.IsNullOrEmpty(driverSignatureBase64))
                {
                    var (filePath, fileName) = await _fileService.SaveBase64FileAsync(driverSignatureBase64, "MovementSignatures");
                    movement.DriverSignature = fileName;
                }

                if (!string.IsNullOrEmpty(employeeSignatureBase64))
                {
                    var (filePath, fileName) = await _fileService.SaveBase64FileAsync(employeeSignatureBase64, "MovementSignatures");
                    movement.EmployeeSignature = fileName;
                }

                movement.CreatedBy = UserId;
                movement.WorkshopId = BranchId;
                movement.BranchId = BranchId;
                movement.ReceivedBranchId = BranchId;
                movement.MovementIN = true;
                movement.CompanyId = CompanyId;
                movement.MasterId = Guid.NewGuid();
                movement.Status = 1;
                movement.IsExternal = IsExternal;
                //card.Cards[0].WorkOrderId;

                // Vehicle Movement validation , date and type and with vehicle
                var VehicleMovementStatus = await _workshopapiClient.CheckVehicleMovementStatusAsync(movement.VehicleID.Value);

                if (movement.GregorianMovementDate.Value.Date.Add(movement.ReceivedTime.Value) < VehicleMovementStatus.lastmovemnetDate)
                {
                    resultJson.IsSuccess = false;
                    resultJson.Message = "Cannot make In before last movement in" + " " + VehicleMovementStatus.lastmovemnetDate.ToString("dd-MM-yyyy hh:mm");
                    return Json(resultJson);
                }

                if ((VehicleMovementStatus.result == 0 || VehicleMovementStatus.result == 2))
                {
                    if (movement.LastVehicleStatus == 3)
                    {
                        var lastMovement = await _vehicleApiClient.GetLastMovementByVehicleId(movement.VehicleID.Value);
                        if (lastMovement.MovementId == 0 || lastMovement.GregorianMovementDate == DateTime.MinValue)
                        {
                            resultJson.IsSuccess = false;
                            resultJson.Message = "Vehicle Should moved out";
                            return Json(resultJson);
                        }
                        if (lastMovement.GregorianMovementDate > movement.GregorianMovementDate.Value.Date.Add(movement.ReceivedTime.Value) || lastMovement.MovementIN.Value)
                        {
                            resultJson.IsSuccess = false;
                            resultJson.Message = "CannotmakeInbeforelastmovementoperation" + " " + lastMovement.GregorianMovementDate;
                            return Json(resultJson);
                        }
                    }
                    var vehicleNams = new List<VehicleNams>();
                    //var workOrder = new MWorkOrderDTO();




                    MWorkOrderDTO workOrder = null;

                    if (movement.WorkOrderId.HasValue)
                    {
                        workOrder = await _workshopapiClient.GetMWorkOrderByID(movement.WorkOrderId.Value);
                    }
                    else if (!string.IsNullOrWhiteSpace(movement.Complaint))
                    {
                        workOrder = new MWorkOrderDTO
                        {
                            VehicleId = movement.VehicleID,
                            WorkOrderType = 2,
                            GregorianDamageDate = movement.GregorianMovementDate,
                            Description = movement.Complaint,
                            Note = movement.ComplaintNote,
                            WorkOrderStatus = 4,
                            InvoicingStatus = 2,
                            CreatedBy = UserId,
                            IsExternal = IsExternal,
                            CompanyId = CompanyId,
                            BranchId = BranchId,
                            Wfstatus = (int)movement.VehicleSubStatusId,
                            VehicleType = IsExternal ? 2: 1
                        };

                        var createdWo = await _apiClient.InsertMWorkOrderAsync(workOrder);
                        workOrder.Id = createdWo.Id;
                        movement.WorkOrderId = createdWo.Id;
                    }
                    checkpoint = "InsertMovement";
                    VehicleMovement newMovement = await _workshopapiClient.InsertVehicleMovementAsync(movement);

                    if (newMovement != null)
                    {
                        if (!IsExternal)
                        {
                            bool IsUpdated = (await _vehicleApiClient.UpdateVehicleStatus(movement.VehicleID.Value, 10, movement.VehicleSubStatusId)).IsSuccess;
                        }

                        

                        //VehicleMovement newMovement = await _workshopapiClient.InsertVehicleMovementAsync(movement);

                        if (movement.WorkOrderId.HasValue && movement.WorkOrderId>0)
                        {
                            var card = new MaintenanceCardDTO
                            {
                                WorkOrderId = movement.WorkOrderId,
                                MovementId = newMovement.MovementId.Value
                            };
                            checkpoint = "UpdateWorkOrderKM";
                            await _workshopapiClient.UpdateWorkOrderKMAsync(movement.WorkOrderId.Value, movement.ReceivedMeter.Value);
                            checkpoint = "UpdateWorkOrderStatus";
                            await _workshopapiClient.UpdateWorkOrderStatusAsync(movement.WorkOrderId.Value, 2);
                            checkpoint = "InsertMaintenanceCard";
                            await _workshopapiClient.InsertDMaintenanceCardAsync(card);
                        }


                        if (!string.IsNullOrEmpty(movement.strikes) && newMovement.MovementId > 0)
                        {
                            newMovement.strikes = movement.strikes;
                            VehicleMovementStrike vehicleMovementStrike = new VehicleMovementStrike();
                            vehicleMovementStrike.MovementId = newMovement.MovementId;
                            vehicleMovementStrike.Strike = movement.strikes;
                            await _workshopapiClient.InsertMWorkshopMovementStrikes(vehicleMovementStrike);
                        }

                        if (workOrder != null && workOrder.WorkOrderType == 1)
                        {
                            #region Update accident status to waiting Repair 
                            InsuranceClaimHistory oInsuranceClaimHistory = new InsuranceClaimHistory();
                            oInsuranceClaimHistory.WorkOrderId = workOrder.Id;
                            oInsuranceClaimHistory.PathId = 3;
                            oInsuranceClaimHistory.Status = 22;
                            oInsuranceClaimHistory.CreatedBy = (int)workOrder.CreatedBy;
                            oInsuranceClaimHistory.CompanyId = (int)workOrder.CompanyId;
                            oInsuranceClaimHistory.CreatedBy = (int)workOrder.CreatedBy;
                            oInsuranceClaimHistory.BranchId = (int)workOrder.BranchId;
                            oInsuranceClaimHistory.vehicleId = (int)workOrder.VehicleId;
                            await _workshopapiClient.UpdateMAccidentStatusAsync(oInsuranceClaimHistory);
                            #endregion
                        }

                        #region Documents - UPDATED FILE HANDLING WITH IFormFile

                        if (newMovement.MovementId > 0)
                        {
                            // Handle photo files using the new file service with IFormFile
                            foreach (var file in Photos)
                            {
                                if (file != null && file.Length > 0)
                                {
                                    // NEW WAY: Using FileService with IFormFile
                                    var (filePath, fileName) = await _fileService.SaveFileAsync(file, "MovementImages");

                                    var document = new VehicleMovementDocument()
                                    {
                                        MovementId = newMovement.MovementId.Value,
                                        FilePath = filePath,
                                        FileName = fileName,
                                        CreatedBy = UserId
                                    };

                                    await _workshopapiClient.InsertMovementDocumentAsync(document);
                                }
                            }
                        }

                        #endregion

                        resultJson.IsSuccess = true;
                        resultJson.Id = newMovement.MovementId.Value;
                        resultJson.Data = Url.Action("VehicleMovementFind", "Movements", new { MovementId = newMovement.MovementId });

                        if (!IsExternal)
                        {
                            checkpoint = "GetVehiclesDDL";
                            vehicleNams = await _vehicleApiClient.GetVehiclesDDL(lang, CompanyId);
                            var PrimaryMessage = string.Format("{0} : ( {1} )", "The vehicle Under Repair", vehicleNams.Where(x => x.id == movement.VehicleID).FirstOrDefault().VehicleName);
                            var SecondaryMessage = string.Format("{0} : ( {1} )", " السيارة قيد الاصلاح", vehicleNams.Where(x => x.id == movement.VehicleID).FirstOrDefault().VehicleName);

                            var oNotification = new Notification()
                            {
                                BranchId = (int)movement.BranchId,
                                Type = 8,
                                RelatedItemId = movement.VehicleID,
                                CreatedBy = UserId,
                                PrimaryMessage = PrimaryMessage,
                                SecondaryMessage = SecondaryMessage,
                                UserId = null,
                            };
                            checkpoint = "Notification_Insert";
                            await _erpApiClient.Notification_Insert(oNotification);
                        }

                        return Json(resultJson);
                    }
                    else
                    {
                        resultJson.IsSuccess = false;
                        return Json(resultJson);
                    }
                }
                else
                {
                    resultJson.IsSuccess = false;
                    resultJson.Message = "The Vehicle is already in the Workshop";
                    return Json(resultJson);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error for vehicle {VehicleId}", movement.VehicleID);
                resultJson.IsSuccess = false;
                resultJson.Message = $"ERROR at: {checkpoint} And the Exeption: {ex}";
                return Json(resultJson);
            }
        }

        public async Task<JsonResult> GetAgreementbyVehicleId(int id)
        {
            try
            {
                var customer = await _vehicleApiClient.GetAgreementbyVehicleId(id);

                return Json(new
                {
                    isSuccess = true,
                    data = customer
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    isSuccess = false,
                    data = (object)null
                });
            }
        }


        #region Data

        private async Task<List<SelectListItem>> GetMakes()
        {

            var makes = await _vehicleApiClient.GetAllManufacturers(lang);

            return makes.Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            }).ToList();
        }

        private async Task<List<SelectListItem>> GetSubStatuses()
        {

            var subStatuses = (await _vehicleApiClient.GetAllSubStatus(CompanyId, lang)).Select(r => new SelectListItem { Text = lang == "en" ? r.PrimaryName : r.SecondaryName, Value = r.Id.ToString() }).ToList();

            return subStatuses;
        }
        #endregion

    }

}
