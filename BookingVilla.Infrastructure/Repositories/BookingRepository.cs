using BookingVilla.Application.Common.Interface;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVilla.Infrastructure.Repositories
{
	public class BookingRepository : Repository<Booking>, IBookingRepository
	{
		private readonly ApplicationDbContext _DbContext;

		public BookingRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_DbContext = dbContext;
		}

		public void Save()
		{
			_DbContext.SaveChanges();
		}

		public void Update(Booking booking)
		{
			_DbContext.Update(booking);
		}

		public void UpdateStatus(int bookingId, string orderStatus, int villaNumber = 0)
		{
			var booking = _DbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);
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
		}

		public void UpdateStripePaymentId(int bookingId, string sessionId, string paymentIntentId)
		{
			var booking = _DbContext.Bookings.FirstOrDefault(b => b.Id == bookingId);
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
		}
	}
}
