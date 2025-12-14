using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class AccountDefinitionBaseDTO
    {
        public int JournalId { get; set; }
        public int WIPAccountId { get; set; }
        public int MaintenanceAccountId { get; set; }
        public int AccessoriesAccountId { get; set; }
        public int AccidentAccountId { get; set; }
        public int MaintenanceProjectsAccountId { get; set; }

        public int InternalCostPartId { get; set; }
        public int InternalCostLabourId { get; set; }
        public int ExternalCostPartId { get; set; }
        public int ExternalCostLabourId { get; set; }
        public int InternalRevenuePartId { get; set; }
        public int InternalRevenueLabourId { get; set; }
        public int InvoiceTypeId { get; set; }
        public int CompanyId { get; set; }
    }

    public class AccountDefinitionDTO : AccountDefinitionBaseDTO
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }

    public class InventoryAccountDefinitionsDTO
    {
        public int FK_JournalNameId { get; set; }
        public int FK_InvoiceTypeId { get; set; }
        public int FK_ItemInTransferAccountId { get; set; }
        public int FK_AdjustmentAccountId { get; set; }
        public int FK_DamageAccountId { get; set; }
        public int FK_WIPAccountId { get; set; }
    }


}
