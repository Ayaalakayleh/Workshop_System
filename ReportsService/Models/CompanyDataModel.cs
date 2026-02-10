using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ReportsService.Models
{
    public class CompanyDataModel
    {
        public string Title { get; set; } = null;
        public string CompanyPrimaryName { get; set; } = null;
        public string Branch { get; set; } = null;
        public byte[] Img { get; set; }
    }
}