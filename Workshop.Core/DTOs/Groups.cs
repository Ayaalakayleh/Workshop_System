using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class Groups
    {
        public int GroupID { get; set; }
        public int BranchId { get; set; }
        public int CompanyId { get; set; }
        public string PrimaryGroupName { get; set; }
        public string SecondaryGroupName { get; set; }
        public string Name { get; set; }

    }
    public class GroupOfUserPermission
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string GroupPermission { get; set; }
    }


}
