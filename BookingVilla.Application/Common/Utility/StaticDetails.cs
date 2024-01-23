using BookingVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public static int VillaNumberAvailability_Count(int villaId, 
            List<VillaNumber> villaNumbersList, DateOnly checkInDate, int nights,
            List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int availableRooms = int.MaxValue;
            var roomNumbers = villaNumbersList.Where(r => r.VillaId == villaId).Count();

            for (int i = 0; i < nights; i++)
            {
                var bookedVillas = bookings.Where(b => b.CheckInDate <= checkInDate.AddDays(i) &&
                b.CheckOutDate > checkInDate.AddDays(i) && b.VillaId == villaId);

                foreach (var booking in bookedVillas)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomNumbers - bookingInDate.Count;
                if (totalAvailableRooms == 0)
                {
                    return 0;
                }
                else
                {
                    if(availableRooms > totalAvailableRooms)
                    {
                        availableRooms = totalAvailableRooms;
                    }
                }
            }
            return availableRooms;
        }
    }
}
