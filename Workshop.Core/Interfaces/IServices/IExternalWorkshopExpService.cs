using Workshop.Core.DTOs.ExternalWorkshopExp;
using DExternalWorkshopExpDTO = Workshop.Core.DTOs.ExternalWorkshopExp.DExternalWorkshopExpDTO;
using MExternalWorkshopExpDTO = Workshop.Core.DTOs.ExternalWorkshopExp.MExternalWorkshopExpDTO;

namespace Workshop.Core.Interfaces.IServices
{
    public interface IExternalWorkshopExpService
    {
        Task<IEnumerable<MExternalWorkshopExpDTO>> ExternalWorkshopExpGetAsync(ExternalWorkshopExpFilterDTO filter);
        Task<bool> InsertExternalWorkshopExpAsync(CreateExternalWorkshopExpDTO createDto);
        Task<IEnumerable<DExternalWorkshopExpDTO>> ExternalWorkshopExpGetDetailsByIdAsync(int headerId);
        Task<MExternalWorkshopExpDTO> ExternalWorkshopExpGetByIdAsync(int id);
        Task<bool> ExternalWorkshopExpDetailsUpdateAsync(List<DExternalWorkshopExpDTO> prData);

        Task<IEnumerable<MExcelMappingDTO>> ExcelMappingGetAsync(ExcelMappingFilterDTO filter);
        Task<bool> ExcelMappingInsertAsync(CreateExcelMappingDTO createDto);
        Task<bool> ExcelMappingUpdateAsync(UpdateExcelMappingDTO updateDto);
        Task<IEnumerable<ExcelMappingColumnDTO>> ExcelMappingGetColumnsAsync();
        Task<IEnumerable<DExcelMappingDTO>> ExcelMappingGetDetailsByIdAsync(int? id, int? workshopId);
        Task<IEnumerable<ExternalWorkshopExpReportDTO>> ExternalWorkshopExpReportAsync(ExternalWorkshopExpReportFilterDTO filter);
    }
}