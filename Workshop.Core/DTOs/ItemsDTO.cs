using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class ItemsDTO
    {
        public int WIPId { get; set; }
        public IEnumerable<BaseItemDTO>? ItemsList { get; set; }
        public string? Items { get; set; }
    }
}
