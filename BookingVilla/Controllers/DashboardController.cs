using Microsoft.AspNetCore.Mvc;

namespace BookingVilla.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
