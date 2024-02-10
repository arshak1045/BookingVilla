using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static BookingVilla.Application.Common.Utility.StaticDetails;

namespace BookingVilla.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(ApplicationDbContext db,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

                if (!_roleManager.RoleExistsAsync(Roles.Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(Roles.Admin)).Wait();
                    _roleManager.CreateAsync(new IdentityRole(Roles.Customer)).Wait();

                    _userManager.CreateAsync(new AppUser
                    {
                        UserName = "adminBookingVilla",
                        Email = "adminBookingVilla@yopmail.com",
                        Name = "Admin Admin",
                        NormalizedUserName = "adminbookingvilla",
                        NormalizedEmail = "adminbookingvilla@yopmail.com",
                        PhoneNumber = "1234567890",
                    }, "Admin123!").GetAwaiter().GetResult();

                    AppUser user = _db.AppUsers.FirstOrDefault(u => u.Email == "adminBookingVilla@yopmail.com");
                    _userManager.AddToRoleAsync(user, Roles.Admin).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}

