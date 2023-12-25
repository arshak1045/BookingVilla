using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookingVilla.Application.Common.Interface
{
	public interface IVillaNumberRepository : IRepository<VillaNumber>
	{
		void Update(VillaNumber villa);
		void Save();
	}
}
