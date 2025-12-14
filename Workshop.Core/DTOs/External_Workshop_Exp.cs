using System.Data;
using Workshop.Core.DTOs.AccountingDTOs;

namespace Workshop.Core.DTOs.ExternalWorkshopExp
{
    public class DExternalWorkshopExpDTO
    {
        public int ID { get; set; }
        public int? HeaderID { get; set; }
        public int? VehicleId { get; set; }
        public string? Invoice_No { get; set; }
        public string? License_Plate_No { get; set; }
        public DateTime Invoice_Date { get; set; }
        public string? Business_Line { get; set; }
        public int? MILAGE { get; set; }
        public string? City { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string? Description { get; set; }
        public string? Maker { get; set; }
        public string? Vin_No { get; set; }
        public string? Model { get; set; }
        public string? Year { get; set; }
        public decimal? SubTotal_BeforVat { get; set; }
        public decimal? Vat { get; set; }
        public decimal? Total { get; set; }
        public string? Service_Type { get; set; }
        public int WorkOrderId { get; set; }

    }

    public class MExternalWorkshopExpDTO
    {
        public int ID { get; set; }
        public int? ExternalWorkshopId { get; set; }
        public DateTime Excel_Date { get; set; }
        public int? Type { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int TotalPages { get; set; }
        public string VehicleName { set; get; }
        public string ExternalWorkshopName { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        //public List<SelectListItem> ExternalWorkshopList { get; set; }
        public string External_Workshop_ExpjsonList { get; set; }
        public List<DExternalWorkshopExpDTO> DExternalWorkshopExp { get; set; }
        public List<TypeSalesPurchases> InvoiceType { get; set; }
        public int InvoiceTypeId { get; set; }
    }

    public class ExternalWorkshopExpFilterDTO
    {
        public DateTime? ToDate { set; get; }

        public DateTime? FromDate { set; get; }
        public int? ExternalWorkshopId { get; set; }
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 25;
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public int VehicleId { get; set; }
    }

    public class ExternalWorkshopExpReportDTO
    {
        public string Invoice_No { get; set; }
        public string License_Plate_No { get; set; }
        public DateTime Invoice_Date { get; set; }
        public string Business_Line { get; set; }
        public int MILAGE { get; set; }
        public string Service_Type { get; set; }
        public int WorkshopId { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int VehicleId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DeductibleAmount { get; set; }
        public decimal ConsumptionValueOfSpareParts { get; set; }
        public decimal Vat { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public string VehicleName { get; set; }
    }

    public class ExternalWorkshopExpReportFilterDTO
    {
        public int? ExternalWorkshopId { get; set; }
        public int? VehicleId { get; set; }
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    //New Dtos

    // DTO for External Workshop Expense creation
    public class CreateExternalWorkshopExpDTO
    {
        public int? ExternalWorkshopId { get; set; }
        public DateTime? Excel_Date { get; set; }
        public int? Type { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int? CreatedBy { get; set; }
        public List<DExternalWorkshopExpDTO>? DExternalWorkshopExp { get; set; }
        public DataTable? PRData { get; set; }

        public void MappMExternalWorkshopExpDTOToCreateExternalWorkshopExpDTO(MExternalWorkshopExpDTO mExternalWorkshopExpDTO)
        {
            this.ExternalWorkshopId = mExternalWorkshopExpDTO.ExternalWorkshopId;
            this.Excel_Date = mExternalWorkshopExpDTO.Excel_Date;
            this.Type = mExternalWorkshopExpDTO.Type;
            this.CompanyId = mExternalWorkshopExpDTO.CompanyId;
            this.BranchId = mExternalWorkshopExpDTO.BranchId;
            this.CreatedBy = mExternalWorkshopExpDTO.CreatedBy;
            this.DExternalWorkshopExp = mExternalWorkshopExpDTO.DExternalWorkshopExp;
        }
    }

    // DTO for Excel Mapping operations
    public class ExcelMappingFilterDTO
    {
        public int CompanyId { get; set; }
        public int? BranchId { get; set; }
        public int? Page { get; set; }
        public int? Id { get; set; }
        public int? WorkshopId { get; set; }
    }

    public class CreateExcelMappingDTO
    {
        public int WorkshopId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int Started_Column { get; set; }
        public int Started_Row { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public List<DExcelMappingDTO> DExcelMappingList { get; set; }

        public void MapToCreateExcelMappingDTO(MExcelMappingDTO mExcelMappingDTO)
        {

            this.WorkshopId = mExcelMappingDTO.WorkshopId;
            this.FilePath = mExcelMappingDTO.FilePath;
            this.FileName = mExcelMappingDTO.FileName;
            this.Started_Column = mExcelMappingDTO.Started_Column;
            this.Started_Row = mExcelMappingDTO.Started_Row;
            this.CompanyId = mExcelMappingDTO.CompanyId;
            this.BranchId = mExcelMappingDTO.BranchId;
            this.CreatedBy = mExcelMappingDTO.CreatedBy;
            this.DExcelMappingList = mExcelMappingDTO.DExcelMappingList;
        }
    }

    public class UpdateExcelMappingDTO
    {
        public int Id { get; set; }
        public int WorkshopId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int Started_Column { get; set; }
        public int Started_Row { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int UpdatedBy { get; set; }

        public List<DExcelMappingDTO> DExcelMappingList { get; set; }

        public void MapToUpdateExcelMappingDTO(MExcelMappingDTO mExcelMappingDTO)
        {
            this.Id = (int)mExcelMappingDTO.Id;
            this.WorkshopId = mExcelMappingDTO.WorkshopId;
            this.FilePath = mExcelMappingDTO.FilePath;
            this.FileName = mExcelMappingDTO.FileName;
            this.Started_Column = mExcelMappingDTO.Started_Column;
            this.Started_Row = mExcelMappingDTO.Started_Row;
            this.CompanyId = mExcelMappingDTO.CompanyId;
            this.BranchId = mExcelMappingDTO.BranchId;
            this.UpdatedBy = (int)mExcelMappingDTO.UpdatedBy;
            this.DExcelMappingList = mExcelMappingDTO.DExcelMappingList;
        }
    }

    public class ExcelMappingColumnDTO
    {
        public string COLUMN_NAME { get; set; }
        public string DATA_TYPE { get; set; }
    }

    public class MExcelMappingDTO
    {
        public int? Id { get; set; }
        public int WorkshopId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int Started_Column { get; set; }
        public int Started_Row { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string jsonList { get; set; }
        public int? page { get; set; }
        public int TotalPages { get; set; }

        public List<DExcelMappingDTO> DExcelMappingList { get; set; }
        public List<ExcelMappingColumnDTO> ExcelMappingColumnsList { get; set; }

    }

    public class DExcelMappingDTO
    {
        public int Id { get; set; }
        public int HeaderId { get; set; }
        public string Column_Name { get; set; }
        public string Addition_Cullman_Name { get; set; }
        public int? Mapping_Column_Index { get; set; }
        public string Mapping_ColumnDB { get; set; }
        public bool Is_Vat_Included { get; set; }
        public string Addition_Data { get; set; }

    }
}
