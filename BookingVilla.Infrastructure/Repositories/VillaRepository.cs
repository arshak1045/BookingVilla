using BookingVilla.Application.Common.Interface;
using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;
using BookingVilla.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookingVilla.Infrastructure.Repositories
{
	public class VillaRepository : Repository<Villa>, IVillaRepository
	{
		private readonly ApplicationDbContext _DbContext;

		public VillaRepository(ApplicationDbContext dbContext) : base (dbContext)
		{
			_DbContext = dbContext;
		}
		public void Save()
		{
			_DbContext.SaveChanges();
		}

		public void Update(Villa villa)
		{
			_DbContext.Update(villa);
		}
	}
}
