using Microsoft.AspNetCore.Mvc;
using Workshop.Core.DTOs;
using Workshop.Core.Interfaces.IServices;
using ClosedXML.Excel;

namespace Workshop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : Controller
    {
        private readonly IReportsService _service;

        public ReportsController(IReportsService service)
        {
            _service = service;
        }

        [HttpPost("GetMonthlyRepairCost")]
        public async Task<IActionResult> GetMonthlyRepairCostReport([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            var result = await _service.GetMonthlyRepairCostReport(filter);
            return Ok(result);
        }
        [HttpPost("GetMonthlyRepairCostBranchWise")]
        public async Task<IActionResult> GetMonthlyRepairCostBranchWiseReport([FromBody] MonthlyRepairCostReportFilterDTO filter)
        {
            var result = await _service.GetMonthlyRepairCostBranchWiseReport(filter);
            return Ok(result);
        }
        [HttpPost("GetConsumption")]
        public async Task<IActionResult> GetConsumptionReport([FromBody] ConsumptionReportFilterDTO filter)
        {
            var result = await _service.GetConsumptionReport(filter);
            return Ok(result);
        }

    }
}
