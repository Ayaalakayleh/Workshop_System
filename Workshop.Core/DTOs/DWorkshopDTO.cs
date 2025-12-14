
namespace Workshop.Core.DTOs.WorkshopDTOs
{
    public class WorkShopCoreDTO
    {
        public string? PrimaryName { get; set; }
        public string? SecondaryName { get; set; }
        public int? UserId { get; set; }
        public bool IsActive { get; set; }
        public int? ParentId { get; set; } = 0;
    }

    public class WorkShopContactDTO : WorkShopCoreDTO
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? GoogleURL { get; set; }
        public int? CityId { get; set; }
    }

    public class WorkShopFullDetailsDTO : WorkShopContactDTO
    {

        public string? PrimaryAddress { get; set; }
        public string? SecondaryAddress { get; set; }
        public bool? IsExternalWorkshop { get; set; }
        public int? VatClassificationId { get; set; }
        public int? AccountId { get; set; }
        public int? SupplierId { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string? InsuranceCompany { get; set; }
        public int[] InsuranceCompanyIds { get; set; } = [];

        public void SetInsuranceCompanyIds()
        {
            this.InsuranceCompanyIds = InsuranceCompany != null
                ? InsuranceCompany.Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id != 0)
                    .ToArray()
                : [];
        }
    }

    public class WorkShopDefinitionDTO : WorkShopFullDetailsDTO
    {
        public string? Name { get; set; }
        public int? Id { get; set; }
        public int UpdatedBy { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateWorkShopDTO : WorkShopFullDetailsDTO
    {
        public int CreatedBy { get; set; }

        public void MapToCreateDto(WorkShopDefinitionDTO definitionDto)
        {
            // Core properties (from WorkShopCoreDTO)
            this.PrimaryName = definitionDto.PrimaryName;
            this.SecondaryName = definitionDto.SecondaryName;
            this.UserId = definitionDto.UserId;
            this.IsActive = definitionDto.IsActive;
            this.ParentId = definitionDto.ParentId;

            // Contact properties (from WorkShopContactDTO)
            this.Email = definitionDto.Email;
            this.Phone = definitionDto.Phone;
            this.GoogleURL = definitionDto.GoogleURL;
            this.CityId = definitionDto.CityId;

            // Full details properties (from WorkShopFullDetailsDTO)
            this.PrimaryAddress = definitionDto.PrimaryAddress;
            this.SecondaryAddress = definitionDto.SecondaryAddress;
            this.IsExternalWorkshop = definitionDto.IsExternalWorkshop;
            this.VatClassificationId = definitionDto.VatClassificationId;
            this.AccountId = definitionDto.AccountId;
            this.SupplierId = definitionDto.SupplierId;
            this.CompanyId = definitionDto.CompanyId;
            this.BranchId = definitionDto.BranchId;

            // Convert InsuranceCompanyIds array to comma-separated string
            this.InsuranceCompany = definitionDto.InsuranceCompanyIds != null && definitionDto.InsuranceCompanyIds.Length > 0
                ? string.Join(",", definitionDto.InsuranceCompanyIds)
                : null;

            // Create-specific property
            this.CreatedBy = definitionDto.CreatedBy;
        }

    }

    public class UpdateWorkShopDTO : WorkShopDefinitionDTO
    {

        public void MapDefinitionToUpdateDto(WorkShopDefinitionDTO definitionDto)
        {

            // Core properties
            Id = definitionDto.Id;
            PrimaryName = definitionDto.PrimaryName;
            SecondaryName = definitionDto.SecondaryName;
            UserId = definitionDto.UserId;
            IsActive = definitionDto.IsActive;
            ParentId = definitionDto.ParentId;

            // Contact properties
            Email = definitionDto.Email;
            Phone = definitionDto.Phone;
            GoogleURL = definitionDto.GoogleURL;
            CityId = definitionDto.CityId;

            // Full details properties
            PrimaryAddress = definitionDto.PrimaryAddress;
            SecondaryAddress = definitionDto.SecondaryAddress;
            IsExternalWorkshop = definitionDto.IsExternalWorkshop;
            VatClassificationId = definitionDto.VatClassificationId;
            AccountId = definitionDto.AccountId;
            SupplierId = definitionDto.SupplierId;
            CompanyId = definitionDto.CompanyId;
            BranchId = definitionDto.BranchId;
            InsuranceCompany = definitionDto.InsuranceCompany;
            InsuranceCompanyIds = InsuranceCompany != null
                ? InsuranceCompany.Split(',')
                    .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                    .Where(id => id != 0)
                    .ToArray()
                : [];

            // Definition properties
            UpdatedBy = definitionDto.UpdatedBy;
            CreatedBy = definitionDto.CreatedBy;
            CreatedDate = definitionDto.CreatedDate;
        }
    }

    public class DeleteWorkShopDTO
    {
        public int Id { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class WorkshopListDTO : WorkShopContactDTO
    {
        public int Id { get; set; }
        public string? ParentName { get; set; } = null;
        public string? ParentPrimaryName { get; set; } = null;
        public string? ParentSecondaryName { get; set; } = null;
        public int? Pages { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    public class WorkShopFilterDTO
    {
        // Basic Filters
        public int? Id { get; set; }
        public int? ParentId { get; set; }
        public string? PrimaryName { get; set; }
        public string? SecondaryName { get; set; }

        // Location Filters
        public int? CityId { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? PrimaryAddress { get; set; }
        public string? SecondaryAddress { get; set; }
        public int CompanyId { get; set; }

        // Pagination and Utility
        public int Page { get; set; } = 1;
        public int RowsOfPage { get; set; } = 25;

        // Date Range Filters
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        //Language Preference
        public string? Language { get; set; } = "en";
    }

    public class ParentWorkshopSimpleDTO
    {
        public string? PrimaryName { get; set; }
        public string? SecondaryName { get; set; }
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? ParentId { get; set; }
    }

}
