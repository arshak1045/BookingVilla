using BookingVilla.Application.Services.Interface;
using BookingVilla.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookingVilla.Controllers
{
    public class VillaNumberController : Controller
    {
		private readonly IVillaNumberService _villaNumberService;
        private readonly IVillaService _villaService;

        public VillaNumberController(IVillaNumberService villaNumberService, IVillaService villaService)
        {
            _villaNumberService = villaNumberService;
            _villaService = villaService;
        }

        public IActionResult Index()
        {
            var villaNumbers = _villaNumberService.GetAllVillaNumbers();
            return View(villaNumbers);
        }

        public IActionResult Create() 
        {
            VillaNumberVM villaNumberVM = new VillaNumberVM()
            {
                VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
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
            if(ModelState.IsValid)
            {
                _villaNumberService.CreateVillaNumber(vN.VillaNumber);
                TempData["success"] = "Success! The villa number is created successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "The villa number already exists!";
            }

            vN.VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
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
				VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
                VillaNumber = _villaNumberService.GetVillaNumber(villaNumberId)
			};
			return View(villaNumberVM);
		}

        [HttpPost]
		public IActionResult Update(VillaNumberVM villaNumberVm)
		{
			if (ModelState.IsValid)
			{
				_villaNumberService.UpdateVillaNumber(villaNumberVm.VillaNumber);
				TempData["success"] = "Success! The villa number is updated successfully.";
				return RedirectToAction(nameof(Index));
			}

			villaNumberVm.VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
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
				VillaList = _villaService.GetAllVillas().Select(item => new SelectListItem
				{
					Text = item.Name,
					Value = item.Id.ToString()
				}),
				VillaNumber = _villaNumberService.GetVillaNumber(villaNumberId)
			};
			return View(villaNumberVM);
		}

		[HttpPost]
		public IActionResult Delete(VillaNumberVM villaNumberVm)
		{
			if (_villaNumberService.DeleteVillaNumber(villaNumberVm.VillaNumber.Villa_Number))
			{
                TempData["success"] = "Success! The villa number is deleted successfully.";
                return RedirectToAction("Index");
			}
			TempData["error"] = "Error! The villa number could not be deleted";
			return View(nameof(Index));
		}
	}
}
