﻿using BookingVilla.Application.Common.Interface;
using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;

namespace BookingVilla.Infrastructure.Repositories
{
	public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
	{
		private readonly ApplicationDbContext _DbContext;

		public VillaNumberRepository(ApplicationDbContext dbContext) : base (dbContext)
		{
			_DbContext = dbContext;
		}
		public void Save()
		{
			_DbContext.SaveChanges();
		}

		public void Update(VillaNumber villa)
		{
			_DbContext.Update(villa);
		}
	}
}
