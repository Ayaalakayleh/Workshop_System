using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class DBPaymentType
    {
        public int Id { get; set; }
        public string PrimaryName { get; set; }
        public string SecondaryName { get; set; }
        public int NumberOfDay { get; set; }
        public bool Ajax { get; set; }


        public int CompanyId { get; set; }
        public int BranchId { get; set; }
        public int ModifiedBy { get; set; }
        public int CreatedBy { get; set; }


        public string CreatedOn { get; set; }
        public string ModifiedOn { get; set; }
        public string CreatedByName { get; set; }
        public string ModifiedByName { get; set; }
    }
}
