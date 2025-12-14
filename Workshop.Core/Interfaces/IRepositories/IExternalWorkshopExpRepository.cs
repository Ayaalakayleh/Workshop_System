using Workshop.Core.DTOs.ExternalWorkshopExp;
using DExternalWorkshopExpDTO = Workshop.Core.DTOs.ExternalWorkshopExp.DExternalWorkshopExpDTO;
using MExternalWorkshopExpDTO = Workshop.Core.DTOs.ExternalWorkshopExp.MExternalWorkshopExpDTO;

namespace Workshop.Core.Interfaces.IRepositories
{
    public interface IExternalWorkshopExpRepository
    {
        Task<IEnumerable<MExternalWorkshopExpDTO>> ExternalWorkshopExpGetAsync(ExternalWorkshopExpFilterDTO filter);
        Task<int> ExternalWorkshopExpInsertAsync(CreateExternalWorkshopExpDTO createDto);
        Task<IEnumerable<DExternalWorkshopExpDTO>> ExternalWorkshopExpGetDetailsByIdAsync(int headerId);
        Task<MExternalWorkshopExpDTO> ExternalWorkshopExpGetByIdAsync(int id);
        Task<int> ExternalWorkshopExpDetailsUpdateAsync(List<DExternalWorkshopExpDTO> prData);

        Task<IEnumerable<MExcelMappingDTO>> ExcelMappingGetAsync(ExcelMappingFilterDTO filter);
        Task<int> ExcelMappingInsertAsync(CreateExcelMappingDTO createDto);
        Task<int> ExcelMappingUpdateAsync(UpdateExcelMappingDTO updateDto);
        Task<IEnumerable<ExcelMappingColumnDTO>> ExcelMappingGetColumnsAsync();
        Task<IEnumerable<DExcelMappingDTO>> ExcelMappingGetDetailsByIdAsync(int? id, int? workshopId);
        Task<IEnumerable<ExternalWorkshopExpReportDTO>> ExternalWorkshopExpReportAsync(ExternalWorkshopExpReportFilterDTO filter);
    }
}