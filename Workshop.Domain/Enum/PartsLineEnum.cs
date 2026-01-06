using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Enum
{
    public enum PartsLineEnum  // the number as it in DB >> LKP_Def_Lookup_Details
    {
        M = 32, //Draft
        Compleated = 33, //Compleated Work
        Delete = 34, //Delete Line
        Lost1 = 35, //Lost sale 1
        Lost2 = 36, //Lost sale 2
        Notice = 37, //Notice
        WIP = 38, //Work in process
        Booked = 39, //Delete Line
        L = 40, //Waiting Labour
        P = 41, //Waiting For Parts
        Q = 42, //Part received
        A = 43, //Awaiting Authority

    }
}
