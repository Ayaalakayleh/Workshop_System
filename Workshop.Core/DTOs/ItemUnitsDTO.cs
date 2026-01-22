using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class ItemUnitsDTO
    {
        public long ItemId { get; set; }
        public int UnitId { get; set; }
        public string? UnitCode { get; set; }
        public string? UnitPrimaryName { get; set; }
        public string? UnitSecondaryName { get; set; }
        public decimal? ConversionFactor { get; set; }
        public bool? IsDecimalUnit { get; set; }
        public bool? IsUnitActive { get; set; }
        public bool? IsBaseUnit { get; set; }

    }
}
