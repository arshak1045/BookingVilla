using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Application.Services.Interface;
using BookingVilla.Domain.Entities;
using static BookingVilla.Application.Common.Utility.StaticDetails;

namespace BookingVilla.Application.Services.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateBooking(Booking booking)
        {
            _unitOfWork.BookingRepository.Add(booking);
            _unitOfWork.BookingRepository.Save();
        }

        public IEnumerable<Booking> GetAllBookings(string userId = "", string statusFilterList = "")
        {
            IEnumerable<string> statusList = statusFilterList.ToLower().Split(',');
            if (!string.IsNullOrEmpty(statusFilterList) && !string.IsNullOrEmpty(userId))
            {
                return _unitOfWork.BookingRepository.GetAll(b => statusList.Contains(b.Status.ToLower()) &&
                   b.UserId == userId, includeProperties: "User,Villa");
            }
            else
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    return _unitOfWork.BookingRepository.GetAll(x => x.UserId == userId, includeProperties: "User,Villa");
                }
                if (!string.IsNullOrEmpty(statusFilterList))
                {
                    return _unitOfWork.BookingRepository.GetAll(
                        b => statusList.Contains(b.Status.ToLower()), includeProperties: "User,Villa");
                }
            }
            return _unitOfWork.BookingRepository.GetAll();
        }

        public Booking GetBookingById(int id)
        {
            return _unitOfWork.BookingRepository.Get(b => b.Id == id, includeProperties: "User,Villa");
        }

        public IEnumerable<int> GetCheckedInVillaNumbers(int villaId)
        {
            return _unitOfWork.BookingRepository.GetAll(
                u => u.VillaId == villaId && u.Status == BookStatus.StatusCheckedIn).Select(u => u.VillaNumber);
        }

        public void UpdateStatus(int bookingId, string orderStatus, int villaNumber = 0)
        {
            var booking = _unitOfWork.BookingRepository.Get(b => b.Id == bookingId, tracked: true);
            if (booking != null)
            {
                booking.Status = orderStatus;
                if (booking.Status == StaticDetails.BookStatus.StatusCheckedIn)
                {
                    booking.VillaNumber = villaNumber;
                    booking.CheckInDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }
                if (booking.Status == StaticDetails.BookStatus.StatusCompleted)
                {
                    booking.CheckOutDate = DateOnly.FromDateTime(DateTime.UtcNow);
                }
            }
            _unitOfWork.BookingRepository.Save();
        }

        public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
        {
            var booking = _unitOfWork.BookingRepository.Get(b => b.Id == bookingId, tracked: true);
            if (booking != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    booking.StripeSessionId = sessionId;
                }
                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    booking.StripePaymentIntentId = paymentIntentId;
                    booking.PaymentDate = DateTime.UtcNow;
                    booking.IsPaymentSuccessful = true;
                }
            }
            _unitOfWork.BookingRepository.Save();
        }
    }
}
