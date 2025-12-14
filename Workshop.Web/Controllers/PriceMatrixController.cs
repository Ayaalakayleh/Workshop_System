using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Workshop.Core.DTOs;
using Workshop.Domain.Enum;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class PriceMatrixController : BaseController
    {
        private readonly AccountingApiClient _accountingApiClient;
        private readonly WorkshopApiClient _apiClient;
        private readonly VehicleApiClient _vehicleApiClient;
        private readonly string lang;
        public PriceMatrixController(AccountingApiClient accountingApiClient, WorkshopApiClient apiClient, 
            VehicleApiClient vehicleApiClient, IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
            _accountingApiClient = accountingApiClient;
            _apiClient = apiClient;
            _vehicleApiClient = vehicleApiClient;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.PriceMatrix.View)]
        public async Task<IActionResult> Index(PriceMatrixModel? priceMatrixModel)
        {
            ViewBag.AccountTypes = Enum.GetValues(typeof(AccountTypeEnum)).Cast<AccountTypeEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList() ?? new List<SelectListItem>();

          
            ViewBag.SalesType = new List<SelectListItem>();
            var allCustomers = await _accountingApiClient.Customer_GetAll(CompanyId, BranchId, 1, lang);
            ViewBag.Customers = allCustomers.Select(c => new SelectListItem
            { Value = c.Id.ToString(), Text = c.CustomerName }).ToList();
            ViewBag.MatchValues = new List<int>();

            if (priceMatrixModel == null)
            {
                priceMatrixModel = new PriceMatrixModel();
                priceMatrixModel.PriceMatrixFilter = new PriceMatrixFilter();
                priceMatrixModel.PriceMatrixList = new List<GetPriceMatrixDTO>();
            }
            if (priceMatrixModel.PriceMatrixFilter == null)
            {
                priceMatrixModel.PriceMatrixFilter = new PriceMatrixFilter();
            }
            if (priceMatrixModel.PriceMatrixList == null)
            {
                priceMatrixModel.PriceMatrixList = new List<GetPriceMatrixDTO>();
            }

            //if (priceMatrixModel?.PriceMatrixFilter?.Name == null) priceMatrixModel.PriceMatrixFilter.Name = String.Empty;
            //if (priceMatrixModel.PriceMatrixFilter.AppliesTo == null) priceMatrixModel.PriceMatrixFilter.AppliesTo = String.Empty;
            if (priceMatrixModel.PriceMatrixFilter.Basis == null) priceMatrixModel.PriceMatrixFilter.Basis = Basis.All;

            // Ensure paging defaults
            priceMatrixModel.PriceMatrixFilter.PageNumber = priceMatrixModel.PriceMatrixFilter.PageNumber <= 0 ? 1 : priceMatrixModel.PriceMatrixFilter.PageNumber;
            priceMatrixModel.PriceMatrixFilter.PageSize = priceMatrixModel.PriceMatrixFilter.PageSize <= 0 ? 25 : priceMatrixModel.PriceMatrixFilter.PageSize;

            // Call paged API
            var paged = await _apiClient.GetAllPricesPaged(priceMatrixModel.PriceMatrixFilter);

            var pa = await _apiClient.GetAllPrices(priceMatrixModel.PriceMatrixFilter);

            var prices = paged?.Items?.ToList() ?? new List<GetPriceMatrixDTO>();

            // enrich match value text pairs
            FilterRTSCodeDTO filterRTSCodeDTO = new FilterRTSCodeDTO();
            var filteredRTSCodes = await _apiClient.GetAllRTSCodesDDLAsync();
            var skills = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(1, CompanyId);
            var manufacturers = await _vehicleApiClient.GetAllManufacturers();

            foreach (var item in prices)
            {
                if (item.BasisId == (int)Basis.RTS)
                {
                    foreach (var filteredRTSCode in filteredRTSCodes.Where(s => item.MatchValue.Contains(s.Id)).Select(s => lang == "en" ? s.PrimaryName : s.SecondaryName))
                        item.MatchValueTextPair.Add(new KeyValuePair<int, string>(item.Id, filteredRTSCode));
                }
                else if (item.BasisId == (int)Basis.Skill && skills != null)
                {
                    foreach (var skill in skills.Where(s => item.MatchValue.Contains(s.Id)).Select(s => lang == "en" ? s.PrimaryName : s.SecondaryName))
                        item.MatchValueTextPair.Add(new KeyValuePair<int, string>(item.Id, skill));
                }
                else if (item.BasisId == (int)Basis.Franchise && manufacturers != null)
                {
                    foreach (var manu in manufacturers.Where(s => item.MatchValue.Contains(s.Id)).Select(s => lang == "en" ? s.ManufacturerPrimaryName : s.ManufacturerSecondaryName))
                        item.MatchValueTextPair.Add(new KeyValuePair<int, string>(item.Id, manu));
                }

            }

            priceMatrixModel.PriceMatrixList = prices;
            priceMatrixModel.TotalPages = paged?.TotalPages ?? 1;
            priceMatrixModel.PriceMatrixFilter.PageNumber = paged?.CurrentPage ?? priceMatrixModel.PriceMatrixFilter.PageNumber;
            priceMatrixModel.PriceMatrixFilter.PageSize = paged?.PageSize ?? priceMatrixModel.PriceMatrixFilter.PageSize;

            return View(priceMatrixModel);
        }

        public async Task<IEnumerable<Object>> SetBasis(int id)
        {
            GetPriceMatrixDTO getPriceMatrixDTO = new GetPriceMatrixDTO();
            var prices = await _apiClient.GetAllPrices(new PriceMatrixFilter());
            var fRTSCodes = await _apiClient.GetAllRTSCodesDDLAsync();
            var skills = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(1, CompanyId);
            var manufacturers = await _vehicleApiClient.GetAllManufacturers();

            if (id == (int)Basis.RTS)
            {
                FilterRTSCodeDTO filterRTSCodeDTO = new FilterRTSCodeDTO();

                if (fRTSCodes != null)
                {
                    var excludedIds = prices
                    .Where(p => p.BasisId == (int)Basis.RTS)
                    .SelectMany(p => p.MatchValue)
                    .ToHashSet();

                    var resultData = fRTSCodes
                        .Where(r => !excludedIds.Contains(r.Id))
                        .Select(r => new
                        {
                            Id = r.Id,
                            Name = lang == "en" ? r.PrimaryName : r.SecondaryName
                        })
                        .ToList();
                    return resultData;

                }
                else { return new List<Object>(); }

            }
            else if (id == (int)Basis.Skill)
            {
                if (skills != null)
                {
                    var excludedIds = prices
                        .Where(p => p.BasisId == (int)Basis.Skill)
                        .SelectMany(p => p.MatchValue)
                        .ToHashSet();

                    var resultData = skills
                        .Where(s => !excludedIds.Contains(s.Id))
                        .Select(s => new
                        {
                            Id = s.Id,
                            Name = lang == "en" ? s.PrimaryName : s.SecondaryName
                        })
                        .ToList();
                    return resultData;

                }
                else { return new List<Object>(); }

            }
            else if (id == (int)Basis.Franchise)
            {
                if (manufacturers != null)
                {

                    var excludedIds = prices
                    .Where(p => p.BasisId == (int)Basis.Franchise)
                    .SelectMany(p => p.MatchValue)
                    .ToHashSet();

                    var resultData = manufacturers
                        .Where(m => !excludedIds.Contains(m.Id))
                        .Select(m => new
                        {
                            Id = m.Id,
                            Name = lang == "en" ? m.ManufacturerPrimaryName : m.ManufacturerSecondaryName
                        })
                        .ToList();
                    return resultData;
                }
                else { return new List<Object>(); }
            }
            else
            {
                List<Object> resultList = new List<Object>();
                return resultList;
            }

        }

        [HttpPost]
        public async Task<IEnumerable<object>> GetMatchedValues([FromBody] PriceMatrixRequest request)
        {

            var prices = await _apiClient.GetAllPrices(new PriceMatrixFilter());
            var fRTSCodes = await _apiClient.GetAllRTSCodesDDLAsync();
            var skills = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(1, CompanyId);
            var manufacturers = await _vehicleApiClient.GetAllManufacturers();
            var resultData = new List<object>();

            foreach (var value in request.MatchValue)
            {
                if (request.BasisId == (int)Basis.RTS && fRTSCodes != null)
                {
                    var rts = fRTSCodes
                        .Where(r => r.Id == value)
                        .Select(r => new
                        {
                            Id = r.Id,
                            Name = lang == "en" ? r.PrimaryName : r.SecondaryName
                        })
                        .FirstOrDefault();

                    if (rts != null)
                        resultData.Add(rts);
                }
                else if (request.BasisId == (int)Basis.Skill && skills != null)
                {
                    var skill = skills
                        .Where(s => s.Id == value)
                        .Select(s => new
                        {
                            Id = s.Id,
                            Name = lang == "en" ? s.PrimaryName : s.SecondaryName
                        })
                        .FirstOrDefault();

                    if (skill != null)
                        resultData.Add(skill);
                }
                else if (request.BasisId == (int)Basis.Franchise && manufacturers != null)
                {
                    var manu = manufacturers
                        .Where(m => m.Id == value)
                        .Select(m => new
                        {
                            Id = m.Id,
                            Name = lang == "en" ? m.ManufacturerPrimaryName : m.ManufacturerSecondaryName
                        })
                        .FirstOrDefault();

                    if (manu != null)
                        resultData.Add(manu);
                }
            }

            return resultData;
        }

        public async Task<IEnumerable<Object>> SetAccountType(int id)
        {
            //int CompanyId = 1202;
            //int BranchId = 386;
            //var Accounts = await _accountingApiClient.ChartOfAccountAcceptTransByCompanyIdAndBranchId(CompanyId, BranchId);
            //var AccountType = await _accountingApiClient.TypeSalesPurchases_GetAll(1202, 386, 1, 1);

            //return Accounts
            //.Where(acc => acc.ID == AccountType.FirstOrDefault(ac => ac.Id == id)?.AccountId)
            //.Select(aItem => new { Id = aItem.ID, Name = lang == "en" ? aItem.AccountNameNo : aItem.AccountSecondaryNameNo });
            var result = await GetSalesTypeListAsync(id, CompanyId, lang);
            return result;

        }
        [HttpGet]
        [CustomAuthorize(Permissions.PriceMatrix.Create)]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                // New Price Matrix
                ViewBag.Title = "Create Price Matrix";

            }
            else
            {
                // Edit existing Price Matrix
                ViewBag.Title = "Edit Price Matrix";

            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [CustomAuthorize(Permissions.PriceMatrix.Create)]
        public async Task<IActionResult> EditAsync(PriceMatrixDTO priceMatrixDTO, int id, int Basis, string Applies)
        {
            if (id == 0)
            {
                // New Price Matrix
                ViewBag.Title = "Create Price Matrix";
                var creatPriceMatrix = new CreatePriceMatrixDTO();
                //load data
                creatPriceMatrix.Id = id;
                creatPriceMatrix.Name = priceMatrixDTO.Name;
                creatPriceMatrix.AppliesTo = Applies;
                creatPriceMatrix.BasisId = Basis;
                creatPriceMatrix.AccountType = priceMatrixDTO.AccountType;
                //creatPriceMatrix.SalesType = priceMatrixDTO.SalesType;
                creatPriceMatrix.AccountId = priceMatrixDTO.AccountId;
                creatPriceMatrix.MatchValue = priceMatrixDTO.MatchValue;
                creatPriceMatrix.Customers = priceMatrixDTO.Customers;
                creatPriceMatrix.RatePerHour = priceMatrixDTO.RatePerHour;
                creatPriceMatrix.Markup = priceMatrixDTO.Markup;

                await _apiClient.AddPricesAsync(creatPriceMatrix);
            }
            else
            {
                // Edit existing Price Matrix
                ViewBag.Title = "Edit Price Matrix";

                UpdatePriceMatrixDTO updatePriceMatrix = new UpdatePriceMatrixDTO();
                updatePriceMatrix.Id = id;
                updatePriceMatrix.Name = priceMatrixDTO.Name;
                updatePriceMatrix.AppliesTo = Applies;
                updatePriceMatrix.BasisId = Basis;
                updatePriceMatrix.AccountType = priceMatrixDTO.AccountType;
                //updatePriceMatrix.SalesType = priceMatrixDTO.SalesType;
                updatePriceMatrix.AccountId = priceMatrixDTO.AccountId;
                updatePriceMatrix.MatchValue = priceMatrixDTO.MatchValue;
                updatePriceMatrix.Customers = priceMatrixDTO.Customers;
                updatePriceMatrix.RatePerHour = priceMatrixDTO.RatePerHour;
                updatePriceMatrix.Markup = priceMatrixDTO.Markup;

                await _apiClient.UpdatePricesAsync(updatePriceMatrix);
                // Load the Price Matrix details using the id and pass to the view
            }

            return RedirectToAction(nameof(Index));
        }

        [CustomAuthorize(Permissions.PriceMatrix.Delete)]
        public async Task<IActionResult> DeletePrice(int priceId = 0)
        {
            PriceMatrixDTO priceMatrixDTO = new PriceMatrixDTO();
            priceMatrixDTO.Id = priceId;
            await _apiClient.DeletePricesAsync(priceMatrixDTO);
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Permissions.PriceMatrix.Delete)]
        public async Task<IActionResult> DeletePriceAjax([FromBody] int priceId)
        {
            if (priceId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid price id" });
            }

            var result = await _apiClient.DeletePricesAsync(new PriceMatrixDTO { Id = priceId });
            return Json(new { success = result });
        }

        [HttpGet]
        [CustomAuthorize(Permissions.PriceMatrix.View)]
        public async Task<IActionResult> GetPriceById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid price id" });
            }

            var dto = await _apiClient.GetPrice(new GetPriceMatrixDTO { Id = id });
            return Json(dto);
        }

        private async Task<List<SelectListItem>> GetSalesTypeListAsync(int accountType, int CompanyId, string lang)
        {
            int headerId = accountType == (int)AccountTypeEnum.Internal ? 9 : 10;
            var matches = await _apiClient.GetAllLookupDetailsByHeaderIdAsync(headerId, CompanyId);

            return matches.Select(sc => new SelectListItem
            {
                Value = sc.Id.ToString(),
                Text = lang == "en" ? sc.PrimaryName : sc.SecondaryName
            }).ToList();
        }

    }
}
