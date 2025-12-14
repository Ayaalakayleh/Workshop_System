using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class WipInvoiceDetailDTO
    {
        public int Id { get; set; }
        public int HeaderId { get; set; }
        public int? ServiceId { get; set; }
        public int? ItemId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }

        public string FullDescription { get; set; }
    }
}
