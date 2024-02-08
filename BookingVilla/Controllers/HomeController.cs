using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Models;
using BookingVilla.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Presentation;
using System.Diagnostics;

namespace BookingVilla.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            HomeVM vm = new HomeVM()
            {
                VillaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "Amenities"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "Amenities");
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(b => b.Status == StaticDetails.BookStatus.StatusApproved ||
            b.Status == StaticDetails.BookStatus.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {
                int roomAvailable = StaticDetails.VillaNumberAvailability_Count
                    (villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }

            HomeVM vm = new HomeVM()
            {
                VillaList = villaList,
                Nights = nights,
                CheckInDate = checkInDate
            };

            return PartialView("_VillaList", vm);
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GeneratePPTExpoert(int id)
        {
            var villa = _unitOfWork.VillaRepository
                .GetAll(includeProperties: "Amenities").FirstOrDefault(v => v.Id == id);

            if (villa == null)
            {
                return RedirectToAction(nameof(Error));
            }

            string basePath = _webHostEnvironment.WebRootPath;
            string filePath = $"{basePath}/Exports/ExportVillaDetails.pptx";

            using IPresentation presentation = Presentation.Open(filePath);

            ISlide slide = presentation.Slides[0];

            IShape? shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaName") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaDescription") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtOccupancy") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", villa.Occupancy.ToString());
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaSize") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = string.Format("Villa Size : {0} sqft", villa.Sqft.ToString());
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtPricePerNight") as IShape;
            if (shape != null)
            {
                shape.TextBody.Text = string.Format("USD {0}/night", villa.Price.ToString("c"));
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if (shape != null)
            {
                List<string> listItems = villa.Amenities.Select(x => x.Name).ToList();

                shape.TextBody.Text = "";

                foreach (var item in listItems)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                }
            }

            shape = slide.Shapes.FirstOrDefault(s => s.ShapeName == "imgVilla") as IShape;
            if (shape != null)
            {
                byte[] imageData;
                string imageUrl;

                try
                {
                    imageUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                catch (Exception)
                {
                    imageUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imageData = System.IO.File.ReadAllBytes(imageUrl);
                }
                slide.Shapes.Remove(shape);
                using MemoryStream imageStream = new(imageData);
                IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
            }

            MemoryStream memoryStream = new ();
            presentation.Save(memoryStream);
            memoryStream.Position = 0;

            return File(memoryStream, "application/pptx", "VillaDetails.pptx");
        }
    }
}
