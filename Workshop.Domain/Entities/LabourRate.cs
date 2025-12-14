using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Entities
{
    public class LabourRate
    {
        public int Id { get; set; }
        public string LabourCode { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionAr { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
