using System.Collections.Generic;
using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class ConsumptionReportViewModel
    {
        public ConsumptionReportFilterDTO Filter { get; set; }
        public List<ConsumptionReportDTO> ReportData { get; set; }
    }
}
