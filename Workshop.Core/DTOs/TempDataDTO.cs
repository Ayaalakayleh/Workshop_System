using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Core.DTOs.TempData
{
    public class TempData
    {
        public TempData()
        {
            DataList = new List<string>();
        }
        public int Id { get; set; }
        public object Data { get; set; }
        public List<string> DataList { get; set; }
        public bool IsSuccess { get; set; }
        public bool flag { get; set; }
        public string HeaderMsg { get; set; }
        public string Message { get; set; }
        public object Data1 { get; set; }
        public List<object> ResultList { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public decimal Amount { get; set; }
        public bool BoolType { get; set; }
        public int ReceiptId { get; set; }
        public int SafetyDepositId { get; set; }
        public int InvoiceNo { get; set; }
        public bool hasFine { get; set; }
        public bool TaxExempt { get; set; }

        public List<Notification> Notification { get; set; }
    }

    public class AjaxData
    {
        public string Data { get; set; }
        public int Id { get; set; }
    }

}
