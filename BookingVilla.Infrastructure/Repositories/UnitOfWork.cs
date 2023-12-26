using BookingVilla.Application.Common.Interface;
using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Infrastructure.Data;

namespace BookingVilla.Infrastructure.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;
		public IVillaRepository VillaRepository { get; private set; }
        public IAmenityRepository AmenityRepository { get; private set; }
        public IVillaNumberRepository VillaNumberRepository { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            VillaRepository = new VillaRepository(context);
            VillaNumberRepository = new VillaNumberRepository(context);
            AmenityRepository = new AmenityRepository(context);
        }
    }
}
