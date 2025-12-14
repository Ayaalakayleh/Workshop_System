namespace Workshop.Core.DTOs.AccountingDTOs
{
    public class AccountTable
    {
        public int ID { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public bool AcceptTrans { get; set; }
        public bool Debit { get; set; }
        public bool Credit { get; set; }
        public bool ProfitandLoss { get; set; }
        public bool Budget { get; set; }
        public decimal StandardValue { get; set; }
        public bool AcceptCostCenter { get; set; }
        public Int64 ParentId { get; set; }
        public string ParentAccountName { get; set; }
        public string AccountSecondaryName { get; set; }

        public decimal DAmount { get; set; }
        public decimal CAmount { get; set; }
        public int AccountOrder { get; set; }
        public int AccountLevel { get; set; }

        public string AccountNameNo { get; set; }
        public string AccountSecondaryNameNo { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public bool IsContract_CC_Dimensions { get; set; }
        public bool IsDepartment_Dimensions { get; set; }
        public bool IsLOB_Dimensions { get; set; }
        public bool IsLocations_Dimensions { get; set; }
        public bool IsRegions_Dimensions { get; set; }
        public bool IsCustomer_Dimensions { get; set; }
        public bool IsVendor_Dimensions { get; set; }
        public bool IsItem_Dimensions { get; set; }
        public bool IsFixedAsset_Dimensions { get; set; }
        public bool IsWorker_Dimensions { get; set; }
        public bool IsCity_Dimensions { get; set; }
        public bool IsD1_Dimensions { get; set; }
        public bool IsD2_Dimensions { get; set; }
        public bool IsD3_Dimensions { get; set; }
        public bool IsD4_Dimensions { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public bool AllowingEntryWithoutFinancialDimensions { get; set; }
    }

    public class TaxClassification
    {
        public int TaxClassificationNo { get; set; }
        public string TaxClassificationName { get; set; }
        public string TaxClassificationArabicName { get; set; }
        public decimal TaxRate { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public string Name { get; set; }

    }
    
    public class CustomerInformation
    {
        public Int64 Id { get; set; }
        public int? CompanyId { get; set; }
        public int BranchIDH { get; set; }
        public int oLDBPaymentType { get; set; }
        public int? Type { get; set; }
        public Int64 ClientbranchId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPrimaryName { get; set; }
        public string CustomerSecondaryname { get; set; }
        public bool BelongsToCompany { get; set; }
        public DateTime Gregorianbirthdate { get; set; }
        public Int64 NationalityId { get; set; }
        public string Job { get; set; }
        public string HomeAdress { get; set; }
        public string workAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string BusinessEmail { get; set; }
        public string BusinessPhone { get; set; }
        public string Notes { get; set; }
        public string TaxNumber { get; set; }
        public bool BlackList { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifyBy { get; set; }
        public DateTime? ModifyAt { get; set; }
        public bool Status { get; set; }
        public string AccountNoReceivable { get; set; }
        public Int64 AccountNoReceivableId { get; set; }
        public Int64 AccountNoPayableId { get; set; }

        public Int64 AccountNo { get; set; }
        public string AccountNoPayable { get; set; }
        public int SalesPaymentTerms { get; set; }
        public int SalesPurchaseTerms { get; set; }
        public string PhotoPath { get; set; }
        public string PhotoPathOld { get; set; }
        public int TotalPages { get; set; }
        public int? FromWhere { get; set; }
        public double PaymenEvaluation { get; set; }
        public int PaymenEvaluationPerCondition { get; set; }
        public int WorkNature { get; set; }
        public int StopServiceId { get; set; }
        public decimal CreditLimit { get; set; }
        public string FilePath { set; get; }
        public List<string> FilesPaths { get; set; }
        public int CustomerTypeId { get; set; }
        public string IdCopyNumber { set; get; }
        public string IdPlaceOfIssue { set; get; }
        public DateTime? IdIssueDate { set; get; }
        public string CustomerRepresentativePrimaryName { set; get; }
        public string CustomerRepresentativeCapacityPrimaryName { set; get; }
        public string CustomerRepresentativeSecondaryname { set; get; }
        public string CustomerRepresentativeCapacitySecondaryname { set; get; }
        public string CustomerRepresentativeIdNumber { set; get; }
        public string RegistrationNo { set; get; }
        public string RegistrationHoldingNo { set; get; }
        public string CommercialRegistration { set; get; }

        public bool IsSettlement { set; get; }
        public string TermsAndConditions { set; get; }
        public string StreetName { get; set; }
        public string PostalCode { get; set; }
        public string DistrictName { get; set; }
        public string IdentityType { get; set; }
        public string BuildingNumber { get; set; }
        public string Alpha2 { get; set; }
        public int? CityId { get; set; }
        public int? AreaId { get; set; }
        public string CityName { get; set; }
        public string RegionName { get; set; }
        public int GroupAccountsId { get; set; }
        public int? ClassificationGroupId { get; set; } // 1. Collection  2. H.O   3.  Legal  4. Operations
        public int CurrencyId { get; set; }
        public int SalesTaxGroupId { get; set; }

        public int TypeOfCustomer { get; set; } // 1. Person, 2. Organization
        public string Language { get; set; } // 1. Ar, 2. en-us
        public string DebtorNumber { get; set; } // 1. Ar, 2. en-us
        public int TypeOfPaymentId { get; set; } // 1.Cash, 2.Checks, 3.Transfer


        public int? Contract_CC_DimensionsId { get; set; }
        public int? Department_DimensionsId { get; set; }
        public int? LOB_DimensionsId { get; set; }
        public int? Locations_DimensionsId { get; set; }
        public int? Regions_DimensionsId { get; set; }
        public int? Customer_DimensionsId { get; set; }
        public int? Vendor_DimensionsId { get; set; }
        public int? Item_DimensionsId { get; set; }
        public int? FixedAsset_DimensionsId { get; set; }
        public int? Worker_DimensionsId { get; set; }
        public int? City_DimensionsId { get; set; }
        public int? D1_DimensionsId { get; set; }
        public int? D2_DimensionsId { get; set; }
        public int? D3_DimensionsId { get; set; }
        public int? D4_DimensionsId { get; set; }
        public bool ShowFinancialDimensions { get; set; }
        public int? SupplierClassificationId { get; set; }
        public bool IsIntegrationDynamic365 { get; set; }
        public bool IsIntegratedDynamic365 { get; set; }
        public string GroupPrimaryName { get; set; }
        public string GroupSecondaryName { get; set; }
        public string LicenseNumber { get; set; }
        public decimal DiscountPercentagePart { get; set; }
        public decimal DiscountPercentageLabor { get; set; }
    }

    public class TypeSalesPurchases
    {
        public int InvoiceType { get; set; } // 1 Seles 2. Procurment
        public int Id { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int AccountId { get; set; }
        public bool IsFiexdAsset { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public string Channel { get; set; }
    }

    public class Item
    {
        public Int64 ItemId { get; set; }
        public int ItemNumber { get; set; }
        public string ItemBarcode { get; set; }
        public string ItemPrimaryName { get; set; }
        public string ItemSecondaryName { get; set; }
        public int CategoryId { get; set; }
        public string ItemPurchaseAccount { get; set; }
        public string ItemSalesAccount { get; set; }
        public string ItemName { get; set; }
        public string CategoryName { get; set; }
        public int? ItemPurchaseAccountId { get; set; }
        public int? ItemSalesAccountId { get; set; }
        public bool IsActive { get; set; }
        public decimal taxRate { get; set; }
        public decimal Price { get; set; }
        public bool Service { get; set; }
        public decimal PurchasingPrice { get; set; }
        public decimal ExtraCharge { get; set; }
        public string CategoryPrimaryName { get; set; }
        public string CategorySecondaryName { get; set; }

        public int TaxClassificationNo { get; set; }
        public int UnitId { get; set; }
        public string Image { get; set; }

        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int CreatedBy { get; set; }

        public int ModifiedBy { get; set; }
        public string ItemUnitToSave { get; set; }
        public decimal MinQuantity { get; set; }
        public string BarcodeSymbolog { get; set; }
        public bool IsMaterial { get; set; }
        public int CriticalLimit { get; set; }
        public int ServiceTime { get; set; }

    }

    public class AccountSales
    {
        public AccountSalesMaster AccountSalesMaster { get; set; }
        public List<AccountSalesDetails> AccountSalesDetails { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int CompanyType { get; set; }
        public int AreaId { get; set; }
        public int CityId { get; set; }
    }

    public class AccountSalesMaster
    {
        public Int64 ID { get; set; }
        public Int64 AccSalesNo { get; set; }
        public Int64 MasterId { get; set; }

        public int AccSalesTypeNo { get; set; }
        public int AccSalesBranch { get; set; }
        public DateTime AccSalesDate { get; set; }
        public string Notes { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int InventoryAccountId { get; set; }
        public string PoNo { get; set; }
        public decimal Net { get; set; }
        public decimal Tax { get; set; }
        public decimal Final { get; set; }
        public string Link { get; set; }
        public Int64? Reference { get; set; }

        public int CurrencyID { get; set; }
        public decimal TotalExtraCharge { get; set; }
        public DateTime SupplyDate { get; set; }
        public int InvoiceTypeNo { get; set; }
        public int InvoiceType { get; set; }
        public string UserId { get; set; }
        public int PaymentTerms { get; set; }
        public bool Paid { get; set; }
        public int TypeSalesPurchasesID { get; set; }
        public bool IsFiexdAsset { get; set; }

        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime DueDate { get; set; }

        public string InvoiceToSave { get; set; }
        public DateTime ReferenceInvoiceDate { get; set; }
        public int ZatcaOrder { get; set; }
        public string InvoiceHash { get; set; }
        public bool ReportedToZatca { get; set; }
        public string ReportingResult { get; set; }
        public string ReportingStatus { get; set; }
        public string QrCode { get; set; }
        public string SignedXml { get; set; }
        public DateTime ZatcaSubmissionDate { get; set; }
        public int ZatcaInvoiceType { get; set; }
        public int ZatcaInvoiceTypeCode { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public bool? IsSpot { get; set; }
        public int CustomerId { get; set; }
        public int TotalReportedInvoice { get; set; }
        public int Fk_AgreementId { get; set; }
        public string ReservationNo { get; set; }
        public string SupplierInvoiceNo { get; set; }
        public int? Contract_CC_DimensionsId { get; set; }
        public int? Department_DimensionsId { get; set; }
        public int? LOB_DimensionsId { get; set; }
        public int? Locations_DimensionsId { get; set; }
        public int? Regions_DimensionsId { get; set; }
        public int? Customer_DimensionsId { get; set; }
        public int? Vendor_DimensionsId { get; set; }
        public int? Item_DimensionsId { get; set; }
        public int? FixedAsset_DimensionsId { get; set; }
        public int? Worker_DimensionsId { get; set; }
        public int? City_DimensionsId { get; set; }
        public int? D1_DimensionsId { get; set; }
        public int? D2_DimensionsId { get; set; }
        public int? D3_DimensionsId { get; set; }
        public int? D4_DimensionsId { get; set; }
        public bool IsIntegrationDynamic365 { get; set; }
        public bool IsIntegratedDynamic365 { get; set; }
        public bool ShowFinancialDimensions { get; set; }
        public int SupplierId { get; set; }
        public string CustomerAccountNo { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class AccountSalesDetails
    {
        public string KeyId { get; set; }
        public Int64 ID { get; set; }
        public Int64 AccSalesNo { get; set; }
        public int AccSalesTypeNo { get; set; }
        public Int64? ItemNumber { get; set; }
        public int? UnitId { get; set; }
        public decimal? CostsCentersNo { get; set; }
        public string Description { get; set; }
        public string ClientTaxNo { get; set; }
        public string ClientName { get; set; }
        public string Reference { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        public decimal Discount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal? TaxClassificationId { get; set; }
        public decimal TaxValue { get; set; }
        public decimal ExtraCharge { get; set; }
        public decimal Final { get; set; }
        public Int64 AccSalesMasterNo { get; set; }
        public int? WarehouseId { get; set; }
        public int? Contract_CC_DimensionsId { get; set; }
        public int? Department_DimensionsId { get; set; }
        public int? LOB_DimensionsId { get; set; }
        public int? Locations_DimensionsId { get; set; }
        public int? Regions_DimensionsId { get; set; }
        public int? Customer_DimensionsId { get; set; }
        public int? Vendor_DimensionsId { get; set; }
        public int? Item_DimensionsId { get; set; }
        public int? FixedAsset_DimensionsId { get; set; }
        public int? Worker_DimensionsId { get; set; }
        public int? City_DimensionsId { get; set; }
        public int? D1_DimensionsId { get; set; }
        public int? D2_DimensionsId { get; set; }
        public int? D3_DimensionsId { get; set; }
        public int? D4_DimensionsId { get; set; }
        public string TaxExemptReason { get; set; }
    }
    public class TransTypeTable
    {
        public Int64 ID { get; set; }
        public string TransCode { get; set; }

        public string TransType { get; set; }
        public string TransTypeArabic { get; set; }
        public bool YearFlage { get; set; }
        public bool MonthFlage { get; set; }
        public int VoucherType { get; set; }
        public string VoucherTypeName { get; set; }
        public bool IsAutoCreated { get; set; }
        public bool IsAutoPosted { get; set; }
        public bool IsAdvance { get; set; }

        public int IsAbleToChange { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }


        public string CreatedOn { get; set; }
        public string ModifiedOn { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }

    }

    public class ValidSupplierInvoicesNo
    {
        public Int64 SupplierId { get; set; }
        public string Invoices { get; set; }


    }
}
