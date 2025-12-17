using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Net;
using System.Reflection;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.City;
using Workshop.Core.DTOs.General;
using Workshop.Web.Models;

namespace Workshop.Web.Services
{
    public class ERPApiClient : BaseApiClient
    {

        public ERPApiClient(
            HttpClient httpClient,
            IConfiguration config,
            IApiAuthStrategy apiAuthStrategy
            ) : base(
                httpClient,
                new ApiSettings
                {
                    ApiUser = config.GetValue<string>("ApiSettings:ERPApiUser"),
                    ApiPassword = config.GetValue<string>("ApiSettings:ERPApiPassword"),

                }, apiAuthStrategy)
        { }

        public async Task<List<Department>?> GetAllDepartmentAsync(int CompanyId, string Language)
        {
            string url = $"/Users/GetCompanyDepartment?CompanyId={CompanyId}&Language={Language}";
            return await SendRequest<List<Department>>(url, HttpMethod.Get);
        }

        public async Task<List<City>?> GetCities(int? areaId, string lang = "en")
        {
            string url = $"/Users/GetCities_ByAreaId?AreaId={areaId}&Language={lang}";
            return await SendRequest<List<City>>(url, HttpMethod.Get);
        }

        public async Task<List<CompanyBranch>> GetActiveBranchesByCompanyId(int companyId, string lang = "en")
        {
            string url = $"/Users/Company_Branches_Active?CompanyId={companyId}&lang={lang}";
            return await SendRequest<List<CompanyBranch>>(url, HttpMethod.Get);
        }

        public async Task<Notification> Notification_Insert(Notification notification)
        {

            string url = $"/Notification/Notification_Insert";
            return await SendRequest<Notification>(url, HttpMethod.Post, notification);
        }

        public async Task<List<CurrencyDTO>> GetCurrecy(int CompanyId, int BranchId, string lang)
        {
            string url = $"/Currencies/Currencies?CompanyId={CompanyId}&BranchId={BranchId}&lang={lang}";
            return await SendRequest<List<CurrencyDTO>>(url, HttpMethod.Get);
        }
        public async Task<List<CompanyInfo>> GetCompaniesInfo(int userId)
        {
            string url = $"/Users/GetUserCompanies?UserId={userId}";
            return await SendRequest<List<CompanyInfo>>(url, HttpMethod.Get);
        }


        public async Task<User?> GetUserInfoByUsername(string username)
        {
            try
            {
                // 1. Create authorized request (GET)
                string url = $"/Users/GetUserInfoByUsername?Username={username}";
                return await SendRequest<User>(url, HttpMethod.Get);


            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetUserInfoByUsername. Username={username}", username);

                return null;
            }
        }

        public async Task<CompanyInfo?> GetCompanyById(int id)
        {
            try
            {
                // 1. Create authorized GET request
                string url = $"/Users/CompaniesLink?CompanyId={id}";
                return await SendRequest<CompanyInfo>(url, HttpMethod.Get);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error occurred in GetCompanyById. CompanyId={id}", id);

                return null;
            }
        }
        public async Task<CompanyBranch?> GetBranchById(int id)
        {
            try
            {
                // 1. Create authorized GET request
                string url = $"/Users/BranchesLink?BranchId={id}";
                return await SendRequest<CompanyBranch>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error occurred in GetCompanyById. CompanyId={id}", id);

                return null;
            }
        }

        public async Task<List<CompanyInfo>?> GetUserCompanies(int userId)
        {
            try
            {
                // 1. Build the authorized GET request
                string url = $"/Users/GetUserCompanies?UserId={userId}";
                return await SendRequest<List<CompanyInfo>>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetUserCompanies. UserId={userId}", userId);

                return null;
            }
        }

        public async Task<List<CompanyBranch>?> GetBranchesByCompanyIdUserId(int companyId, int userId)
        {
            try
            {
                // 1. Build the authorized GET request
                string url = $"/Users/BranchesByUser?CompanyId={companyId}&UserId={userId}&BranchType=0";
                return await SendRequest<List<CompanyBranch>>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetBranchesByCompanyIdUserId. CompanyId={companyId}, UserId={userId}",
                //    companyId, userId);

                return null;
            }
        }

        public async Task<UserPermissionGroup?> GetUserPermission(int userId, string moduleId, int companyId, int branchId)
        {
            try
            {
                // Build request
                string url = $"/Users/GetUserPermission?userId={userId}&ModulesId={moduleId}&companyId={companyId}&branchId={branchId}";
                return await SendRequest<UserPermissionGroup>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetUserPermission (userId={userId}, moduleId={moduleId}, companyId={companyId}, branchId={branchId})",
                //    userId, moduleId, companyId, branchId);

                return null;
            }
        }

        public async Task<List<Menu>?> GetUserMenu(string lang, int userId, string moduleId, int companyId, int branchId)
        {
            try
            {
                // Build request
                string url = $"/Users/GetUserMenu?language={lang}&UserId={userId}&ModuleId={moduleId}&companyId={companyId}&branchId={branchId}";
                return await SendRequest<List<Menu>?>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetUserMenu (lang={lang}, userId={userId}, moduleId={moduleId}, companyId={companyId}, branchId={branchId})",
                //    lang, userId, moduleId, companyId, branchId);

                return null;
            }
        }

        public async Task<string?> GetJWT_TokenUser(int userId, string userName, int companyId, int branchId, int roleId, string oldToken)
        {
            try
            {
                // Build authorized GET request
                string url = $"/Users/GetJWT_TokenUser?UserId={userId}&UserName={userName}&CompanyId={companyId}&BranchId={branchId}&Role_Id={roleId}&OldToken={oldToken}";
                return await SendRequest<string>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetJWT_TokenUser (userId={userId}, userName={userName}, companyId={companyId}, branchId={branchId}, roleId={roleId})",
                //    userId, userName, companyId, branchId, roleId);

                return null;
            }
        }

        public async Task<List<Modules>?> GetUserModule(int userId, int companyId, int branchId)
        {
            try
            {
                // Build authorized GET request
                string url = $"/Users/GetUserModule?UserId={userId}&companyId={companyId}&branchId={branchId}";
                return await SendRequest<List<Modules>?>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetUserModule (userId={userId}, companyId={companyId}, branchId={branchId})",
                //    userId, companyId, branchId);

                return null;
            }
        }

        public async Task<List<Workshop.Core.DTOs.Groups>> GetAllNotificationGroups(string lang, int branchId, int companyId)
        {
            string url = $"/Users/GetAllGroups?lang={1}&BranchId={2}&CompanyId={3}";
            return await SendRequest<List<Workshop.Core.DTOs.Groups>>(url, HttpMethod.Get);

        }

        public async Task<List<ModulesDTO>> GetAllModules()
        {
            string url = $"/Users/GetAllModules";
            return await SendRequest<List<ModulesDTO>>(url, HttpMethod.Get);
        }

        public async Task<User?> GetUserInfoById(int Id)
        {
            try
            {
                // Build request
                string url = $"/Users/GetUserInfoById?Id={Id}";
                return await SendRequest<User>(url, HttpMethod.Get);

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Error in GetUserPermission (userId={userId}, moduleId={moduleId}, companyId={companyId}, branchId={branchId})",
                //    userId, moduleId, companyId, branchId);

                return null;
            }
        }

    }
}
