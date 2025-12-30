using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.WorkshopDTOs;
using Workshop.Core.DTOs.WorkshopMovement;

namespace Workshop.Core.DTOs.Vehicle
{
	public class VehicleDefinitions //: MainDirectoryPath
	{
		public int Id { get; set; }
		public string VehicleName { get; set; }
		public int ManufacturerId { get; set; }
		public int VehicleModelId { get; set; }
		public int ManufacturingYear { get; set; }
		public string PlateNumber { get; set; }
		public int FuleLevelId { get; set; }
		public int FuleLevel { get; set; }
		public string OwnerName { get; set; }
		public string path { get; set; }
		public string ImageName { get; set; }
		public int CompanyId { get; set; }
		public decimal CostCenter { get; set; }

		public int VehicleStatusId { get; set; }
		public string ChassisNo { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public int InsuranceCompanyId { get; set; }
		public int VehicleId { get; set; }
		public int Color { get; set; }
		public int? VehicleTypeId { get; set; }
		public decimal CurrentMeter { get; set; }
		public int? VehicleSubStatusId { get; set; }
		public int? AreaId { get; set; }
		public DateTime PurchasingDate { get; set; }
		public string SubModel { get; set; }

		public List<Manufacturers> ColManufacturers { get; set; }
		public List<VehicleModel> ColVehicleModels { get; set; }
		public List<CustomerInformation> ColInsuranceCompany { get; set; }
		public Manufacturers RefManufacturers { get; set; }
		public VehicleModel RefVehicleModels { get; set; }
		public VehicleStatus RefVehicleStatus { get; set; }
		public CustomerInformation InsuranceCompany { get; set; }
		public List<M_VehicleColor> ColVehicleColors { set; get; }
		public M_VehicleColor RefVehicleColor { set; get; }
		public VehicleClass RefVehicleClasses { set; get; }

		public List<VehicleSubStatus> ColVehicleSubStatus { get; set; }
		public int? ProjectId { get; set; }
		public int? Contract_CC_DimensionsId { get; set; }
		public int? Customer_DimensionsId { get; set; }
		public int? Department_DimensionsId { get; set; }
		public int? LOB_DimensionsId { get; set; }
		public int? Locations_DimensionsId { get; set; }
		public int? Regions_DimensionsId { get; set; }
		public int? FixedAsset_DimensionsId { get; set; }
		public int? Vendor_DimensionsId { get; set; }
		public int? Item_DimensionsId { get; set; }
		public int? Worker_DimensionsId { get; set; }
		public int? City_DimensionsId { get; set; }
		public int? D1_DimensionsId { get; set; }
		public int? D2_DimensionsId { get; set; }
		public int? D3_DimensionsId { get; set; }
		public int? D4_DimensionsId { get; set; }
		public int VehicleClassId { get; set; }
        public bool isRecall { get; set; }

    }

	public class CustomerInformationSummery
	{
		public Int64 Id { get; set; }
		public string CustomerPrimaryName { get; set; }
		public string CustomerSecondaryname { get; set; }
		public string CustomerName { get; set; }

	}

	public class Manufacturers
	{
		public int Id { get; set; }
		public string ManufacturerPrimaryName { get; set; }
		public string ManufacturerSecondaryName { get; set; }
		public string FactoryCountry { get; set; }
		public string Logo { get; set; }
		public List<Manufacturers> ColManufacturers { get; set; }
		public VehicleModel RefVehicleModel { get; set; }
		public List<VehicleModel> ColVehicleModels { get; set; }
		public VehicleSeries RefVehicleSeries { get; set; }
		public List<VehicleSeries> ColVehicleSeries { get; set; }
		public string Name { get; set; }
	}

	public class VehicleModel
	{
		public int Id { get; set; }
		public string VehicleModelPrimaryName { get; set; }
		public string VehicleModelSecondaryName { get; set; }
		public int ManufacturerId { get; set; }
		public int? VehicleSeriesId { get; set; }
		public string CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public string ModifyBy { get; set; }
		public DateTime ModifyAt { get; set; }
		public bool Status { get; set; }
		public VehicleSeries RefVehicleSeries { get; set; }
		public List<VehicleSeries> ColVehicleSeries { get; set; }
		public Manufacturers RefManufacturers { get; set; }
		public List<Manufacturers> ColManufacturers { get; set; }
		public string Name { get; set; }

	}

	public class VehicleStatus
	{
		public int Id { get; set; }
		public string VehicleStatusPrimaryName { get; set; }
		public string VehicleStatusSecondaryName { get; set; }
		public string Name { get; set; }

	}

	public class M_VehicleColor
	{
		public int Id { set; get; }
		public string VehiclePrimaryName { set; get; }
		public string VehicleSecondaryName { set; get; }
		public string VehicleColor { set; get; }
		public int CreatedBy { set; get; }
		public int ModifyBy { set; get; }
		public string Name { set; get; }
	}

	public class VehicleClass
	{
		public int Id { get; set; }
		public string VehicleClassPrimaryName { get; set; }
		public string VehicleClassSecondaryName { get; set; }
		public string Name { get; set; }
		public int PassengerNo { get; set; }
		public int SuitcasesNo { get; set; }

		public string Image { get; set; }

		public int ClassDegree { get; set; }

	}

	public class VehicleSubStatus
	{
		public int Id { get; set; }
		public string PrimaryName { get; set; }
		public string SecondaryName { get; set; }
		public int CompanyId { get; set; }
		public bool IsDeleted { set; get; }
		public DateTime? UpdatedDate { get; set; }
		public int UpdatedBy { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public string Name { get; set; }
	}

	public class VehicleSeries
	{
		public int Id { get; set; }
		public string VehicleSeriesPrimaryName { get; set; }
		public string VehicleSeriesSecondaryName { get; set; }
		public int SeriesClassId { get; set; }
		public string Name { get; set; }
		public List<VehicleSeriesClass> SeriesClass { get; set; }
	}

	public class VehicleSeriesClass
	{
		public int Id { get; set; }
		public string PrimaryName { get; set; }
		public string SecondaryName { get; set; }
		public string Name { get; set; }
	}

	public class CreateVehicleDefinitionsModel
	{
		public CreateVehicleDefinitionsModel()
		{

			ColVehicleColors = new List<M_VehicleColor>();
			ColVehicleModels = new List<VehicleModel>();
			ColManufacturers = new List<Manufacturers>();
			ColClasses = new List<VehicleClass>();

		}
		public int Id { get; set; }
		public int ManufacturerId { get; set; }
		public int VehicleModelId { get; set; }
		public int ManufacturingYear { get; set; }
		public string PlateNumber { get; set; }
		public int FuleLevel { get; set; }
		public int Color { get; set; }
		public decimal CurrentMeter { get; set; }
		public int VehicleStatusId { get; set; }
		public string ChassisNo { get; set; }
		public int CompanyId { get; set; }
		public int BranchId { get; set; }
		public string CreatedBy { get; set; }
		public string path { get; set; }
		//[JsonIgnore]
		//public HttpPostedFileBase Photos { get; set; }
		public List<M_VehicleColor> ColVehicleColors { set; get; }
		public List<VehicleModel> ColVehicleModels { get; set; }
		public List<Manufacturers> ColManufacturers { get; set; }
		public List<VehicleClass> ColClasses { get; set; }
		public int? VehicleClassId { get; set; }
	}

	public class VehicleNams
	{
		public int id { set; get; }
		public string VehicleName { set; get; }
		public int Total { get; set; }
		public decimal CurrentMeter { get; set; }
		public int VehicleStatusId { get; set; }
		public string VehicleSubStatus { set; get; }
		public string ManufacturingYear { set; get; }
		public string ManufacturerName { set; get; }
		public string ModelName { set; get; }
		public string PlateNumber { set; get; }
		public string ChassisNo { set; get; }
	}

	//Vehicle filter
	public class VehicleFilter
	{
		public int? ManufacturerId { get; set; }
		public string? PlateNumber { get; set; }
		public int Page { get; set; } = 1;
	}

	public class VehicleMovement
	{
		public int? Id { get; set; }//vid
		public int? MovementInId { get; set; }//vid
		public int? MoveOutWorkshopId { get; set; }//vid
		public int? MoveInWorkshopId { get; set; }//vid
		public int? MovementOutId { get; set; }//vid
		public int? WIP_Id { get; set; }
		public int? FromWhere { get; set; }
		public int? ReceivedBranchId { get; set; }
		public string? fuelLevel { get; set; }
		public int? FuelLevelId { get; set; }
		public string? ImagesFilePath { get; set; }
		public string? ImagesFilePathLastIn { get; set; }
		//[DisplayFormat(DataFormatString = "{dd-MM-yyyy}")]
		public DateTime? GregorianMovementDate { set; get; }
		public DateTime? hijriMovementDate { set; get; }
		public DateTime? LastMaintenanceDate { set; get; }
		public TimeSpan? ReceivedTime { get; set; }
		public decimal? ReceivedMeter { get; set; }
		public string? ReceivedDriverId { get; set; }
		public string? VehicleMovemens { set; get; }
		public int? RelatedItemId { get; set; }
		public int? fk_VehicleId { get; set; }
		public int? RecordNo { get; set; }
		public int? MovementId { get; set; }
		public string? MovementType { get; set; }
		public int? ExitBranchId { get; set; }
		public TimeSpan? ExitTime { get; set; }
		public int? VehicleID { get; set; }
		public int? FK_AgreementId { set; get; }
		public int? reservationId { set; get; }
		public int? WorkshopId { set; get; }
		public string? VehicleModelName { get; set; }
		public string? ManufactureName { get; set; }
		public string? PlateNumber { get; set; }
		public decimal? ExitMeter { get; set; }
		public decimal? TotalWorkOrder { get; set; }
		public decimal? PartsCost { get; set; }
		public decimal? LaborCost { get; set; }
		public decimal? ExitOilMeter { get; set; }
		public decimal? ReceivedOilMeter { get; set; }
		public int? MovementTypeId { get; set; }
		public int? MaintenanceStatus { get; set; }
		public string? ExitDriverId { get; set; }
		public string? ResivedDriverId { get; set; }
		public string? ExitBranch { get; set; }
		public string? ReceivedBranch { get; set; }
		public string? Note { get; set; }
		public int? CreatedBy { get; set; }
		public int? AccidentId { get; set; }
		public DateTime? CreatedAt { get; set; }
		public int? ModifyBy { get; set; }
		public DateTime? ModifyAt { get; set; }
		public bool? MovementOut { get; set; }
		public bool? IsMechanicCheck { get; set; }
		public bool? MovementIN { get; set; }
		public bool? IsRegularMaintenance { get; set; } = false;
		public Guid? MasterId { get; set; }
		public string? strikes { get; set; }
		public MaintenanceCardDTO? Card { get; set; }
		public List<FuleLevel>? fuelLevels { get; set; }
		public List<CompanyBranch>? ColBranches { get; set; }
		public List<ReservationType>? ColReservationType { get; set; }
		public List<WorkShopDefinitionDTO>? workshops { get; set; }
		public ReservationType? RefReservationType { get; set; }
		public VehicleDefinitions? RefVehicledefinitions { get; set; }
		public List<VehicleDefinitions>? ColVehicledefinitions { get; set; }
		public List<MaintenanceCardDTO>? ColMaintenanceCard { get; set; }
		public List<Item>? Items { get; set; }
		public List<Item>? Services { get; set; }
		public List<Category>? categories { get; set; }
		public List<VehicleMovement>? ColMovements { get; set; }
		public List<MWorkOrderDTO>? WorkOrders { get; set; }
		[DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}")]
		public bool? ishijriMovement { set; get; }
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime? GregorianMovementEndDate { set; get; }
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
		public DateTime? hijriMovementEndDate { set; get; }
		public bool? ishijriEndMovement { set; get; }
		public Agreement? agreement { set; get; }
		public string? Movement { set; get; }
		public int? result { set; get; }
		public int? TotalPages { get; set; }
		public int? page { set; get; }
		public string? VehicleName { set; get; }
		public List<VehicleNams>? vehicleNams { set; get; }
		public string? ImagesFilePathLastOut { get; set; }
		public List<VehicleMovementDocument>? VehicleMovementDocuments { get; set; }
		public List<VehicleMovementDocument>? LastVehicleMovementDocuments { get; set; }
		public int? CompanyId { get; set; }
		public bool? CanEdit { get; set; } = true;
		public Int64? TranNo { get; set; }
		public Int64? TranTypeNo { get; set; }
		public DateTime? TransDate { get; set; }
		public decimal? RevenueDuesAmount { get; set; }
		public int? BranchId { get; set; }
		public int? flag { get; set; } // 1 only in (can make work order or transfer), 
		public List<int>? ColAgreementId { get; set; }
		public decimal? FuelLevelValue { get; set; }
		public string? DriverSignature { get; set; }
		public string? EmployeeSignature { get; set; }
		public string? Reason { get; set; }
		public int? AgT { get; set; } // for transfer
		public DateTime? MinMovementDate { get; set; } // for revenue
		public int? AgreementPeriodTypeId { get; set; }
		public int? Status { get; set; }
		public string? InvoceNo { get; set; }
		public decimal? ConsumptionValueOfSpareParts { set; get; }
		public decimal? Vat { set; get; }
		public decimal? DeductibleAmount { set; get; }
		public int? LastVehicleStatus { set; get; }
		public int? VehicleSubStatusId { get; set; }
		public int? WorkOrderId { get; set; }
		public string? Complaint { get; set; }
		public string? ComplaintNote { get; set; }
		public bool? IsExternal { get; set; }
		public List<MovementInvoice>? MovementInvoice { get; set; }
		public int? Days { get; set; }
		public bool? AddService { get; set; }
		public VehicleMovement? LastMovementDetails { get; set; }
		public int? HasWorkorder { get; set; }
		public decimal? VatRate { get; set; }
		public decimal? TotalCost { get; set; }
		public List<TypeSalesPurchases>? InvoiceType { get; set; }
		public int? InvoiceTypeId { get; set; }
		public int? InvoiceId { get; set; }
		public bool NotTaxable { get; set; } = false;
		public List<int>? ServicesIds { get; set; }
		public List<CreateWIPServiceDTO>? WIPServices { get; set; }
		public bool isPart { get; set; } = false;
		public List<VehicleChecklist>? VehicleCkecklist { get; set; }
		public List<TyreChecklist>? TyreCkecklist { get; set; }
		public bool HasRecall { get; set; }
		public int? RecallId { get; set; }
        public List<VehicleNams>? ExternalVehicleNams { get; set; }
		public List<int>? Recalls { get; set; }
    }

	public class VehicleMovementDocument
	{
		public int Id { set; get; }

		public int MovementId { set; get; }

		public string FilePath { set; get; }

		public string FileName { set; get; }

		public int CreatedBy { set; get; }

		public int UpdatedBy { set; get; }

		public bool IsDeleted { set; get; }
	}

	public class VehicleAdvancedFilter
	{
		public int? ManufacturerId { get; set; }
		public int? VehicleModelId { get; set; }
		public int? VehicleStatusId { get; set; }
		public int? VehicleTypeId { get; set; }
		public string? PlateNumber { get; set; }
		public int? BranchId { get; set; }
		public int? CompanyId { get; set; }
		public int? VehicleId { get; set; }
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 50;
		public string ChassisNo { get; set; }

	}
	public class VehicleDocuments
	{
		public int FileType { get; set; }
		public int VehicleId { get; set; }
		public int FileId { get; set; }
		public string FilePath { get; set; }
		public string FileName { get; set; }
		public string FileContentType { get; set; }
		public List<DocumentType> doctypes { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedDate { get; set; }
		public int? UpdatedBy { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public bool IsDeleted { get; set; }
		public string Number { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public DateTime? HijriExpiryDate { get; set; }
		public List<string> Files { get; set; }
		public string strExpiryDate { get; set; }
		public string strHijriExpiryDate { get; set; }
		public string strStartDate { get; set; }
		public string strHijriStartDate { get; set; }
		public int FromWhere { get; set; }
		public bool HasExpiryDate { get; set; }
		public decimal ExpensesAmount { get; set; }
		//public int FileSystemId { get; set; }
		public int? ExpensesType { get; set; }
		public DateTime? TransDate { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? HijriStartDate { get; set; }
		public int CompanyId { get; set; }
		public int BranchId { get; set; }
		public int FileSystemId { get; set; }  // 1 GPP , 2 Estemara , 3 MOT , 4 Insurance with chassis , 5 insurace with plate 
											   //
		public Int64 TransNo { get; set; }
		public bool IsHijriStartDate { get; set; }
		public bool IsHijriExpiryDate { get; set; }
		public int Id { get; set; }

	}

	public class DocumentType
	{
		public DocumentType()
		{
			Accounts = new List<AccountTable>();
		}
		public int ID { get; set; }
		public string PrimaryName { get; set; }
		public string SecondaryName { get; set; }
		public int CreatedBy { get; set; }
		public DateTime CreatedAt { get; set; }
		public int? ModifyBy { get; set; }
		public DateTime? ModifyAt { get; set; }
		public bool IsRequired { get; set; }
		public int CustomerType { get; set; } // Individual , 2 company
		public int Type { get; set; } // 1 agreement , 2 vehicle , 3 customer , 4 chauffer,5 FrameContract
									  //public int DocSystemId { get; set; }
		public bool HasExpiryDate { get; set; }
		public int? ExpensesType { get; set; }
		public bool ProceedIfExpired { get; set; }
		public List<AccountTable> Accounts { get; set; }
		public bool CanEdit { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public string Name { get; set; }
		public DateTime? LastExpenseDate { get; set; }
		public string strLastExpenseDate { get; set; }
		public int FileSystemId { get; set; } // 1 GTP , 2 Estemara , 3 MOT , 4 Insurance with chassis , 5 insurace with plate ,6 MVPI
	}
	public class VehicleMovementStrike
	{
		public int? MovementId { get; set; }
		public string? Strike { get; set; }
	}

	public class VehicleMovementDTO
	{
		public int MovementId { get; set; }
		public DateTime GregorianMovementDate { get; set; }
		public int VehicleID { get; set; }

		public int? ExitBranchId { get; set; }
		public int ReceivedBranchId { get; set; }

		public decimal ReceivedMeter { get; set; }
		public decimal? ExitMeter { get; set; }

		public TimeSpan? ExitTime { get; set; }
		public TimeSpan? ReceivedTime { get; set; }

		public string? ExitDriverId { get; set; }
		public string? ResivedDriverId { get; set; }

		public bool MovementIN { get; set; }
		public bool MovementOut { get; set; }

		public int? WorkshopId { get; set; }
		public Guid? MasterId { get; set; }
		public bool? IsExternal { get; set; }

		public int? WorkOrderId { get; set; }
		public int? FuelLevelId { get; set; }
	}

	public class VehicleWorkOrdersSummery
	{
		public int Id { get; set; }
		public int WorkOrderType { get; set; }
		public int VehicleId { get; set; }
		public int FK_VehicleMovementId { set; get; }
		public int FK_AgreementId { set; get; }
		public DateTime GregorianDamageDate { set; get; }
		public bool IsFix { set; get; }
		public int WorkOrderNo { get; set; }
		public int WorkOrderStatus { get; set; } // 1== Open 2== Inprogress 3== under reper 4== closed
		public string Services { get; set; }
		public string WorkOrderTitle { get; set; }
		public int? BranchId { get; set; }
	}

    public class VehicleChecklistLookup
    {
        public int? Id { get; set; }
        public string? PrimaryDescription { get; set; }
        public string? SecondaryDescription { get; set; }
    }
    public class TyreChecklistLookup
    {
        public int? Id { get; set; }
        public string? PrimaryDescription { get; set; }
        public string? SecondaryDescription { get; set; }
    }
    public class VehicleChecklist
	{
		public int Id { get; set; }
		public int LookupId { get; set; }
        public string? LookupPrimaryDescription { get; set; }
        public string? LookupSecondaryDescription { get; set; }
        public int MovementId { get; set; }
		public bool Pass { get; set; }
		public string? Description { get; set; }
	}
    public class TyreChecklist
    {
        public int Id { get; set; }
        public int LookupId { get; set; }
        public string? LookupPrimaryDescription { get; set; }
        public string? LookupSecondaryDescription { get; set; }
        public int MovementId { get; set; }
        public string?  Brand { get; set; }
        public string? DOT { get; set; }
        public decimal? WearLevel { get; set; }
    }
}
