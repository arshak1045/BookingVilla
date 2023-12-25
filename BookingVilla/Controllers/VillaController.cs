using BookingVilla.Application.Common.Interface;
using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookingVilla.Controllers
{
    public class VillaController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public VillaController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var villas = _unitOfWork.VillaRepository.GetAll();
            return View(villas);
        }

        public IActionResult Create() 
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if(villa.Name == villa.Description)
            {
                ModelState.AddModelError("", "The name and description should be differ");
            }

            if(ModelState.IsValid)
            {
                _unitOfWork.VillaRepository.Add(villa);
				_unitOfWork.VillaRepository.Save();
                TempData["success"] = "Success! The villa is created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Update(int villaId)
        {
            Villa? villa = _unitOfWork.VillaRepository.Get(x => x.Id == villaId);
            if(villa == null)
            {
				return RedirectToAction("Error", "Home");
			}
            return View(villa);
        }

        [HttpPost]
		public IActionResult Update(Villa villa)
		{
			if (ModelState.IsValid && villa.Id > 0)
			{
				_unitOfWork.VillaRepository.Update(villa);
				_unitOfWork.VillaRepository.Save();
                TempData["success"] = "Success! The villa is updated successfully.";
                return RedirectToAction(nameof(Index));
			}
			return View(nameof(Index));
		}


        public IActionResult Delete(int villaId) 
        {
			Villa? villa = _unitOfWork.VillaRepository.Get(x => x.Id == villaId);
			if (villa == null)
			{
				return RedirectToAction("Error", "Home");
			}
			return View(villa);
		}

		[HttpPost]
		public IActionResult Delete(Villa villa)
		{
            Villa? villaFromDb = _unitOfWork.VillaRepository.Get(x => x.Id == villa.Id);

			if (villaFromDb is not null)
			{
				_unitOfWork.VillaRepository.Remove(villaFromDb);
				_unitOfWork.VillaRepository.Save();
                TempData["success"] = "Success! The villa is deleted successfully.";
                return RedirectToAction(nameof(Index));
			}
			return View(nameof(Index));
		}
	}
}
