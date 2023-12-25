using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;
using BookingVilla.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;

namespace BookingVilla.Controllers
{
    public class VillaNumberController : Controller
    {

		private readonly IUnitOfWork _unitOfWork;

		public VillaNumberController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
        {
            var villaNumbers = _unitOfWork.VillaNumberRepository.GetAll(includeProperties: "Villa");
            return View(villaNumbers);
        }

        public IActionResult Create() 
        {
            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                })
            };
            return View(villaNumberVM);
        }

        [HttpPost]
        public IActionResult Create(VillaNumberVM vN)
        {
            bool isVillaNumberExists = _unitOfWork.VillaNumberRepository.Any(item => 
                item.Villa_Number == vN.VillaNumber.Villa_Number);

            if(ModelState.IsValid && !isVillaNumberExists)
            {
				_unitOfWork.VillaNumberRepository.Add(vN.VillaNumber);
				_unitOfWork.VillaNumberRepository.Save();
                TempData["success"] = "Success! The villa number is created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (isVillaNumberExists)
            {
                TempData["error"] = "The villa number already exists!";
            }

            vN.VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });

			return View(vN);
        }

        public IActionResult Update(int villaNumberId)
        {
			VillaNumberVM villaNumberVM = new VillaNumberVM()
			{
				VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
                VillaNumber = _unitOfWork.VillaNumberRepository.Get(item => item.Villa_Number == villaNumberId)
			};
			return View(villaNumberVM);
		}

        [HttpPost]
		public IActionResult Update(VillaNumberVM villaNumberVm)
		{

			if (ModelState.IsValid)
			{
				_unitOfWork.VillaNumberRepository.Update(villaNumberVm.VillaNumber);
				_unitOfWork.VillaNumberRepository.Save();
				TempData["success"] = "Success! The villa number is updated successfully.";
				return RedirectToAction(nameof(Index));
			}

			villaNumberVm.VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
			{
				Text = item.Name,
				Value = item.Id.ToString()
			});
            return View(villaNumberVm);
		}


        public IActionResult Delete(int villaNumberId) 
        {
			VillaNumberVM villaNumberVM = new VillaNumberVM()
			{
				VillaList = _unitOfWork.VillaRepository.GetAll().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
				VillaNumber = _unitOfWork.VillaNumberRepository.Get(item => item.Villa_Number == villaNumberId)
			};
			return View(villaNumberVM);
		}

		[HttpPost]
		public IActionResult Delete(VillaNumberVM villaNumberVm)
		{
            VillaNumber? villaNumberForRemove = _unitOfWork.VillaNumberRepository.Get(item =>
            item.Villa_Number == villaNumberVm.VillaNumber.Villa_Number);

			if (villaNumberForRemove is not null)
			{
				_unitOfWork.VillaNumberRepository.Remove(villaNumberForRemove);
				_unitOfWork.VillaNumberRepository.Save();
                TempData["success"] = "Success! The villa number is deleted successfully.";
                return RedirectToAction("Index");
			}
			TempData["error"] = "Error! The villa number could not be deleted";
			return View(nameof(Index));
		}
	}
}
