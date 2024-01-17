using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVilla.Application.Common.Utility
{
    public static class StaticDetails
    {
        public struct Roles
        {
            public const string Admin = "Admin";   
            public const string Customer = "Customer";   
        }

        public struct BookStatus
        {
            public const string StatusPending = "Pending";
            public const string StatusApproved = "Approved";
            public const string StatusCheckedIn = "CheckedIn";
            public const string StatusCompleted = "Completed";
            public const string StatusCanceled = "Canceled";
            public const string StatusRefunded = "Refunded";
        }
    }
}
