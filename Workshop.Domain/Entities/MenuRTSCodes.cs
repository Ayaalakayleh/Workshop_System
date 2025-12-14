using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Entities
{
    public class MenuRTSCodes
    {
        public int Id { get; set; }
        public int FK_MenuId { get; set; } 
        public int FK_RTSId { get; set; }    
        public int SequenceNo { get; set; } = 1;
        public decimal Time { get; set; } = 1;

        // Navigation Properties
        public Menu Menu { get; set; }
        public RTSCode RTSCode { get; set; }
    }
}
