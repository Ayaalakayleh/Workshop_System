using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class InventoryTransactionDetailsDTO
    {
        public int? FK_ItemId { get; set; }         // Item Id
        public int? FK_UnitId { get; set; }        // Unit Id
        public string? KeyId { get; set; }         // DevExpress keyId
        public decimal? Quantity { get; set; }     // NULL
        public decimal? UnitQuantity { get; set; } // UnitQuantity
        public decimal? Price { get; set; }    // Price
        public decimal? Total { get; set; }    // Price * UnitQuantity
        public int? FK_LocatorId { get; set; }    // Locator ID
        public string? Description { get; set; } // Details Note
        public string? Serial { get; set; }    // NULL
        public string? Batch { get; set; }     // NULL
        public DateTime? ExpiryDate { get; set; } // NULL
    }

    public class CreateInventoryTransactionDTO
    {
        public DateTime TransactionDate { get; set; }
        public int? TransactionReferenceNo { get; set; }
        public int? RequestId { set; get; }
        public int? FK_TransactionReferenceTypeId { get; set; }
        public int? FK_WarehouseId { get; set; }
        public int FK_TransactionTypeId { get; set; } // Type of this Movement AS: In the TransactionType Enum 
        public int FK_TransactionStatusId { get; set; }
        public IFormFile? AttachmentFile { get; set; }
        public string? AttachmentPath { get; set; }
        public string? Description { get; set; }
        public int? CompanyId { get; set; }
        public int? BranchId { get; set; }
        public int? FK_FromWarehouseId { get; set; }
        public int? FK_ToWarehouseId { get; set; }
        public long? Fk_FinancialTransactionMasterId { get; set; }
        public long? FinancialTransactionNo { get; set; }
        public int? FinancialTransactionTypeNo { get; set; }
        public int? Fk_InvoiceType { get; set; }
        public int? FK_ReasonCodeId { get; set; }
        public int? CreatedBy { get; set; }
        public int? StockType { get; set; }
        public int WhrHouse { set; get; } // select wharehouse only
        public List<InventoryTransactionDetailsDTO> Details { get; set; } = new();
    }

    public class InventoryTransactionByIdDTO
    {//7-10
        public long ID { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionReferenceNo { get; set; }
        public int FK_TransactionReferenceTypeId { get; set; }
        public int FK_WarehouseId { get; set; }
        public int FK_TransactionTypeId { get; set; }
        public int FK_TransactionStatusId { get; set; }
        public string AttachmentPath { get; set; }
        public string Description { get; set; }
        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int? FK_FromWarehouseId { get; set; }
        public int? FK_ToWarehouseId { get; set; }
        public int? Fk_FinancialTransactionMasterId { get; set; }
        public string FinancialTransactionNo { get; set; }
        public string FinancialTransactionTypeNo { get; set; }
        public string? ItemNumber { get; set; }
        public int? Fk_InvoiceType { get; set; }
        public int? FK_ReasonCodeId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }

        public int WhrHouse { set; get; }
        public List<ItemTransactionDTO> Details { get; set; } = new();

    }

    public class ItemTransactionDTO
    {
        public int ID { get; set; }
        public int FK_HeaderId { get; set; }
        public string? KeyId { get; set; }

        public int FK_ItemId { get; set; }
        public string? ItemNameAr { get; set; }
        public string? ItemNameEn { get; set; }

        public int FK_WarehouseId { get; set; }
        public string? WarehouseNameAr { get; set; }
        public string? WarehouseNameEn { get; set; }

        public int? FK_UnitId { get; set; }
        public string? UnitNameAr { get; set; }
        public string? UnitNameEn { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        public string StockType { get; set; } = string.Empty;

        public int FK_LocatorId { get; set; }
        public string? LocatorName { get; set; }

        public string Description { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int ComapnyId { get; set; }
        public int BranchId { get; set; }
        public string Serial { get; set; } = string.Empty;
        public string Batch { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public int FK_TransactionStatusId { get; set; }
    }

    public class DeleteInventoryTransactionDTO
    {
        public long ID { get; set; }
        public int ModifiedBy { get; set; }
        public long FK_FinancialTransactionReverseId { get; set; }
    }
}
