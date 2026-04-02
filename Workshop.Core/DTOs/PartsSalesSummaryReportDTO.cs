using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class PartsSalesSummaryReportDTO
    {

        public int? WIP { get; set; }
        public DateTime? InvoiceDateStart { get; set; }
        public DateTime? InvoiceDateEnd { get; set; }
        public int? CustomerId { get; set; }

    }

}
