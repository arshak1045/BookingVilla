using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;

namespace BookingVilla.Application.Common.Interface
{
	public interface IBookingRepository : IRepository<Booking>
	{
		void Update(Booking booking);
		void Save();
		void UpdateStatus(int bookingId, string orderStatus);
		void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId);

	}
}
