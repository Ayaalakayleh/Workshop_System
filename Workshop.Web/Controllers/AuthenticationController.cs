using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.General;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _config;
        private User _oUser;
        private readonly ERPApiClient _erpapicient;

        public AuthenticationController(ILogger<AuthenticationController> logger, IConfiguration config, ERPApiClient eRPApiClient)
        {
            _logger = logger;
            _config = config;
            _erpapicient = eRPApiClient;
        }
        public async Task<IActionResult> Index()
        {
            string token = Request.Cookies["Token"];

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Logout");

            var customAuthorize = new CustomAuthorizeAttribute();

            if (!customAuthorize.VeirifyJWTToken(token, _config, _logger))
                return RedirectToAction("Logout");

            var jwt = new JWT_Token();
            var userToken = jwt.ValidateJwtToken(token);

            _oUser = await _erpapicient.GetUserInfoByUsername(userToken.UserName);
            var company = await _erpapicient.GetCompanyById(userToken.CompanyId);

            HttpContext.Session.SetInt32("CompanyId", company.Id);
            HttpContext.Session.SetString("CompanyInfo", System.Text.Json.JsonSerializer.Serialize(company));
            HttpContext.Session.SetString("Role", _oUser.Role_Id.ToString());
            HttpContext.Session.SetString("UserGroupId", _oUser.UserGroupId.ToString());
            HttpContext.Session.SetInt32("UserId", _oUser.UserID);

            HttpContext.Session.SetString("UserInfo", System.Text.Json.JsonSerializer.Serialize(_oUser));
            var branchInfo = await _erpapicient.GetBranchById(userToken.BranchId);
            HttpContext.Session.SetInt32("BranchId", userToken.BranchId);
            HttpContext.Session.SetString("BranchInfo", System.Text.Json.JsonSerializer.Serialize(branchInfo));

            if (Request.Cookies["URL"] != null)
            {
                TempData["URL"] = Request.Cookies["URL"];
                Response.Cookies.Delete("URL");
            }
            else
            {

                return RedirectToAction("SelectBranchLink", new { branch = userToken.BranchId });
            }

            return RedirectToAction("SelectBranchLink", new { branch = userToken.BranchId });
        }
        public async Task<IActionResult> Logout()
        {
            try
            {
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }

                HttpContext.Session.Clear();

                string logoutUrl = _config["ApiSettings:SSOLogOut"];
                return Redirect(logoutUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout Error");
                throw;
            }
        }
        public async Task<IActionResult> SelectCompany()
        {
            try
            {
                var userJson = HttpContext.Session.GetString("UserInfo");
                if (string.IsNullOrEmpty(userJson))
                    return RedirectToAction("Logout");

                var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson);

                var companies = await _erpapicient.GetUserCompanies(user.UserID);

                return View(companies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelectCompany Error");
                throw;
            }
        }
        public async Task<IActionResult> SelectCompanyLink(int Company)
        {
            try
            {
                var company = await _erpapicient.GetCompanyById(Company);

                HttpContext.Session.SetInt32("Company", company.Id);
                HttpContext.Session.SetString("CompanyInfo", System.Text.Json.JsonSerializer.Serialize(company));

                return RedirectToAction("SelectBranch");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelectCompanyLink Error");
                throw;
            }
        }
        public async Task<IActionResult> SelectBranch(string URL)
        {
            try
            {
                int companyId = HttpContext.Session.GetInt32("CompanyId") ?? 0;

                var UserID = HttpContext.Session.GetInt32("UserId") ?? 0;

                var branches = await _erpapicient.GetBranchesByCompanyIdUserId(companyId, UserID);

                if (!string.IsNullOrEmpty(URL))
                    TempData["URL"] = URL;

                return View(branches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelectBranch Error");
                throw;
            }
        }
        public async Task<IActionResult> SelectBranchLink(int branch)
        {
            try
            {
                int companyId = HttpContext.Session.GetInt32("CompanyId") ?? 0;

                var userJson = HttpContext.Session.GetString("UserInfo");
                var user = System.Text.Json.JsonSerializer.Deserialize<User>(userJson);
                var branchInfo = await _erpapicient.GetBranchById(branch);
                HttpContext.Session.SetInt32("BranchId", branch);
                HttpContext.Session.SetString("BranchInfo", System.Text.Json.JsonSerializer.Serialize(branchInfo));

                // Load user permission/menu
                var permissionData = await _erpapicient.GetUserPermission(
                    user.UserID, "11", companyId, branch
                );

                HttpContext.Session.SetString("Permission", permissionData?.Permissions ?? "");
                HttpContext.Session.SetString("UserGroupId", permissionData?.Groups ?? "");

                var primaryMenu = await _erpapicient.GetUserMenu("en", user.UserID, "11", companyId, branch);

                // Inject Consumption Report if not exists
                //if (primaryMenu != null && !primaryMenu.Any(m => m.MethodName == "ConsumptionReport"))
                //{
                //    // Find parent (Reports section)
                //    // Try to find an existing report item to get the parent ID
                //    var existingReport = primaryMenu.FirstOrDefault(m => m.ControllerName == "Reports");
                //    long parentId = -1;
                //    if (existingReport != null)
                //    {
                //        parentId = existingReport.ParentId;
                //    }
                //    else
                //    {
                //        // Fallback: look for a top-level menu named "Reports"
                //        var reportsGroup = primaryMenu.FirstOrDefault(m => (m.PrimaryName != null && m.PrimaryName.Contains("Report")) && m.ParentId == -1);
                //        if (reportsGroup != null) parentId = reportsGroup.Id;
                //    }

                //    int newId = primaryMenu.Any() ? primaryMenu.Max(m => m.Id) + 1 : 99990;

                //    primaryMenu.Add(new Menu
                //    {
                //        Id = newId,
                //        ParentId = parentId,
                //        PrimaryName = "Consumption Report",
                //        SecondarName = "تقرير الاستهلاك",
                //        Icon = "fal fa-chart-pie",
                //        ControllerName = "Reports",
                //        MethodName = "ConsumptionReport",
                //        Seq = 100,
                //        ModuleId = 11,
                //        UserId = user.UserID,
                //        MenuList = new List<Menu>()
                //    });
                //}

                HttpContext.Session.SetString("PrimaryMenuList", System.Text.Json.JsonSerializer.Serialize(primaryMenu));

                // If deep link exists
                if (TempData["URL"] != null)
                {
                    string token = await _erpapicient.GetJWT_TokenUser(
                        user.UserID,
                        user.Username,
                        companyId,
                        branch,
                        user.Role_Id,
                        Request.Cookies["Token"]
                    );

                    Response.Cookies.Delete("Token");

                    Response.Cookies.Append("Token", token, new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddDays(1),
                        HttpOnly = true,
                        Secure = false
                    });

                    return Redirect(TempData["URL"].ToString());
                }

                return RedirectToAction("Index", "QuickAccess");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SelectBranchLink Error");
                return RedirectToAction("Index", "QuickAccess");
            }
        }
        public async Task<IActionResult> SelectModules()
        {
            int companyId = HttpContext.Session.GetInt32("CompanyId") ?? 0;
            int branchId = HttpContext.Session.GetInt32("BranchId") ?? 0;

            var UserID = HttpContext.Session.GetInt32("UserId") ?? 0;
            var modules = await _erpapicient.GetUserModule(UserID, companyId, branchId);
            return View(modules);
        }
        public IActionResult SelectModuleLink(string Url)
        {
            return Redirect(Url);
        }
        public IActionResult LogoutProgrammatically()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet("/Authentication/EmbedVerify")]
        public async Task<IActionResult> EmbedVerify(int userId, string contactHash, string returnUrl)
        {
            // 1) Validate returnUrl 
            if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return BadRequest("Invalid returnUrl");

            // 2) Validate inputs
            if (userId <= 0 || string.IsNullOrWhiteSpace(contactHash))
                return BadRequest("Missing parameters");

            // 3) Get user info from DB/API by userId (NOT from cookies / NOT from JWT)
            var user = await _erpapicient.GetUserInfoById(userId);
            if (user == null)
                return Unauthorized();

            // 4) Regenerate expected hash using same secrets + same logic
            string prefix = _config["EmbedAuth:PrefixPassword"] ?? "";
            string suffix = _config["EmbedAuth:SuffixPassword"] ?? "";

            string expectedHash = EmbedHashService.Generate(user.Email, user.PhoneNo, user.UserID, prefix, suffix);

            // 5) Timing-safe compare
            if (!EmbedHashService.FixedTimeEqualsHex(expectedHash, contactHash))
                return Unauthorized();

            // 6) Session registration (must satisfy SessionTimeout)
            HttpContext.Session.SetInt32("UserId", user.UserID);
            HttpContext.Session.SetString("UserInfo", JsonSerializer.Serialize(user));
            HttpContext.Session.SetString("Role", user.Role_Id.ToString());
            HttpContext.Session.SetInt32("UserGroupId", user.UserGroupId);

            // 1. Get a company for this user (choose default/first)
            var companies = await _erpapicient.GetUserCompanies(user.UserID);
            var company = companies?.FirstOrDefault();
            if (company == null) return Unauthorized("No company for user");

            HttpContext.Session.SetInt32("CompanyId", company.Id);
            HttpContext.Session.SetString("CompanyInfo", JsonSerializer.Serialize(company));

            // 2. Get a branch for this user/company (choose default/first)
            var branches = await _erpapicient.GetBranchesByCompanyIdUserId(company.Id, user.UserID);
            var branch = branches?.FirstOrDefault();
            if (branch == null) return Unauthorized("No branch for user");

            HttpContext.Session.SetInt32("BranchId", branch.BranchID);
            HttpContext.Session.SetString("BranchInfo", JsonSerializer.Serialize(branch));
            var primaryMenu = await _erpapicient.GetUserMenu("en", user.UserID, "11", company.Id, branch.BranchID);
            HttpContext.Session.SetString("PrimaryMenuList", System.Text.Json.JsonSerializer.Serialize(primaryMenu));

            HttpContext.Session.GetString("UserGroupId");
            HttpContext.Session.SetInt32("CurrencyId", branch.CurrencyIDH);

            // 7) Redirect to internal page
            return Redirect(returnUrl);
        }

    }
}
