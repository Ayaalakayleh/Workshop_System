
namespace Workshop.Core.DTOs.General
{
    public class StateResponse
    {
        public StateResponse()
        {
            Actions = new List<WorkflowActions>();
            UsersContactInformation = new List<UserContactInformation>();
        }
        public int CurrentState { get; set; }
        public int? NextState { get; set; }
        public string PrimaryCurrentStateString { get; set; }
        public string SecondaryNextStateString { get; set; }
        public string SecondaryCurrentStateString { get; set; }
        public string PrimaryNextStateString { get; set; }
        public int? ActionId { get; set; } // 1 accepted , 2 reject , 3 review
        public List<WorkflowActions> Actions { get; set; }
        public int NextGroupId { get; set; } // Can edit to next group
        public bool IsFinished { get; set; }
        public bool IsRejected { get; set; }
        public int NotificationType { get; set; }//1=> SMS , 2 => Email , 3 => SMS and EMAIL
        public List<UserContactInformation> UsersContactInformation { get; set; }

    }

    public class WorkflowActions
    {
        public int Id { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public string Name { get; set; }
    }

    public class UserContactInformation
    {
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public int Id { get; set; }

    }

    public class FuleLevel
    {
        public int Id { get; set; }
        public string FuleLevelPrimaryName { get; set; }
        public string FuleLevelSecondaryName { get; set; }
        public decimal FuelLevelPercentage { get; set; }
    }

    public class CompanyBranch
    {
        public int BranchID { get; set; }
        public int BranchNumber { get; set; }
        public string BranchPrimaryName { get; set; }
        public string BranchSecondaryName { get; set; }
        public string SmallCurPrimaryName { get; set; }
        public string SmallCurSecondaryName { get; set; }
        public int CompanyId { get; set; }
        public int AreaId { get; set; }
        public int CityId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifyBy { get; set; }
        public DateTime ModifyAt { get; set; }
        public bool IsActive { get; set; }
        public string Host { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public bool Ssl { get; set; }
        public CompanyInfo oCompanyInfo { get; set; }
        public List<CompanyInfo> oLCompany { get; set; }
        public bool IsNumberingAutomatic { get; set; }
        public string CurrencyPrimaryName { get; set; }
        public string CurrencySecondlyName { get; set; }
        public string CurrencyCode { get; set; }
        public int CurrencyIDH { get; set; }
        public int CountryId { get; set; }
        public string TaxNumber { get; set; }
        public string RegistrationNo { get; set; }
        public string SMTPEmail { get; set; }
        public int CompanyCountryId { get; set; }
        public string Name { get; set; }
        public string ISOCode { get; set; }
        public string CountryCode { get; set; }
        public Byte[] Img { get; set; }

    }

    public class ReservationType
    {
        public int ReservationTypeId { get; set; }
        public string ReservationPrimaryName { get; set; }
        public string ReservationSecondaryName { get; set; }
        public string ReservationName { get; set; }


    }

    public class Category
    {
        public int CategoryId { get; set; }
        public int CategoryNumber { get; set; }
        public string CategoryPrimaryName { get; set; }
        public string CategorySecondaryName { get; set; }
        public string CategoryName { get; set; }
        public decimal CategoryExtraCharge { get; set; }
        public DateTime? CategoryExtraChargeDate { get; set; }
        //public TaxType CategoryTaxType { get; set; }
        public bool CategoryService { get; set; }
        public bool IsActive { get; set; }


        public int CompanyId { get; set; }

        public int BranchId { get; set; }

        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public int TaxClassificationNo { get; set; }
        public int GroupId { get; set; }

    }

    public class Agreement
    {
        public Int64 AgreementId { get; set; }
        public string CustomerName { set; get; }
        public int? LeaseCustomerId { get; set; }
        public Reservation refReservation { get; set; }
        public int? AgreementStatusId { get; set; }
        public int? VehicleDefinitionId { get; set; }
        public int CustomerId { get; set; }
        public DateTime GregorianReturnDate { get; set; }
    }

    public class Reservation
    {
        public int ReservationType { get; set; } // 0 limited period , 1 open
        public int? Id { get; set; }
        public TimeSpan PickupTime { get; set; }
        public TimeSpan ReturnTime { get; set; }
        public int? CustomerId { get; set; }
    }

    public class CompanyInfo
    {
        public int Id { get; set; }
        public int CompanyTaxNumber { get; set; }
        public int workshopId { get; set; }
        public string CompanySecondaryName { get; set; }
        public string CompanyPrimaryName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Fax { get; set; }
        public string FooterLine1 { get; set; }
        public string FooterLine2 { get; set; }
        public Byte[] Img { get; set; } // array of byte
        public string Image { get; set; } // url
        public string Host { get; set; }
        public string Email { get; set; }
        public string SMTPEmail { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public bool Ssl { get; set; }
        public int CashDeportation { get; set; }
        public int CountryId { get; set; }
        public string ISOCode { get; set; }
        public string CountryCode { get; set; }
        public int DefaultForOnline { get; set; }
        public int CurrencyIDH { get; set; }
        public string TaxNumber { get; set; }
        public int CompanyType { get; set; }
        public string SignatureImage { get; set; }
        public string PrimaryImage { get; set; }
        public string SecondaryImage { get; set; }
        public Byte[] SignatureImg { get; set; } // array of byte
        public Byte[] PrimaryImg { get; set; } // array of byte
        public Byte[] SecondaryImg { get; set; } // array of byte
        public string RegistrationNo { get; set; }
        public string PostalCode { get; set; }

        public bool IsNumberingAutomatic { get; set; }

        public string CurrencyPrimaryName { get; set; }

        public string CurrencySecondlyName { get; set; }

        public string CurrencyCode { get; set; }
        public string ShortAddress { get; set; }
        public string AddressAr { get; set; }
        public string CRNumber { get; set; }
    }
    public class UserPermissionGroup
    {
        public string Permissions { get; set; }
        public string Groups { get; set; }
    }
    public class Menu
    {
        public int Id { get; set; }
        public Int64 ParentId { get; set; }
        public string PrimaryName { get; set; }
        public string SecondarName { get; set; }
        public string Icon { get; set; }
        public string ControllerName { get; set; }
        public string MethodName { get; set; }
        public string Parameter { get; set; }
        public int Seq { get; set; }
        public int ModuleId { get; set; }
        public int UserId { get; set; }
        public List<Menu> MenuList { get; set; }
    }
    public class Modules
    {
        public int Id { get; set; }
        public string ModulePrimaryName { get; set; }
        public string ModuleSecondaryName { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int version { get; set; }
        public int createdBy { get; set; }
        public int ModifyBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifyAt { get; set; }
        public string HubName { get; set; }
    }


}
