using DocumentFormat.OpenXml.Bibliography;
using Workshop.Core.DTOs;
using Workshop.Core.DTOs.Vehicle;

namespace Workshop.Web.Models
{
    public class PartsSummaryReportModel
    {
        public IEnumerable<PartsSummaryDTO> data { get; set; }
        public string CompanyName { get; set; }
        public string Lang { get; set; }

    }
}
