using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{


   
    //public class AccountSalesDetails
    //{
    //    public string KeyId { get; set; }
    //    public Int64 ID { get; set; }
    //    public Int64 AccSalesNo { get; set; }
    //    public int AccSalesTypeNo { get; set; }
    //    public Int64? ItemNumber { get; set; }
    //    public int? UnitId { get; set; }
    //    public decimal? CostsCentersNo { get; set; }
    //    public string Description { get; set; }
    //    public string ClientTaxNo { get; set; }
    //    public string ClientName { get; set; }
    //    public string Reference { get; set; }
    //    public decimal Quantity { get; set; }
    //    public decimal UnitQuantity { get; set; }
    //    public decimal Price { get; set; }
    //    public decimal Total { get; set; }
    //    public decimal Discount { get; set; }
    //    public decimal DiscountPercentage { get; set; }
    //    public decimal? TaxClassificationId { get; set; }
    //    public decimal TaxValue { get; set; }
    //    public decimal ExtraCharge { get; set; }
    //    public decimal Final { get; set; }
    //    public Int64 AccSalesMasterNo { get; set; }
    //    public int? WarehouseId { get; set; }
    //    public int? Contract_CC_DimensionsId { get; set; }
    //    public int? Department_DimensionsId { get; set; }
    //    public int? LOB_DimensionsId { get; set; }
    //    public int? Locations_DimensionsId { get; set; }
    //    public int? Regions_DimensionsId { get; set; }
    //    public int? Customer_DimensionsId { get; set; }
    //    public int? Vendor_DimensionsId { get; set; }
    //    public int? Item_DimensionsId { get; set; }
    //    public int? FixedAsset_DimensionsId { get; set; }
    //    public int? Worker_DimensionsId { get; set; }
    //    public int? City_DimensionsId { get; set; }
    //    public int? D1_DimensionsId { get; set; }
    //    public int? D2_DimensionsId { get; set; }
    //    public int? D3_DimensionsId { get; set; }
    //    public int? D4_DimensionsId { get; set; }
    //    public string TaxExemptReason { get; set; }

    //}

    public class TransactionMaster
    {
        public Int64 ID { get; set; }
        public int TranTypeNo { get; set; }
        public int TranTypeNoReverse { get; set; }// For Reverse Trans { Customer , Vendor }
        public Int64 TranNo { get; set; }
        public DateTime TranDate { get; set; }
        public string DescriptionCode { get; set; }

        public string Notes { get; set; }
        public decimal Total { get; set; }
        public decimal EnteredCurrencyTotal { get; set; }
        public int CurrencyID { get; set; }
        public int Rate { get; set; }
        public int CreateBy { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public bool IsAutoCreated { get; set; }
        public int VoucherType { get; set; }
        public int? TransTypeNoSearch { get; set; }

        public string ImageName { get; set; } = "";

        public int IsCompanyCenterialized { get; set; }
        public List<TransactionDetails> oLTransactionDetails { get; set; }


    }
    public class TransactionDetails
    {
        public Int64 ID { get; set; }
        public Int64 MasterId { get; set; }
        public string KeyId { get; set; }
        public string AccountNo { get; set; }
        public decimal DAmount { get; set; }
        public decimal CAmount { get; set; }
        public decimal EnteredCurrencyDAmount { get; set; }
        public decimal EnteredCurrencyCAmount { get; set; }
        public string Notes { get; set; }
        public int? AccountId { get; set; }
        public int? CostCenterId { get; set; }
        public decimal? CostsCentersNo { get; set; }
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
        public string CheeksJson { get; set; }
        public string FinancialDimensionsJson { get; set; }
    }
}
