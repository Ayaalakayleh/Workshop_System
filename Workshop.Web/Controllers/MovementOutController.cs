using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Insurance;
using Workshop.Core.DTOs.TempData;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Interfaces.Services;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]

    public class MovementOutController : BaseController
    {

        private readonly WorkshopApiClient _workshopApiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly AccountingApiClient _accountingApiClient;
        private readonly ERPApiClient _erpApiClient;
        private readonly IFileService _fileService;
        public readonly string lang;
        private readonly int areaId = 1; //((CompanyBranch)Session["branchInfo"]).AreaId;
        private readonly dynamic workshop = new { BranchPrimaryName = "B Name", BranchSecondaryName = "B Secondory Name" };// ((CompanyBranch)Session["branchInfo"]);


        public MovementOutController(
            WorkshopApiClient workshopapiClient,
            VehicleApiClient vehicleApiClient,
            AccountingApiClient accountingApiClient,
            ERPApiClient erpApiClient,
            IFileService fileService,
            IConfiguration configuration, IWebHostEnvironment env, IMemoryCache cache) : base(cache, configuration, env)
        {
            _workshopApiClient = workshopapiClient;
            _vehicleApiClient = vehicleApiClient;
            _accountingApiClient = accountingApiClient;
            _erpApiClient = erpApiClient;
            _fileService = fileService;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;

        }

        [CustomAuthorize(Permissions.Movements.MovementOutCreate)]
        public async Task<IActionResult> MovementOut(int movementId)
        {

            VehicleMovement movement = await _workshopApiClient.GetVehicleMovementByIdAsync(movementId);
            ViewBag.LastMovementDate = movement.GregorianMovementDate;

            //WorkOrders
            WorkOrderFilterDTO workOrderFilter = new WorkOrderFilterDTO();
            workOrderFilter.VehicleID = movement.VehicleID;
            workOrderFilter.CompanyId = CompanyId;
            workOrderFilter.language = lang;

            movement.WorkOrders = await _workshopApiClient.GetMWorkOrdersAsync(workOrderFilter);
            movement.RefReservationType = new ReservationType();
            movement.LastVehicleMovementDocuments = new List<VehicleMovementDocument>();
            movement.RefVehicledefinitions = new VehicleDefinitions();
            movement.RefVehicledefinitions.InsuranceCompany = new CustomerInformation();

            movement.RefVehicledefinitions = await _vehicleApiClient.GetVehicleDetails(movement.VehicleID.Value, lang);
            //ovehicleMovement.RefVehicledefinitions.ColInsuranceCompany = await _vehicleApiClient.GetInsuranceCompany(language, CompanyId);

            //Missed from DB
            movement.TotalWorkOrder = 0; //await _workshopApiClient.GetWorkOrderTotalByMasterId(movement.MasterId);
            movement.RelatedItemId = BranchId;//((CompanyInfo)Session["CompanyInfo"]).workshopId;
            movement.ColMaintenanceCard = new List<MaintenanceCardDTO>();

            if (movement.MovementId.HasValue)
                movement.ColMaintenanceCard = await _workshopApiClient.GetDMaintenanceCardsByMovementIdAsync(movement.MovementId.Value);
            var sameOrder = movement.WorkOrders.Find(x => x.Id == movement.ColMaintenanceCard.First().WorkOrderId);

            foreach (var card in movement.ColMaintenanceCard)
            {
                if (card.WorkOrderId == sameOrder.Id)
                    card.Description = sameOrder.Description;
            }


            movement.RefVehicledefinitions.ColVehicleSubStatus = await _vehicleApiClient.GetAllSubStatus(CompanyId, lang);
            if (movement.ColMaintenanceCard.Count == 0)
            {
                if (movement.MasterId.HasValue)
                    movement.ColMaintenanceCard = await _workshopApiClient.GetDMaintenanceCardsByMasterIdAsync(movement.MasterId.Value);
            }

            movement.fuelLevels = await _vehicleApiClient.GetFuleLevel();
            movement.GregorianMovementDate = DateTime.Now;

            return View("MovementOut", movement);
        }


        [HttpPost]
        [CustomAuthorize(Permissions.Movements.MovementOutCreate)]
        public async Task<IActionResult> MoveOut([FromBody] VehicleMovement movement)
        {

            var result = new TempData();
            try
            {
                var workOrder = new MWorkOrderDTO();
                result.Notification = new List<Notification>();
                movement.CompanyId = CompanyId;
                movement.CreatedBy = UserId;
                movement.Status = 4;
                movement.MovementOut = true;
                int? movementInId = 0;
                var car = new VehicleDefinitions();

                var jobCard = await _workshopApiClient.GetJobCardByMasterId(movement.MasterId.Value);

                if (jobCard != null && jobCard.Id > 0)
                {
                    if (jobCard.Status != 3) // not closed
                    {
                        result.IsSuccess = false;
                        result.Type = "error";
                        result.Message = "Job Card Not Closed";
                        return Json(result);
                    }
                }

                movement.InvoiceType = await _accountingApiClient.TypeSalesPurchases_GetAll(movement.CompanyId.Value, BranchId, 1, 2);

                if (((jobCard != null && jobCard.Id > 0) || movement.InvoiceId > 0))
                {
                    result.IsSuccess = false;
                    result.Type = "error";
                    result.Message = "Total Cost Should Be Greater Thanzero";
                    return Json(result);
                }

                var VehicleMovementStatus = await _workshopApiClient.CheckVehicleMovementStatusAsync(movement.VehicleID.Value);
                if (movement.GregorianMovementDate.Value.Add(movement.ExitTime.Value) < VehicleMovementStatus.lastmovemnetDate)
                {
                    result.IsSuccess = false;
                    result.Type = "error";
                    result.Message = "Cannot makeout before last movement in " + " " + VehicleMovementStatus.lastmovemnetDate;
                    return Json(result);
                }

                workOrder = await _workshopApiClient.GetMWorkOrderByID((int)(movement.WorkOrderId ?? 0));
                movement.WorkOrderId = workOrder?.Id;
                if (movement != null && !movement.IsExternal.Value)
                {
                    car = await _vehicleApiClient.M_VehicleDefinitionsGetPlateNumberCostCenterById((int)movement.VehicleID);
                }

                movementInId = (await _workshopApiClient.InsertVehicleMovementAsync(movement))?.MovementId;

                if (movementInId != 0 && !movement.IsExternal.Value)
                {
                    if (movement.LastVehicleStatus == 2)
                    {
                        await _vehicleApiClient.UpdateVehicleStatus(movement.VehicleID.Value, 11);  // Update Vehicle Status to Out Garage 

                        var oNotification = new Notification()
                        {
                            BranchId = workOrder.BranchId,
                            Type = 8,
                            RelatedItemId = movement.VehicleID,
                            CreatedBy = UserId,//SessionManager.GetSessionUserInfo.UserID,
                            PrimaryMessage = string.Format("{0} : ( {1} )", "The vehicle Out Garage", car.PlateNumber),
                            SecondaryMessage = string.Format("{0} : ( {1} )", " السيارة خرجت من الورشة", car.PlateNumber),
                            ModuleId = 2,
                            Link = "/VehicleMovements/VehicleMovementIN?VehicleNo=" + movement.VehicleID + "&Type=8",
                            PrimaryType = "السيارة خرجت من الورشة",
                            SecondaryType = "The vehicle Out Garage",
                            ToModuleId = 2
                        };
                        var notification = await _erpApiClient.Notification_Insert(oNotification);

                        //ToDo: BaseController
                        //ModulesHubsConnection(notification);
                    }
                    else if (movement.LastVehicleStatus == 3)
                    {
                        await _vehicleApiClient.UpdateVehicleStatus(movement.VehicleID.Value, 3);  // Update Vehicle Status to Rented  

                    }
                    else if (movement.LastVehicleStatus == 13)
                    {
                        await _vehicleApiClient.UpdateVehicleStatus(movement.VehicleID.Value, 13);  // Update Vehicle Status to complementry  

                    }
                    else if (movement.LastVehicleStatus == 12)
                    {
                        await _vehicleApiClient.UpdateVehicleStatus(movement.VehicleID.Value, 12);  // Update Vehicle Status to staff  
                    }
                }

                movement.MovementId = movementInId;

                VehicleMovementStrike vehicleMovementStrike = new VehicleMovementStrike();
                vehicleMovementStrike.MovementId = movement.MovementId;
                vehicleMovementStrike.Strike = movement.strikes;
                await _workshopApiClient.InsertMWorkshopMovementStrikes(vehicleMovementStrike);

                //await _workshopApiClient.UpdateDMaintenanceCardAsync(movement.Card);
                // Update Job Card status to be closed if collection process completed 
                if ((workOrder.WorkOrderType == 1 && workOrder.CollectionPathStatusId == 16) || workOrder.WorkOrderType == 2)
                {
                    await _workshopApiClient.UpdateWorkOrderStatusAsync(workOrder.Id, 4); //Closed
                }
                else
                {
                    await _workshopApiClient.UpdateWorkOrderStatusAsync(workOrder.Id, 2); // Pending
                }
                #region Update accident status to Waiting to Receive Vehicle from Operation 24
                // Update accident status to Waiting To Send to Operation 
                if (workOrder.WorkOrderType == 1)
                {
                    InsuranceClaimHistory oInsuranceClaimHistory = new InsuranceClaimHistory();
                    oInsuranceClaimHistory.WorkOrderId = workOrder.Id;
                    oInsuranceClaimHistory.PathId = 3;
                    oInsuranceClaimHistory.Status = 24;
                    oInsuranceClaimHistory.CreatedBy = workOrder.CreatedBy;
                    oInsuranceClaimHistory.CompanyId = workOrder.CompanyId;
                    oInsuranceClaimHistory.CreatedBy = workOrder.CreatedBy;
                    oInsuranceClaimHistory.BranchId = workOrder.BranchId;
                    oInsuranceClaimHistory.vehicleName = workOrder.VehicleName;
                    oInsuranceClaimHistory.vehicleId = workOrder.VehicleId;
                    await _workshopApiClient.UpdateMAccidentStatusAsync(oInsuranceClaimHistory);
                }
                #endregion

                if (!movement.IsExternal.Value)
                {
                    var serviceSchedule = new ServiceScheduleDTO()
                    {
                        VehicleId = movement.VehicleID.Value,
                        WorkOrderId = workOrder.Id,
                        Meter = movement.ExitMeter.Value,
                        Date = movement.GregorianMovementDate.Value
                    };

                    await _workshopApiClient.UpdateServiceScheduleByWorkOrderIdAsync(serviceSchedule);
                }
                if (!movement.IsExternal.Value)
                {
                    var oNotification = new Notification()
                    {
                        BranchId = workOrder.BranchId ,
                        Type = 8,
                        RelatedItemId = movement.VehicleID,
                        CreatedBy = UserId,//SessionManager.GetSessionUserInfo.UserID,
                        PrimaryMessage = string.Format("{0} : ( {1} ) ,{2}  {3}  {4} {5}", "Vehicle", car.PlateNumber, "Moved Out from  ", workshop.BranchPrimaryName, "Workshop", DateTime.Now),
                        SecondaryMessage = string.Format("{0} : ( {1} ) ,{2}  {3} {4} ", " المركبة", car.PlateNumber, "اخرجت من ورشة ", workshop.BranchSecondaryName, DateTime.Now),
                        ModuleId = 2,
                        Link = "/VehicleMovements/VehicleMovementIN?VehicleNo=" + movement.VehicleID + "&Type=8",
                        PrimaryType = "السيارة خرجت من الورشة",
                        SecondaryType = "The vehicle Out Garage",
                        ToModuleId = 2
                    };
                    var notification = await _erpApiClient.Notification_Insert(oNotification);

                    //ToDo: BaseController
                    //ModulesHubsConnection(notification);
                }
                #region NRA_Logger 

                //ToDo: Exception Logger
                //var NRA_Logger = ExceptionLogger.NRA_Logger(movement.VehicleID, movement.VehicleSubStatusId, AreaId, workOrder.BranchId, movement.CompanyId, userId);
                #endregion
                result.IsSuccess = true;
                result.Type = "success";
                return Json(result);
            }
            catch (Exception)
            {
                result.IsSuccess = false;
                result.Type = "error";
                result.Message = "ErrorHappend";
                return Json(result);
            }
        }
    }

}
