using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Globalization;
using System.Linq;
using Workshop.Core.DTOs;
using Workshop.Web.Models;
using Workshop.Web.Services;

namespace Workshop.Web.Controllers
{
    [SessionTimeout]
    public class TeamsController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly WorkshopApiClient _teamsService;
        private static readonly string INDEX_PAGE = "Index";
        public readonly string lang;

        public TeamsController(IConfiguration configuration, IWebHostEnvironment env, WorkshopApiClient teamsService, IMemoryCache cache) : base(cache, configuration, env)
        {
            _configuration = configuration;
            _env = env;
            _teamsService = teamsService;
            this.lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
        }

        [CustomAuthorize(Permissions.Teams.View)]
        public async Task<IActionResult> Index(TeamModel? oFilterTeamDTO)
        {
            oFilterTeamDTO ??= new TeamModel();
            oFilterTeamDTO.TeamFilter ??= new FilterTeamDTO();
            oFilterTeamDTO.PageNumber = oFilterTeamDTO.PageNumber > 0 ? oFilterTeamDTO.PageNumber : 1;

            oFilterTeamDTO.PageSize = oFilterTeamDTO.PageSize ?? 25;

            if (oFilterTeamDTO.TeamFilter.Name == null)
                oFilterTeamDTO.TeamFilter.Name = string.Empty;
            if (oFilterTeamDTO.TeamFilter.Code == null)
                oFilterTeamDTO.TeamFilter.Code = string.Empty;

            var pagedResult = await _teamsService.GetAllTeamsPagedAsync(oFilterTeamDTO.TeamFilter);
            var result = pagedResult?.Teams ?? new List<GetTeamDTO>();

            ViewBag.Teams = result;
            ViewBag.PaginationInfo = pagedResult;
            ViewBag.Filter = oFilterTeamDTO;
            var TechsList = await _teamsService.GetTechniciansDDL(BranchId);
            ViewBag.TechsList = (TechsList ?? Enumerable.Empty<TechnicianDTO>()).Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();
            oFilterTeamDTO.TeamsList = result.ToList();
            oFilterTeamDTO.TotalPages = pagedResult?.TotalPages ?? 1;
            oFilterTeamDTO.PageNumber = pagedResult?.CurrentPage ?? oFilterTeamDTO.PageNumber;
            oFilterTeamDTO.PageSize = pagedResult?.PageSize ?? oFilterTeamDTO.PageSize;
            return View(oFilterTeamDTO);
        }

        //[HttpPost]
        //public async Task<IActionResult> Index(TeamsModel )
        //{
        //    var oFilterTeamDTO = new FilterTeamDTO();
        //    oFilterTeamDTO.Name = name;
        //    oFilterTeamDTO.Code = code;
        //    oFilterTeamDTO.PageNumber = pageNumber ?? 1;
        //    oFilterTeamDTO.PageSize = 25;

        //    if (oFilterTeamDTO.Name == null)
        //        oFilterTeamDTO.Name = string.Empty;
        //    if (oFilterTeamDTO.Code == null)
        //        oFilterTeamDTO.Code = string.Empty;

        //    var pagedResult = await _teamsService.GetAllTeamsPagedAsync(oFilterTeamDTO);
        //    var result = pagedResult?.Teams ?? new List<GetTeamDTO>();

        //    ViewBag.Teams = result;
        //    ViewBag.PaginationInfo = pagedResult;
        //    ViewBag.Filter = oFilterTeamDTO;

        //    var TechsList = await _teamsService.GetTechniciansDDL(0);
        //    ViewBag.TechsList = (TechsList ?? Enumerable.Empty<TechnicianDTO>()).Select(t => new SelectListItem { Text = lang == "en" ? t.PrimaryName : t.SecondaryName, Value = t.Id.ToString() }).ToList();
            
        //    return View(result);
        //}

        [HttpPost]
        public async Task<IActionResult> GetTeamsPartial([FromBody] FilterTeamDTO? oFilterTeamDTO)
        {
            oFilterTeamDTO ??= new FilterTeamDTO();
            oFilterTeamDTO.PageNumber = oFilterTeamDTO.PageNumber ?? 1;
            oFilterTeamDTO.PageSize = oFilterTeamDTO.PageSize ?? 25;

            if (oFilterTeamDTO.Name == null)
                oFilterTeamDTO.Name = string.Empty;
            if (oFilterTeamDTO.Code == null)
                oFilterTeamDTO.Code = string.Empty;

            var pagedResult = await _teamsService.GetAllTeamsPagedAsync(oFilterTeamDTO);
            var result = pagedResult?.Teams ?? new List<GetTeamDTO>();

            ViewBag.PaginationInfo = pagedResult;
            ViewBag.Filter = oFilterTeamDTO;

            TeamModel teamModel = new TeamModel();
            teamModel.TeamsList = result.ToList();
            teamModel.TotalPages = pagedResult?.TotalPages ?? 1;
            teamModel.PageNumber = pagedResult?.CurrentPage ?? teamModel.PageNumber;
            teamModel.PageSize = pagedResult?.PageSize ?? teamModel.PageSize;

            return PartialView("_Teams", teamModel);
        }

        [HttpPost]
        public async Task<IActionResult> getTeamByID([FromBody] int teamID)
        {
            var result = await _teamsService.GetTeamByIDAsync(teamID);
            return Ok(result);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.Teams.Edit)]
        public async Task<IActionResult> EditTeam(UpdateTeamDTO updatedTeam, string Color)
        {

            if (updatedTeam.Id != 0)
            {

                updatedTeam.Color = getColor(Color);

                await _teamsService.UpdateTeamAsync(updatedTeam);

            }
            else
            {
                var teamToAdd = new AddTeamDTO
                {
                    Code = updatedTeam.Code,
                    Short = updatedTeam.Short,
                    PrimaryName = updatedTeam.PrimaryName,
                    SecondaryName = updatedTeam.SecondaryName,
                    Color = getColor(Color),
                    Technicians = updatedTeam.Technicians

                };
                await _teamsService.AddTeamAsync(teamToAdd);

            }

            return RedirectToAction(INDEX_PAGE);
        }

        [HttpPost]
        [CustomAuthorize(Permissions.Teams.Delete)]
        public async Task<IActionResult> DeleteTeamAjax([FromBody] int teamId)
        {
            try
            {
                var result = await _teamsService.DeleteTeamAsync(teamId);
                if (result > 0)
                {
                    return Ok(new { success = true, result = result });
                }
                else
                {
                    return Ok(new { success = false, result = result, message = "Team could not be deleted. It may be in use." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [CustomAuthorize(Permissions.Teams.Delete)]
        public async Task<IActionResult> DeleteTeam(int teamId)
        {

            await _teamsService.DeleteTeamAsync(teamId);
            return RedirectToAction(INDEX_PAGE);
        }

        public IActionResult TeamsList()
        {
            return PartialView("_TeamsList");
        }

        public int getColor(string hexColor)
        {
            // Remove the '#' if present
            if (hexColor.StartsWith("#"))
            {
                hexColor = hexColor.Substring(1);
            }

            // Parse the hexadecimal string to an integer
            int argbValue = int.Parse(hexColor, NumberStyles.HexNumber);
            return argbValue;
        }

    }
}
