using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;
using static BookingVilla.Application.Common.Utility.StaticDetails;

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
		public IActionResult Index()
		{
			return View();
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
			booking.Status = BookStatus.StatusPending;
			booking.BookingDate = DateTime.Now;
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(b => b.Status == StaticDetails.BookStatus.StatusApproved ||
            b.Status == StaticDetails.BookStatus.StatusCheckedIn).ToList();
			int roomAvailable = StaticDetails.VillaNumberAvailability_Count
                (villa.Id, villaNumbersList, booking.CheckInDate, booking.Nights, bookedVillas);

			if (roomAvailable == 0)
			{
				TempData["error"] = "Room has been sold out";
				return RedirectToAction(nameof(FinalizeBooking), new
				{
					villaId = villa.Id,
					checkInDate = booking.CheckInDate,
					nights = booking.Nights
				});
			}

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

			if (booking.Status == BookStatus.StatusPending )
			{
				var service = new SessionService();
				Session session = service.Get(booking.StripeSessionId);

				if (session.PaymentStatus == "paid")
				{
					_unitOfWork.BookingRepository.UpdateStatus(booking.Id, BookStatus.StatusApproved, 0);
					_unitOfWork.BookingRepository.UpdateStripePaymentId(booking.Id,session.Id, session.PaymentIntentId);
					_unitOfWork.BookingRepository.Save();
				}
			}

			return View(bookingId);
		}

		[Authorize]
		public IActionResult BookingDetails(int bookingId)
		{
			Booking booking = _unitOfWork.BookingRepository.Get(x => x.Id == bookingId,
				includeProperties: "User,Villa");

			if (booking.VillaNumber == 0 && booking.Status == BookStatus.StatusApproved)
			{
				var availableVillaNumber = AssignAvailableVillaNumberByVilla(booking.VillaId);
				booking.VillaNumbers = _unitOfWork.VillaNumberRepository.GetAll(u => u.VillaId == booking.VillaId
				&& availableVillaNumber.Any(x => x == u.Villa_Number)).ToList();
			}

			return View(booking);
		}

		[HttpPost]
		[Authorize(Roles = Roles.Admin)]
		public IActionResult CheckIn(Booking booking)
		{
			_unitOfWork.BookingRepository.UpdateStatus(booking.Id, StaticDetails.BookStatus.StatusCheckedIn, booking.VillaNumber);
			_unitOfWork.BookingRepository.Save();
			TempData["Success"] = "Booking Updated Successfully.";
			return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
		}

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.BookingRepository.UpdateStatus(booking.Id, StaticDetails.BookStatus.StatusCompleted, booking.VillaNumber);
            _unitOfWork.BookingRepository.Save();
            TempData["Success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Cancel(Booking booking)
        {
            _unitOfWork.BookingRepository.UpdateStatus(booking.Id, StaticDetails.BookStatus.StatusCanceled, 0);
            _unitOfWork.BookingRepository.Save();
            TempData["Success"] = "Booking Canceled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
		{
			List<int> availableVillaNumber = new();
			var villaNumbers = _unitOfWork.VillaNumberRepository.GetAll(u => u.VillaId == villaId);
			var checkedInVilla = _unitOfWork.BookingRepository.GetAll(u => u.VillaId == villaId &&
			u.Status == BookStatus.StatusCheckedIn).Select(u => u.VillaNumber);

			foreach (var villaNumber in villaNumbers)
			{
				if(!checkedInVilla.Contains(villaNumber.Villa_Number))
				{
					availableVillaNumber.Add(villaNumber.Villa_Number);
				}
			}

			return availableVillaNumber;
		}

		#region API_Calls
		[Authorize]
		[HttpGet]
		public IActionResult GetAll(string status) 
		{
			IEnumerable<Booking> bookings;

			if (User.IsInRole(Roles.Admin))
			{
				bookings = _unitOfWork.BookingRepository.GetAll(includeProperties: "User,Villa");
			}
			else
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

				bookings = _unitOfWork.BookingRepository.GetAll(b => b.UserId == userId, 
					includeProperties: "User,Villa");
			}
			if(!string.IsNullOrEmpty(status))
			{
				bookings = bookings.Where(b => b.Status.ToLower().Equals(status.ToLower()));
			}

			return Json(new {data = bookings});
		}

        #endregion
    }
}
