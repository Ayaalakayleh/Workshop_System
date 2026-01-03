using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class UpdateSinglePartWarehouseDTO
    {
        public int Id { get; set; }
        public int WIPId { get; set; }
        public decimal? Quantity { get; set; }
        public int WarehouseId { get; set; }
        public int LocatorId { get; set; }
    }
}
