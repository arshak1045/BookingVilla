using BookingVilla.Domain.Entities;

namespace BookingVilla.Application.Services.Interface
{
    public interface IBookingService
    {
        void CreateBooking(Booking booking);
        Booking GetBookingById(int id);
        IEnumerable<Booking> GetAllBookings(string userId="", string statusFilterList="");
        void UpdateStatus(int bookingId, string orderStatus, int villaNumber);
        void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId);
        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId);
    }
}
