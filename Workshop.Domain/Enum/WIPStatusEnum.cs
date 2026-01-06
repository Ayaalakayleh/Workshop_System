using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Enum
{
    public enum WIPStatusEnum  // the number as it in DB >> LKP_Def_Lookup_Details
    {
        M = 2024, //Draft
        B = 2025, //Compleated Work
        W = 2026, //Delete Line
        P = 2027, //Lost sale 1
        L = 2028, //Lost sale 2
        A = 2029, //Notice
        C = 2030, //Compleated
        G = 2031, //Gone but not invoiced
        Invoiced = 2032, //Invoiced


    }
}
