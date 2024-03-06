using BookingVilla.Application.Services.Interface;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingVilla.Controllers
{
	[Authorize]
	public class VillaController : Controller
	{
		private readonly IVillaService _villaService;

        public VillaController(IVillaService villaService)
        {
			_villaService = villaService;
        }

        public IActionResult Index()
		{
			var villas = _villaService.GetAllVillas();
			return View(villas);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Villa villa)
		{
			if (villa.Name == villa.Description)
			{
				ModelState.AddModelError("", "The name and description should be differ");
			}
			if (ModelState.IsValid)
			{
				_villaService.CreateVilla(villa);
				TempData["success"] = "Success! The villa is created successfully.";
				return RedirectToAction(nameof(Index));
			}
			return View();
		}

		public IActionResult Update(int villaId)
		{
			Villa? villa = _villaService.GetVilla(villaId);
			if (villa == null)
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
				_villaService.UpdateVilla(villa);
				TempData["success"] = "Success! The villa is updated successfully.";
				return RedirectToAction(nameof(Index));
			}
			return View(nameof(Index));
		}


		public IActionResult Delete(int villaId)
		{
			Villa? villa = _villaService.GetVilla(villaId);
			if (villa == null)
			{
				return RedirectToAction("Error", "Home");
			}
			return View(villa);
		}

		[HttpPost]
		public IActionResult Delete(Villa villa)
		{
			bool deleted = _villaService.DeleteVilla(villa.Id);

			if (deleted) 
			{
                TempData["success"] = "Success! The villa is deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
			else
			{
                TempData["error"] = "Failed to delete villa.";
            }
			return View();
		}
	}
}
