using BookingVilla.Application.Common.Utility;
using BookingVilla.Application.Services.Interface;
using BookingVilla.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingVilla.Controllers
{
	[Authorize(Roles = StaticDetails.Roles.Admin)]
    public class AmenityController : Controller
    {
		private readonly IAmenityService _amenityService;
		private readonly IVillaService _villaService;

        public AmenityController(IAmenityService amenityService, IVillaService villaService)
        {
            _amenityService = amenityService;
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var amenities = _amenityService.GetAllAmenities();
            return View(amenities);
        }

        public IActionResult Create() 
        {
            AmenityVM AmenityVM = new AmenityVM()
            {
                VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                })
            };
            return View(AmenityVM);
        }

        [HttpPost]
        public IActionResult Create(AmenityVM vN)
        {
            if(ModelState.IsValid)
            {
				_amenityService.CreateAmenity(vN.Amenity);
                TempData["success"] = "Success! The amenity is created successfully.";
                return RedirectToAction(nameof(Index));
            }

            vN.VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
			return View(vN);
        }

        public IActionResult Update(int amenityId)
        {
			AmenityVM amenityVM = new AmenityVM()
			{
				VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
                Amenity = _amenityService.GetAmenityById(amenityId)
			};
			return View(amenityVM);
		}

        [HttpPost]
		public IActionResult Update(AmenityVM AmenityVM)
		{
			if (ModelState.IsValid)
			{
				_amenityService.UpdateAmenity(AmenityVM.Amenity);
				TempData["success"] = "Success! The amenity is updated successfully.";
				return RedirectToAction(nameof(Index));
			}

			AmenityVM.VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
			{
				Text = item.Name,
				Value = item.Id.ToString()
			});
            return View(AmenityVM);
		}

        public IActionResult Delete(int amenityId) 
        {
			AmenityVM amenityVM = new AmenityVM()
			{
				VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
				Amenity = _amenityService.GetAmenityById(amenityId)
			};
			return View(amenityVM);
		}

		[HttpPost]
		public IActionResult Delete(AmenityVM amenityVM)
		{
			if (_amenityService.DeleteAmenity(amenityVM.Amenity.Id))
			{
                TempData["success"] = "Success! The amenity is deleted successfully.";
                return RedirectToAction("Index");
			}
			TempData["error"] = "Error! The amenity could not be deleted";
			return View(nameof(Index));
		}
	}
}
