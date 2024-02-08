using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using System.Security.Claims;
using static BookingVilla.Application.Common.Utility.StaticDetails;

namespace BookingVilla.Controllers
{
	public class BookingController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
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
		[Authorize]
		public IActionResult GenerateInvoice(int id, string downloadType)
		{
			string basePath = _webHostEnvironment.WebRootPath;
			WordDocument document = new();
			//Load Template
			string dataPath = $"{basePath}/exports/BookingDetails.docx";
			using FileStream fileStream = new(
				dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			document.Open(fileStream, FormatType.Automatic);

			//Update Template
			Booking booking = _unitOfWork.BookingRepository.Get(
				b => b.Id == id, includeProperties: "User,Villa");
			TextSelection textSelection = document.Find(
				"xx_customer_name", false, true);
			WTextRange textRange = textSelection.GetAsOneRange();
			textRange.Text = booking.User.Name;

			textSelection = document.Find("xx_customer_email", false, true);
			textRange = textSelection.GetAsOneRange();
			textRange.Text = booking.Email;

            textSelection = document.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = booking.User.PhoneNumber;

            textSelection = document.Find("xx_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = $"Booking Id - {booking.Id}";

            textSelection = document.Find("xx_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = $"Booking Date - {booking.BookingDate.ToShortDateString()}";

            textSelection = document.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = booking.PaymentDate.ToShortDateString();

            textSelection = document.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = booking.CheckInDate.ToShortDateString();

            textSelection = document.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = booking.CheckOutDate.ToShortDateString();

            textSelection = document.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = booking.TotalCost.ToString("c");

			WTable table = new (document);
			table.TableFormat.Borders.LineWidth = 1f;
			table.TableFormat.Borders.Color = Color.Black;
			table.TableFormat.Paddings.Top = 7f;
			table.TableFormat.Paddings.Bottom = 7f;
			table.TableFormat.Borders.Horizontal.LineWidth = 1f;

			int rows = booking.VillaNumber > 0 ? 3 : 2;
			table.ResetCells(rows, 4);

			WTableRow row0 = table.Rows[0];
			row0.Cells[0].AddParagraph().AppendText("Nights");
			row0.Cells[0].Width = 80;
			row0.Cells[1].AddParagraph().AppendText("Villa");
			row0.Cells[1].Width = 220;
			row0.Cells[2].AddParagraph().AppendText("Price Per Night");
			row0.Cells[3].AddParagraph().AppendText("Total");
			row0.Cells[3].Width = 80;

			WTableRow row1 = table.Rows[1];
			row1.Cells[0].AddParagraph().AppendText(booking.Nights.ToString());
			row1.Cells[0].Width = 80;
			row1.Cells[1].AddParagraph().AppendText(booking.Villa.Name);
            row1.Cells[1].Width = 220;
            row1.Cells[2].AddParagraph().AppendText((booking.TotalCost / booking.Nights).ToString());
			row1.Cells[3].AddParagraph().AppendText((booking.TotalCost).ToString("c"));
            row1.Cells[3].Width = 80;

			if (booking.VillaNumber > 0)
			{
                WTableRow row2 = table.Rows[2];
                row2.Cells[0].Width = 80;
                row2.Cells[1].AddParagraph().AppendText($"Villa Number - {booking.VillaNumber}");
                row2.Cells[1].Width = 220;
                row2.Cells[3].Width = 80;
            }

			WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
			tableStyle.TableProperties.RowStripe = 1;
			tableStyle.TableProperties.ColumnStripe = 2;
			tableStyle.TableProperties.Paddings.Top = 2;
			tableStyle.TableProperties.Paddings.Bottom = 1;
			tableStyle.TableProperties.Paddings.Left = 5.4f;
			tableStyle.TableProperties.Paddings.Right = 5.4f;

			ConditionalFormattingStyle firstRowStyle = tableStyle.
				ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
			firstRowStyle.CharacterFormat.Bold = true;
			firstRowStyle.CharacterFormat.TextColor = Color.FromArgb(255, 255, 255, 255);
			firstRowStyle.CellProperties.BackColor = Color.Black;

			table.ApplyStyle("CustomStyle");

			TextBodyPart textBodyPart = new (document);
			textBodyPart.BodyItems.Add(table);

			document.Replace("<ADDTABLEHERE>", textBodyPart, false, false);

            using DocIORenderer renderer = new ();
			MemoryStream stream = new ();

			if (downloadType == "word")
			{
                document.Save(stream, FormatType.Docx);
                stream.Position = 0;

                return File(stream, "application/docx", "BookingDetails.docx");
            }
			else
			{
				PdfDocument pdfDocument= renderer.ConvertToPDF(document);
				pdfDocument.Save(stream);
				stream.Position = 0;

				return File(stream, "application/pdf", "BookingDetails.pdf");
			}

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
