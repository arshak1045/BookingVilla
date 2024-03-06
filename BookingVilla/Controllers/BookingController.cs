using BookingVilla.Application.Common.Utility;
using BookingVilla.Application.Services.Interface;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
		private readonly IWebHostEnvironment _webHostEnvironment;
		private readonly IVillaService _villaService;
		private readonly IVillaNumberService _villaNumberService;
		private readonly IBookingService _bookingService;
		private readonly UserManager<AppUser> _userManager;

		public BookingController(IWebHostEnvironment webHostEnvironment,
			IVillaService villaService, IVillaNumberService villaNumberService,
			IBookingService bookingService, UserManager<AppUser> userManager)
		{
			_webHostEnvironment = webHostEnvironment;
			_villaService = villaService;
			_villaNumberService = villaNumberService;
			_bookingService = bookingService;
			_userManager = userManager;
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
			AppUser user = _userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
			Booking booking = new Booking()
			{
				VillaId = villaId,
				Villa = _villaService.GetVilla(villaId),
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
			var villa = _villaService.GetVilla(booking.VillaId);
			booking.TotalCost = villa.Price * booking.Nights;
			booking.Status = BookStatus.StatusPending;
			booking.BookingDate = DateTime.Now;

			if (!_villaService.IsAvailableVillaByDate(villa.Id, booking.Nights, booking.CheckInDate))
			{
				TempData["error"] = "Room has been sold out";
				return RedirectToAction(nameof(FinalizeBooking), new
				{
					villaId = villa.Id,
					checkInDate = booking.CheckInDate,
					nights = booking.Nights
				});
			}
            _bookingService.CreateBooking(booking);

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
			_bookingService.UpdateStripePaymentId(booking.Id, session.Id, session.PaymentIntentId);
			Response.Headers.Add("Location", session.Url);
			return new StatusCodeResult(303);
		}

		[Authorize]
		public IActionResult BookingConfirmation(int bookingId)
		{
			Booking booking = _bookingService.GetBookingById(bookingId);

			if (booking.Status == BookStatus.StatusPending )
			{
				var service = new SessionService();
				Session session = service.Get(booking.StripeSessionId);

				if (session.PaymentStatus == "paid")
				{
					_bookingService.UpdateStatus(booking.Id, BookStatus.StatusApproved, 0);
                    _bookingService.UpdateStripePaymentId(booking.Id,session.Id, session.PaymentIntentId);
				}
			}
			return View(bookingId);
		}

		[Authorize]
		public IActionResult BookingDetails(int bookingId)
		{
			Booking booking = _bookingService.GetBookingById(bookingId);

			if (booking.VillaNumber == 0 && booking.Status == BookStatus.StatusApproved)
			{
				var availableVillaNumber = AssignAvailableVillaNumberByVilla(booking.VillaId);
				booking.VillaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == booking.VillaId
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
			Booking booking = _bookingService.GetBookingById(id);
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
			_bookingService.UpdateStatus(booking.Id, BookStatus.StatusCheckedIn, booking.VillaNumber);
			TempData["Success"] = "Booking Updated Successfully.";
			return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
		}

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, BookStatus.StatusCompleted, booking.VillaNumber);
            TempData["Success"] = "Booking Completed Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public IActionResult Cancel(Booking booking)
        {
            _bookingService.UpdateStatus(booking.Id, BookStatus.StatusCanceled, 0);
            TempData["Success"] = "Booking Canceled Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
		{
			List<int> availableVillaNumber = new();
			var villaNumbers = _villaNumberService.GetAllVillaNumbers().Where(u => u.VillaId == villaId);
			var checkedInVilla = _bookingService.GetCheckedInVillaNumbers(villaId);

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
			string userId = "";

            if (!string.IsNullOrEmpty(status))
            {
                status = "";
            }
            if (!User.IsInRole(Roles.Admin))
			{
				var claimsIdentity = (ClaimsIdentity)User.Identity;
				userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
			}
            bookings = _bookingService.GetAllBookings(userId, status);
            return Json(new {data = bookings});
		}
        #endregion
    }
}
