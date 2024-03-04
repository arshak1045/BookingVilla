using BookingVilla.Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BookingVilla.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetTotalBookingsRadialChartData()
        {
            return Json(await _dashboardService.GetTotalBookingsRadialChartData());
        }

        public async Task<IActionResult> GetRegisteredUsersRadialChartData()
        {
            return Json( await _dashboardService.GetRegisteredUsersRadialChartData());
        }

        public async Task<IActionResult> GetTotalRevenueRadialChartData()
        {
            return Json(await _dashboardService.GetTotalRevenueRadialChartData());
        }

		public async Task<IActionResult> GetBookingsPieChartData()
		{
			return Json(await _dashboardService.GetBookingsPieChartData());
		}

        public async Task<IActionResult> GetMemberAndBookingLineChartData()
        {
            return Json(await _dashboardService.GetMemberAndBookingLineChartData());
        }
    }
}
