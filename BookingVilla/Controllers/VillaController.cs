using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace BookingVilla.Controllers
{
    public class VillaController : Controller
    {

        private readonly ApplicationDbContext _DbContext;

        public VillaController(ApplicationDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public IActionResult Index()
        {
            var villas = _DbContext.Villas.ToList();
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
                _DbContext.Villas.Add(villa);
                _DbContext.SaveChanges();
                TempData["success"] = "Success! The villa is created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Update(int villaId)
        {
            Villa? villa = _DbContext.Villas.FirstOrDefault(x => x.Id == villaId);
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
				_DbContext.Villas.Update(villa);
				_DbContext.SaveChanges();
                TempData["success"] = "Success! The villa is updated successfully.";
                return RedirectToAction(nameof(Index));
			}
			return View(nameof(Index));
		}


        public IActionResult Delete(int villaId) 
        {
			Villa? villa = _DbContext.Villas.FirstOrDefault(x => x.Id == villaId);
			if (villa == null)
			{
				return RedirectToAction("Error", "Home");
			}
			return View(villa);
		}

		[HttpPost]
		public IActionResult Delete(Villa villa)
		{
            Villa? villaFromDb = _DbContext.Villas.FirstOrDefault(x => x.Id == villa.Id);

			if (villaFromDb is not null)
			{
				_DbContext.Villas.Remove(villaFromDb);
				_DbContext.SaveChanges();
                TempData["success"] = "Success! The villa is deleted successfully.";
                return RedirectToAction(nameof(Index));
			}
			return View(nameof(Index));
		}
	}
}
