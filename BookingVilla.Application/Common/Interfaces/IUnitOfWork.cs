using BookingVilla.Application.Common.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingVilla.Application.Common.Interfaces
{
	public interface IUnitOfWork
	{
		IVillaRepository VillaRepository { get; }
		IVillaNumberRepository VillaNumberRepository { get; }
		IAmenityRepository AmenityRepository { get; }
		IBookingRepository BookingRepository { get; }
		IAppUserRepository AppUserRepository { get; }
	}
}
