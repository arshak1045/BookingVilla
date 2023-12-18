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

        private readonly ApplicationDbContext _DbContext;

        public VillaNumberController(ApplicationDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public IActionResult Index()
        {
            var villaNumbers = _DbContext.VillaNumbers.Include(item => item.Villa).ToList();
            return View(villaNumbers);
        }

        public IActionResult Create() 
        {
            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = _DbContext.Villas.ToList().Select(item => new SelectListItem
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
            bool isVillaNumberExists = _DbContext.VillaNumbers.Any(item => 
                item.Villa_Number == vN.VillaNumber.Villa_Number);

            if(ModelState.IsValid && !isVillaNumberExists)
            {
                _DbContext.VillaNumbers.Add(vN.VillaNumber);
                _DbContext.SaveChanges();
                TempData["success"] = "Success! The villa number is created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (isVillaNumberExists)
            {
                TempData["error"] = "The villa number already exists!";
            }

            vN.VillaList = _DbContext.Villas.ToList().Select(item => new SelectListItem
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
				VillaList = _DbContext.Villas.ToList().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
                VillaNumber = _DbContext.VillaNumbers.FirstOrDefault(item => item.Villa_Number == villaNumberId)
			};
			return View(villaNumberVM);
		}

        [HttpPost]
		public IActionResult Update(VillaNumberVM villaNumberVm)
		{

			if (ModelState.IsValid)
			{
				_DbContext.VillaNumbers.Update(villaNumberVm.VillaNumber);
				_DbContext.SaveChanges();
				TempData["success"] = "Success! The villa number is updated successfully.";
				return RedirectToAction(nameof(Index));
			}

			villaNumberVm.VillaList = _DbContext.Villas.ToList().Select(item => new SelectListItem
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
				VillaList = _DbContext.Villas.ToList().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
				VillaNumber = _DbContext.VillaNumbers.FirstOrDefault(item => item.Villa_Number == villaNumberId)
			};
			return View(villaNumberVM);
		}

		[HttpPost]
		public IActionResult Delete(VillaNumberVM villaNumberVm)
		{
            VillaNumber? villaNumberForRemove = _DbContext.VillaNumbers.FirstOrDefault(item =>
            item.Villa_Number == villaNumberVm.VillaNumber.Villa_Number);

			if (villaNumberForRemove is not null)
			{
				_DbContext.VillaNumbers.Remove(villaNumberForRemove);
				_DbContext.SaveChanges();
                TempData["success"] = "Success! The villa number is deleted successfully.";
                return RedirectToAction("Index");
			}
			TempData["error"] = "Error! The villa number could not be deleted";
			return View(nameof(Index));
		}
	}
}
