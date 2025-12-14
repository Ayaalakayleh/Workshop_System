using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public abstract class BaseWIPInvoice
    {
        public int WIPId { get; set; }
        public int? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public int? InvoiceType { get; set; }
        public int AccountType { get; set; }
        public int? TransactionMasterId { get; set; }
        public int? TransactionCostMasterId { get; set; }
        public int? ReferanceNo { get; set; }
        public decimal? Total { get; set; }
        public decimal? Tax { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Net { get; set; }
        public bool? IsReturn { get; set; }
    }

    public class WIPInvoiceDTO : BaseWIPInvoice
    {
        public int Id { get; set; }
    }

    public class CreateWIPInvoiceDTO : BaseWIPInvoice
    {
       
        public int? OldTransactionMasterId { get; set; }
        public int CreatedBy { get; set; }
    }

}
