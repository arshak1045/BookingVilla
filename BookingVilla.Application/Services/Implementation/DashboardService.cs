using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Application.Services.Interface;
using BookingVilla.Application.Common.DTO;

namespace BookingVilla.Application.Services.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        static int previousMonth = DateTime.Now.Month == 1 ? 12 : DateTime.Now.Month - 1;
        private readonly DateTime previousMonthStartDate = new(DateTime.Now.Year, previousMonth, 1);
        private readonly DateTime currentMonthStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);

        public DashboardService(IUnitOfWork unitOfWork)
        {
             _unitOfWork = unitOfWork;
        }

        public async Task<PieChartDto> GetBookingsPieChartData()
        {
            var totalBookings = _unitOfWork.BookingRepository.GetAll(
                b => b.BookingDate >= DateTime.Now.AddDays(-30) &&
                (b.Status != StaticDetails.BookStatus.StatusPending
                || b.Status == StaticDetails.BookStatus.StatusCanceled));
            var customerWithOneBooking = totalBookings.GroupBy(b => b.UserId).Where(x => x.Count() == 1).Select(x => x.Key).ToList();
            int bookingsByNewCustomer = customerWithOneBooking.Count();
            int bookingsByReturningCustomer = totalBookings.Count() - bookingsByNewCustomer;

            PieChartDto PieChartDto = new PieChartDto()
            {
                Labels = new string[] { "New Customer", "Returning Customer Bookings" },
                Series = new decimal[] { bookingsByNewCustomer, bookingsByReturningCustomer }
            };

            return PieChartDto;
        }

        public async Task<LineChartDto> GetMemberAndBookingLineChartData()
        {
            var bookingData = _unitOfWork.BookingRepository.GetAll(b => b.BookingDate >= DateTime.Now.AddDays(-30) &&
            b.BookingDate.Date <= DateTime.Now).GroupBy(b => b.BookingDate.Date).Select(b =>
            new
            {
                DateTime = b.Key,
                NewBookingCount = b.Count(),
            });

            var userData = _unitOfWork.AppUserRepository.GetAll(u => u.CreatedAt >= DateTime.Now.AddDays(-30) &&
            u.CreatedAt.Date <= DateTime.Now).GroupBy(u => u.CreatedAt.Date).Select(u =>
            new
            {
                DateTime = u.Key,
                NewUserCount = u.Count(),
            });

            var leftJoin = bookingData.GroupJoin(userData, booking => booking.DateTime, user => user.DateTime,
                (booking, user) => new
                {
                    booking.DateTime,
                    booking.NewBookingCount,
                    NewUserCount = user.Select(u => u.NewUserCount).FirstOrDefault()
                });

            var rightJoin = userData.GroupJoin(bookingData, user => user.DateTime, booking => booking.DateTime,
                (user, booking) => new
                {
                    user.DateTime,
                    NewBookingCount = booking.Select(u => u.NewBookingCount).FirstOrDefault(),
                    user.NewUserCount
                });

            var mergedData = leftJoin.Union(rightJoin).OrderBy(x => x.DateTime).ToList();
            var newBookingData = mergedData.Select(x => x.NewBookingCount).ToArray();
            var newUserData = mergedData.Select(x => x.NewUserCount).ToArray();
            var categories = mergedData.Select(x => x.DateTime.ToString("MM/dd/yyyy")).ToArray();

            List<ChartData> chartData = new()
            {
                new ChartData
                {
                    Name = "New Bookings",
                    Data = newBookingData
                },
                new ChartData
                {
                    Name = "New Members",
                    Data = newUserData
                }
            };

            LineChartDto LineChartDto = new()
            {
                Categories = categories,
                Series = chartData
            };

            return LineChartDto;
        }

        public async Task<RadialBarChartDto> GetRegisteredUsersRadialChartData()
        {
            var totalUsers = _unitOfWork.AppUserRepository.GetAll();
            var countByCurrentMonth = totalUsers.Count(
                b => b.CreatedAt >= currentMonthStartDate &&
                b.CreatedAt <= DateTime.Now);
            var countByPreviousMonth = totalUsers.Count(
                b => b.CreatedAt >= previousMonthStartDate &&
                b.CreatedAt <= currentMonthStartDate);
            return GetRadialChartModel(totalUsers.Count(), countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetTotalBookingsRadialChartData()
        {
            var totalBookings = _unitOfWork.BookingRepository.GetAll(
                b => b.Status != StaticDetails.BookStatus.StatusPending
                || b.Status == StaticDetails.BookStatus.StatusCanceled);
            var countByCurrentMonth = totalBookings.Count(
                b => b.BookingDate >= currentMonthStartDate &&
                b.BookingDate <= DateTime.Now);
            var countByPreviousMonth = totalBookings.Count(
                b => b.BookingDate >= previousMonthStartDate &&
                b.BookingDate <= currentMonthStartDate);

            return GetRadialChartModel(totalBookings.Count(), countByCurrentMonth, countByPreviousMonth);
        }

        public async Task<RadialBarChartDto> GetTotalRevenueRadialChartData()
        {
            var totalBookings = _unitOfWork.BookingRepository.GetAll(
                b => b.Status != StaticDetails.BookStatus.StatusPending
                || b.Status == StaticDetails.BookStatus.StatusCanceled);
            var revenue = Convert.ToInt32(totalBookings.Sum(b => b.TotalCost));
            var countByCurrentMonth = totalBookings.Where(
                b => b.BookingDate >= currentMonthStartDate &&
                b.BookingDate <= DateTime.Now).Sum(b => b.TotalCost);
            var countByPreviousMonth = totalBookings.Where(
                b => b.BookingDate >= previousMonthStartDate &&
                b.BookingDate <= currentMonthStartDate).Sum(b => b.TotalCost);

            return GetRadialChartModel(revenue, countByCurrentMonth, countByPreviousMonth);
        }

        private static RadialBarChartDto GetRadialChartModel(int totalCount, double currentMonthCount, double prevMonthCount)
        {
            RadialBarChartDto RadialBarChartDto = new RadialBarChartDto();
            int increaseDecreaseRatio = 100;

            if (prevMonthCount != 0)
            {
                increaseDecreaseRatio = Convert.ToInt32((currentMonthCount - prevMonthCount) / prevMonthCount * 100);
            }

            RadialBarChartDto.TotalCount = totalCount;
            RadialBarChartDto.CountInCurrentMonth = Convert.ToInt32(currentMonthCount);
            RadialBarChartDto.HasRatioIncreased = currentMonthCount > prevMonthCount;
            RadialBarChartDto.Series = new int[] { increaseDecreaseRatio };

            return RadialBarChartDto;
        }
    }
}
