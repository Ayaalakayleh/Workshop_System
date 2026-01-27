using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workshop.Core.DTOs.AccountingDTOs;
using Workshop.Core.DTOs.General;
using Workshop.Core.DTOs.Vehicle;

namespace Workshop.Core.DTOs
{
    public class AgreementCustomerDTO
    {
        public long AgreementId { get; set; }
        public string CustomerPrimaryName { get; set; }
        public string CustomerSecondaryname { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CompanyPrimaryName { get; set; }
        public string CompanySecondaryName { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhoneNumber { get; set; }
    }
}
