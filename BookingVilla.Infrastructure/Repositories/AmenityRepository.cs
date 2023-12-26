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
	public class AmenityRepository : Repository<Amenity>, IAmenityRepository
	{
		private readonly ApplicationDbContext _DbContext;

		public AmenityRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
			_DbContext = dbContext;
		}

		public void Save()
		{
			_DbContext.SaveChanges();
		}

		public void Update(Amenity amenity)
		{
			_DbContext.Update(amenity);
		}
	}
}
