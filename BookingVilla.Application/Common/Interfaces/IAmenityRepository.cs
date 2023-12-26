using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;

namespace BookingVilla.Application.Common.Interface
{
	public interface IAmenityRepository : IRepository<Amenity>
	{
		void Update(Amenity amenity);
		void Save();
	}
}
