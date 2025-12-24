using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using NPOI.Util;
using System.Threading.Tasks;
using Workshop.Core.DTOs;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class AccountDefinitionController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly WorkshopApiClient _apiClient;
        private readonly AccountingApiClient _accountingApiClient;

        public readonly string lang;


        public AccountDefinitionController(IConfiguration configuration, IWebHostEnvironment env, WorkshopApiClient apiClient, AccountingApiClient accountingApiClient, IMemoryCache cache)
            : base(cache, configuration, env)
        {
            _configuration = configuration;
            _env = env;
            _apiClient = apiClient;
            _accountingApiClient = accountingApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;

        }

        [CustomAuthorize(Permissions.AccountDefinitions.View)]
        public async Task<IActionResult> Index()
        {
            var branchId = BranchId;
            var companyId = CompanyId;


            AccountDefinitionDTO dto = new AccountDefinitionDTO();


            var data = await _apiClient.GetAccountDefinitionGetAsync(companyId);
            if (data != null)
            {
                dto = data;
            }

            var accountsList = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(companyId, branchId, lang);
            ViewBag.Accounts = accountsList.Select(t => new SelectListItem
            {
                Text = t.AccountName,
                Value = t.ID.ToString(),
                Selected =
                    t.ID == dto.JournalId ||
                    t.ID == dto.WIPAccountId ||
                    t.ID == dto.MaintenanceAccountId ||
                    t.ID == dto.AccessoriesAccountId ||
                    t.ID == dto.AccidentAccountId ||
                    t.ID == dto.MaintenanceProjectsAccountId ||
                    t.ID == dto.InternalCostPartId ||
                    t.ID == dto.InternalCostLabourId ||
                    t.ID == dto.ExternalCostPartId ||
                    t.ID == dto.ExternalCostLabourId ||
                    t.ID == dto.InternalRevenuePartId ||
                    t.ID == dto.InternalRevenueLabourId
            }).ToList();

            var invoiceTypes = await _accountingApiClient.TypeSalesPurchases_GetAll(companyId, branchId, 1, null);
            ViewBag.InvoiceTypes = invoiceTypes.Select(t => new SelectListItem
            {
                Text = lang == "en" ? t.PrimaryName : t.SecondaryName,
                Value = t.Id.ToString(),
                Selected = t.Id == dto.InvoiceTypeId
            }).ToList();

            var directExpenseTypes = (await _accountingApiClient.ExpenseType_Get(CompanyId))
                .Where(x => x.FK_TypeOfExpenseId == 1)
                .ToList();
            ViewBag.directExpenseTypes = directExpenseTypes.Select(t => new SelectListItem
            {
                Text = lang == "en" ? t.PrimaryName : t.SecondaryName,
                Value = t.Id.ToString(),
                Selected = t.Id == dto.PettyCashExpenseTypeId
            }).ToList();

            var TransTypeTable = await _accountingApiClient.TransactionType(companyId, branchId, lang);
            var _TransTypeTable = TransTypeTable.Where(x => x.IsAutoCreated == true && x.VoucherType == 1 && !new long[] { 6, 7, 8, 9, 10, 11, 12, 13, 3120, 3121, 3122 }.Contains(x.ID)).ToList();
            ViewBag.TransTypeTable = _TransTypeTable.Select(t => new SelectListItem
            {
                Text = t.TransType,
                Value = t.ID.ToString(),
                Selected = t.ID == dto.JournalId 
            }).ToList();
            return View(dto);
        }


        [HttpPost]
        [CustomAuthorize(Permissions.AccountDefinitions.Create)]
        public async Task<IActionResult> Edit(AccountDefinitionDTO dto)
        {
            var success=0;
            dto.CompanyId = CompanyId;

            if(dto.Id == 0)
            {
                dto.CreatedBy = UserId; 
                success = await _apiClient.AddAccountDefinitionAsync(dto)??0;
            }
            else
            {
                dto.UpdatedBy = UserId; 
                success = await _apiClient.UpdateAccountDefinitionAsync(dto)??0;
            }
            if (success > 0)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }
    }
}
