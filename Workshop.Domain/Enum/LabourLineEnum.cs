using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workshop.Domain.Enum
{
    public enum LabourLineEnum  // the number as it in DB >> LKP_Def_Lookup_Details
    {
        M = 18, //Draft
        Booked = 19, //Booked
        WIP = 20, //WIP technician clock-in
        P = 21, //Waiting For Parts
        Approval = 22, //Approval
        WaitingForLabour = 23, //Waiting for Labour
        Tranfer = 24, // Transfer
        Completed = 25, //Completed
        TranferCompleted = 26, //TranferCompleted 
    }
}
