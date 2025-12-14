using Workshop.Core.DTOs;

namespace Workshop.Web.Models
{
    public class ExcelImportModel
    {

        public List<VehicleRecallDTO> ImportedRows { get; set; } = new List <VehicleRecallDTO>();
        public List<VehicleRecallDTO> RejectedRows { get; set; } = new List<VehicleRecallDTO>();

    }
}
