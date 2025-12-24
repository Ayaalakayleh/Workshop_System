using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System.Net;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Vehicle;
using Workshop.Web.Models;

namespace Workshop.Web.Services
{
    public class VehicleApiClient : BaseApiClient
    {

        public VehicleApiClient(
            HttpClient httpClient,
            IConfiguration config,
            IApiAuthStrategy apiAuthStrategy
            ) : base(
                httpClient,
                new ApiSettings
                {
                    ApiUser = config.GetValue<string>("ApiSettings:VehicleApiUser"),
                    ApiPassword = config.GetValue<string>("ApiSettings:VehicleApiPassword"),

                }, apiAuthStrategy)
        {
        }

        public async Task<List<CustomerInformationSummery>> GetInsuranceCompanySummery(int companyId, string lang = "en")
        {

            string url = $"/CustomerInformation/GetInsuranceCompanySummery?CompanyId={companyId}&language={lang}";
            return await SendRequest<List<CustomerInformationSummery>>(url, HttpMethod.Get);
        }

        public async Task<List<Manufacturers>> GetAllManufacturers(string lang = "en")
        {

            string url = $"/Manufacturers/GetAllManufacturers?language={lang}";
            return await SendRequest<List<Manufacturers>>(url, HttpMethod.Get);
        }

        public async Task<List<VehicleDefinitions>> VehicleDefinitions_GetExternalWSVehicles(int page, int? manufacturerId, string platenumber, int? modelId, string ChassisNo = null)
        {

            string url = $"/VehicleDefinition/VehicleDefinitions_GetExternalWSVehicles?page={page}&manufacturerId={manufacturerId}&platenumber={platenumber}&modelId={modelId}&ChassisNo={ChassisNo}";
            return await SendRequest<List<VehicleDefinitions>>(url, HttpMethod.Get);
        }

        public async Task<List<VehicleModel>> GetAllVehicleModel(int manufacturerId, string language)
        {

            string url = $"/VehicleModel/GetById?id={manufacturerId}&language={language}";
            return await SendRequest<List<VehicleModel>>(url, HttpMethod.Get);
        }

        public async Task<List<VehicleDefinitions>> GetWorkshopVehicles(VehicleAdvancedFilter vehicle)
        {

            string url = $"/VehicleDefinition/GetWorkshopVehicle";
            return await SendRequest<List<VehicleDefinitions>>(url, HttpMethod.Post, vehicle);
        }

        public async Task<VehicleDefinitions> GetVehicleDetails(int id, string language)
        {

            string url = $"/VehicleDefinition/GetVehicleDetails?Id={id}&language={language}";
            return await SendRequest<VehicleDefinitions>(url, HttpMethod.Get);
        }

        public async Task<VehicleDefinitions> VehicleDefinitions_Find(int VehicleId)
        {

            string url = $"/VehicleDefinition/VehicleDefinitions_Find?VehicleId={VehicleId}";
            return await SendRequest<VehicleDefinitions>(url, HttpMethod.Get);
        }

        public async Task<VehicleDefinitions> GetExternalVehicleDetails(int id, string language)
        {

            string url = $"/VehicleDefinition/GetExternalVehicleDetails?Id={id}&language={language}";
            return await SendRequest<VehicleDefinitions>(url, HttpMethod.Get);

        }

        public async Task<List<VehicleNams>> GetExteralVehicleName(string language)
        {
            string url = $"/VehicleDefinition/GetExteralVehicleName?language={language}";
            return await SendRequest<List<VehicleNams>>(url, HttpMethod.Get);
        }

        public async Task<List<Agreement>> GetLastAgreement(string lang, int vehicleId)
        {
            string url = $"/Agreement/GetLastAgreement?VehicleId={vehicleId}&language={lang}";
            return await SendRequest<List<Agreement>>(url, HttpMethod.Get);
        }

        public async Task<Agreement> GetAgreement(int AgreementId, string language)
        {

            string url = $"/Agreement/GetAgreement?AgreementId={AgreementId}&language={language}";
            return await SendRequest<Agreement>(url, HttpMethod.Get);

        }

        public async Task<VehicleMovement> GetVehicleMovement(int vehicleId)
        {
            string url = $"/Movement/GetVehicleMovements?VehicleId={vehicleId}";
            return await SendRequest<VehicleMovement>(url, HttpMethod.Get);
        }

        public async Task<CreateVehicleDefinitionsModel> VehicleDefinitions_GetExternalWSVehicleById(int id)
        {

            string url = $"/VehicleDefinition/VehicleDefinitions_GetExternalWSVehicleById?Id={id}";
            return await SendRequest<CreateVehicleDefinitionsModel>(url, HttpMethod.Get);
        }

        public async Task<List<M_VehicleColor>> GetAllColors(string language)
        {

            string url = $"/VehicleColor/VehicleColor_GetAll?lang={language}";
            return await SendRequest<List<M_VehicleColor>>(url, HttpMethod.Get);
        }

        public async Task<List<VehicleNams>> GetVehiclesDDL(string language, int companyId, int? vehicleId = null)
        {

            string url = $"/VehicleDefinition/GetVehicleName?language={language}&CompanyId={companyId}&VehicleId={vehicleId}";
            return await SendRequest<List<VehicleNams>>(url, HttpMethod.Get);
        }

        public async Task<int> InsertExternalVehicle(CreateVehicleDefinitionsModel vehicleDefinitions)
        {

            string url = $"/VehicleDefinition/InsertExternalWSVehicle";
            return await SendRequest<int>(url, HttpMethod.Post, vehicleDefinitions);
        }

        public async Task<List<VehicleDefinitions>> GetVehiclesPlatesByCompanyId(int companyId)
        {

            string url = $"/VehicleDefinition/GetVehiclesPlatesByCompanyId?CompanyId={companyId}";
            return await SendRequest<List<VehicleDefinitions>>(url, HttpMethod.Get);
        }

        public async Task<List<VehicleClass>> GetAllVehicleClass(string lang = "en")
        {

            string url = $"/VehicleClass/GetAllVehicleClass?language={lang}";
            return await SendRequest<List<VehicleClass>>(url, HttpMethod.Get);
        }

        public async Task<VehicleMovement> GetLastMaintenanceMovement(int vehicleId)
        {

            string url = $"/Movement/GetLastMaintenanceMovement?VehicleId={vehicleId}";
            return await SendRequest<VehicleMovement>(url, HttpMethod.Get);
        }
        public async Task<VehicleMovement> GetLastMovementByVehicleId(int vehicleId)
        {

            string url = $"/VehicleMovements/GetLastMovement?VehicleId={vehicleId}";
            return await SendRequest<VehicleMovement>(url, HttpMethod.Get);
        }

        public async Task<ApiResponse<bool?>> UpdateVehicleStatus(int id, int statusId, int? subStatusId = null)
        {

            string url = $"/VehicleDefinition/UpdateVehicleStatus?Id={id}&statusId={statusId}&SubStatusId={subStatusId}";
            return await SendRequest<ApiResponse<bool?>>(url, HttpMethod.Get);
        }

        public async Task<Notification> Notification_Insert(Notification notification)
        {

            string url = $"/Notification/Notification_Insert";
            return await SendRequest<Notification>(url, HttpMethod.Post, notification);
        }

        public async Task<List<VehicleSubStatus>> GetAllSubStatus(int companyId, string language)
        {

            string url = $"/SubStatus/GetAllSubStatus?CompanyId={companyId}&language={language}";
            return await SendRequest<List<VehicleSubStatus>>(url, HttpMethod.Get);
        }

        public async Task<List<FuleLevel>> GetFuleLevel()
        {

            string url = $"/Workshop/GetFuleLevel";
            return await SendRequest<List<FuleLevel>>(url, HttpMethod.Get);
        }

        public async Task<VehicleDefinitions> M_VehicleDefinitionsGetPlateNumberCostCenterById(int id)
        {

            string url = $"/VehicleDefinition/M_VehicleDefinitionsGetPlateNumberCostCenterById?Id={id}";
            return await SendRequest<VehicleDefinitions>(url, HttpMethod.Get);
        }
       /* public async Task<VehicleDefinitions> VehicleDefinitions_Find(int vehicleId)
        {

            string url = $"/VehicleDefinition/VehicleDefinitions_Find?VehicleId={vehicleId}";
            return await SendRequest<VehicleDefinitions>(url, HttpMethod.Get);
        }*/



        public async Task<VehicleDocuments> Documants_GetByVehicleIdAndSystemTypeId(int VehicleId, int FileSystemId)
        {
            string url = $"/VehicleDefinition/Documants_GetByVehicleIdAndSystemTypeId?VehicleId={VehicleId}&FileSystemId={FileSystemId}";
            return await SendRequest<VehicleDocuments>(url, HttpMethod.Get);
        }

        public async Task<CustomerInformation> CustomerInformationDetails_GetByAgreementId(int Id)
        {
            string url = $"/Customers/CustomerInformationDetails_GetByAgreementId?AgreementId={Id}";
            return await SendRequest<CustomerInformation>(url, HttpMethod.Get);
        }
        public async Task<List<Agreement>> GetAgreementbyVehicleId(int id)
        {
            string url = $"/Agreement/GetAgreementbyVehicleId?VehicleDefinitionId={id}";
            return await SendRequest<List<Agreement>>(url, HttpMethod.Get);
        }
        public async Task<Agreement> GetActiveAgreementId(int id)
        {
            string url = $"/Agreement/GetActiveAgreementId?VehicleDefinitionId={id}";
            return await SendRequest<Agreement>(url, HttpMethod.Get);
        }
        public async Task<VehicleDefinitions> VehicleDefinitionsGetByChassisNo(string chassisNo)
        {
            string url = $"/VehicleDefinition/VehicleDefinitionsGetByChassisNo?chassisNo={chassisNo}";
             var result = await SendRequest<VehicleDefinitions>(url, HttpMethod.Get);
            return result;
        }
        public async Task<List<VehicleDefinitions>> GetChassiDDL(int CompanyId, int vehicleType)
        {
            string url = $"/VehicleDefinition/GetChassiDDL?CompanyId={CompanyId}&vehicleType={vehicleType}";
            var result = await SendRequest<List<VehicleDefinitions>>(url, HttpMethod.Get);
            return result;
        }
        public async Task<List<CustomerInformation>> Get_CustomerInformation(int BranchId, string Language, string search = null)
        {
            string url = $"/CustomerInformation/Get_CustomerInformation?BranchId={BranchId}&Language={Language}&search={search}";
            var result = await SendRequest<List<CustomerInformation>>(url, HttpMethod.Get);
            return result;
        }
        
        public async Task<List<OpenAgreementInfo>> M_GetOpenAgreementByVehicleOrCustomer(int? customerId, int? vehicleId)
        {
            string url = $"/CustomerInformation/GetOpenAgreementByVehicleOrCustomer?customerId={customerId}&vehicleId={vehicleId}";
            var result = await SendRequest<List<OpenAgreementInfo>>(url, HttpMethod.Get);
            return result;
        }
        public async Task<CustomerInformation> GetCustomerData(int customerId)
        {
            string url = $"CustomerInformation/GetCustomerData?id={customerId}";
            var result = await SendRequest<CustomerInformation>(url, HttpMethod.Get);
            return result;
        }
    }
}
