using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Enum
{
    //Important: Check table in ERP WorkflowEnum to get right enum
    public enum WorkflowEnum
    {
        PriceQuotation = 1,
        InsuranceClaim = 2,
        PurchaseOrder = 3,
        CustomerApproval = 4,
        RequestQuotations = 5,
        Prelease = 6,
        PreleasePool = 7,
        FleetCycle = 8,
        UsedCarTransfer = 9,
        SalesOrder = 10,
        EarlyTermination = 11,
        LeadApproval = 12,
        PettyCashRequest = 13,
        PettyCashPayment = 14,
        InternalPurchaseRequestIndirect = 15,
        InternalPurchaseRequest = 16,
        PurchaseOrderIndirect = 17,
        PartsFirstLevel = 18,
        PartsSecondLevel = 19,
        PartsThirdLevel = 20,
        PartsFourthLevel = 21,
        PartsFifthLevel = 22,
        RFQ = 23,
        RFQIndirect = 24,
        ProjectReplacement = 25,
        ReplacementCreditNote = 26,
        ReplacementCreditNoteGreater = 27,
        CustomerIRR = 28,
        ReceivePOChangeQty = 29,
        DeliveryNotedirect = 30,
        DeliveryNoteIndirect = 31,
        PaymentRequestdirect = 32,
        PaymentRequestIndirect = 33,
        PurchaseInvoicedirect = 34,
        PurchaseInvoiceIndirect = 35,

    }
}
