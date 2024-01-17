using BookingVilla.Application.Common.Interface;
using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVilla.Infrastructure.Repositories
{
	public class AppUserRepository : Repository<AppUser>, IAppUserRepository
	{
		private readonly ApplicationDbContext _DbContext;

		public AppUserRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_DbContext = dbContext;
		}
	}
}
