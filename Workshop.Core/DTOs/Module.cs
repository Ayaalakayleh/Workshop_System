using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class ModulesDTO
    {
        public int Id { get; set; }
        public string ModulePrimaryName { get; set; }
        public string ModuleSecondaryName { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int version { get; set; }
        public int createdBy { get; set; }
        public int ModifyBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifyAt { get; set; }
        public string HubName { get; set; }
    }
}
