using BookingVilla.Domain.Entities;

namespace BookingVilla.Application.Services.Interface
{
    public interface IVillaNumberService
    {
        IEnumerable<VillaNumber> GetAllVillaNumbers();
        VillaNumber GetVillaNumber(int number);
        void CreateVillaNumber(VillaNumber number);
        void UpdateVillaNumber(VillaNumber villaNumber);
        bool DeleteVillaNumber(int number);
    }
}
