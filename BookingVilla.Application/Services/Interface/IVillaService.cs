using BookingVilla.Domain.Entities;

namespace BookingVilla.Application.Services.Interface
{
    public interface IVillaService
    {
        IEnumerable<Villa> GetAllVillas();
        Villa GetVilla(int id);
        void CreateVilla(Villa villa);
        void UpdateVilla(Villa villa);
        bool DeleteVilla(int id);
        IEnumerable<Villa> GetAllAvailableVillasByDate(int nights, DateOnly checkInDate);
        bool IsAvailableVillaByDate(int villaId, int nights, DateOnly checkInDate);
    }
}
