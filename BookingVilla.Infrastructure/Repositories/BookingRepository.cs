using BookingVilla.Application.Common.Interface;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVilla.Infrastructure.Repositories
{
	public class BookingRepository : Repository<Booking>, IBookingRepository
	{
		private readonly ApplicationDbContext _DbContext;

		public BookingRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_DbContext = dbContext;
		}

		public void Save()
		{
			_DbContext.SaveChanges();
		}

		public void Update(Booking booking)
		{
			_DbContext.Update(booking);
		}

		
	}
}
