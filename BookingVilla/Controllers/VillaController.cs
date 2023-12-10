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
                return RedirectToAction("Index", "Villa");
            }
            return View();
        }
    }
}
