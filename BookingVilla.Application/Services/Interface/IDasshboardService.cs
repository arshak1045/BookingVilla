using BookingVilla.Application.Common.DTO;

namespace BookingVilla.Application.Services.Interface
{
    public interface IDashboardService
    {
        Task<RadialBarChartDto> GetTotalBookingsRadialChartData();
        Task<RadialBarChartDto> GetRegisteredUsersRadialChartData();
        Task<RadialBarChartDto> GetTotalRevenueRadialChartData();
        Task<PieChartDto> GetBookingsPieChartData();
        Task<LineChartDto> GetMemberAndBookingLineChartData();
    }
}