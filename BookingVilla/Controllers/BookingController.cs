using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookingVilla.Controllers
{
	public class BookingController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public BookingController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		[Authorize]
		public IActionResult FinalizeBooking(int nights, DateOnly checkInDate, int villaId )
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			AppUser user = _unitOfWork.AppUserRepository.Get(user => user.Id == userId);
			Booking booking = new Booking()
			{
				VillaId = villaId,
				Villa = _unitOfWork.VillaRepository.Get(villa => villa.Id == villaId, includeProperties: "Amenities"),
				CheckInDate = checkInDate,
				Nights = nights,
				CheckOutDate = checkInDate.AddDays(nights),
				UserId = userId,
				Phone = user.PhoneNumber,
				Email = user.Email,
				Name = user.Name,

			};
			booking.TotalCost = booking.Villa.Price * nights;

			return View(booking);
		}

		[Authorize]
		[HttpPost]
		public IActionResult FinalizeBooking(Booking booking)
		{
			var villa = _unitOfWork.VillaRepository.Get(villa => villa.Id == booking.VillaId);
			booking.TotalCost = villa.Price * booking.Nights;

			booking.Status = StaticDetails.BookStatus.StatusPending;
			booking.BookingDate = DateTime.Now;

			_unitOfWork.BookingRepository.Add(booking);
			_unitOfWork.BookingRepository.Save();

			var domain = $"{Request.Scheme}://{Request.Host.Value}/";
			var options = new SessionCreateOptions
			{
				LineItems = new List<SessionLineItemOptions>(),
				Mode = "payment",
				SuccessUrl = domain + $"Booking/BookingConfirmation?bookingId={booking.Id}",
				CancelUrl = domain + $"Booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate}&nights{booking.Nights}",
			};

			options.LineItems.Add(new SessionLineItemOptions
			{
				PriceData = new SessionLineItemPriceDataOptions()
				{
					UnitAmount = (long)(booking.TotalCost * 100),
					Currency = "usd",
					ProductData = new SessionLineItemPriceDataProductDataOptions()
					{
						Name = villa.Name,
					}
				},
				Quantity = 1
			});

			var service = new SessionService();
			Session session = service.Create(options);

			_unitOfWork.BookingRepository.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
			_unitOfWork.BookingRepository.Save();

			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}

		[Authorize]
		public IActionResult BookingConfirmation(int bookingId)
		{
			Booking booking = _unitOfWork.BookingRepository.Get(x => x.Id == bookingId,
				includeProperties: "User,Villa");

			if (booking.Status == StaticDetails.BookStatus.StatusPending )
			{
				var service = new SessionService();
				Session session = service.Get(booking.StripeSessionId);

				if (session.PaymentStatus == "paid")
				{
					_unitOfWork.BookingRepository.UpdateStatus(booking.Id, StaticDetails.BookStatus.StatusApproved);
					_unitOfWork.BookingRepository.UpdateStripePaymentId(booking.Id,session.Id, session.PaymentIntentId);
					_unitOfWork.BookingRepository.Save();
				}
			}

			return View(bookingId);
		}

	}
}
