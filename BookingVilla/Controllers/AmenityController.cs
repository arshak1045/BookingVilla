using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Domain.Entities;
using BookingVilla.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingVilla.Controllers
{
	[Authorize(Roles = StaticDetails.Roles.Admin)]
    public class AmenityController : Controller
    {

		private readonly IUnitOfWork _unitOfWork;

		public AmenityController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
        {
            var amenities = _unitOfWork.AmenityRepository.GetAll(includeProperties: "Villa");
            return View(amenities);
        }

        public IActionResult Create() 
        {
            AmenityVM AmenityVM = new AmenityVM()
            {
                VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
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
				_unitOfWork.AmenityRepository.Add(vN.Amenity);
				_unitOfWork.AmenityRepository.Save();
                TempData["success"] = "Success! The amenity is created successfully.";
                return RedirectToAction(nameof(Index));
            }

            vN.VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
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
				VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
                Amenity = _unitOfWork.AmenityRepository.Get(item => item.Id == amenityId)
			};
			return View(amenityVM);
		}

        [HttpPost]
		public IActionResult Update(AmenityVM AmenityVM)
		{

			if (ModelState.IsValid)
			{
				_unitOfWork.AmenityRepository.Update(AmenityVM.Amenity);
				_unitOfWork.AmenityRepository.Save();
				TempData["success"] = "Success! The amenity is updated successfully.";
				return RedirectToAction(nameof(Index));
			}

			AmenityVM.VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
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
				VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
				Amenity = _unitOfWork.AmenityRepository.Get(item => item.Id == amenityId)
			};
			return View(amenityVM);
		}

		[HttpPost]
		public IActionResult Delete(AmenityVM amenityVM)
		{
            Amenity? amenityForRemove = _unitOfWork.AmenityRepository.Get(item =>
            item.Id == amenityVM.Amenity.Id);

			if (amenityForRemove is not null)
			{
				_unitOfWork.AmenityRepository.Remove(amenityForRemove);
				_unitOfWork.AmenityRepository.Save();
                TempData["success"] = "Success! The amenity is deleted successfully.";
                return RedirectToAction("Index");
			}
			TempData["error"] = "Error! The amenity could not be deleted";
			return View(nameof(Index));
		}
	}
}
