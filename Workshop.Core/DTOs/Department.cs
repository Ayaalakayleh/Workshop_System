using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs
{
    public class Department
    {
        public int Id { get; set; }
        public string DepartmentPrimaryName { get; set; }
        public string DepartmentSecondaryName { get; set; }
        public string Name { get; set; }
    }
}
