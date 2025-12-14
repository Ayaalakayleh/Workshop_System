using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Enum
{
    public enum PricingMethodEnum
    {
        Sum,
        Fixed
    }

    public enum AccountTypeEnum
    {
        Internal=1,
        External=2
    }

    public enum AccountInternalMatchEnum
    {
        Rented = 1,
        Company = 2
    }

    public enum AccountExternalMatchEnum
    {
        Customer=1,
        Warranty=2,
        Insurance=3,
        Employee=4
    }

    
    public enum LabourLineEnum  // the number as it in DB >> LKP_Def_Lookup_Details
    {
        M = 18, //Draft
        Booked = 19, //Booked
        WIP = 20, //WIP technician clock-in
        P = 21, //Waiting For Parts
        Approval = 22, //Approval
        L = 23, //Waiting for Labour
        QA = 24, //QA
        Completed = 25, //Completed
        Invoiced = 26, //Invoiced
    }
    
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
    public enum ReservationStatus
    {
        New = 44,
        Booked = 45,
        Completed = 46,
        Canceled = 47,
        NotShow = 48,
        UnderRepair = 49
    }

}
