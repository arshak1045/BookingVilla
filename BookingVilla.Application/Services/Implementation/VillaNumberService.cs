using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Services.Interface;
using BookingVilla.Domain.Entities;

namespace BookingVilla.Application.Services.Implementation
{
    public class VillaNumberService : IVillaNumberService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateVillaNumber(VillaNumber number)
        {
            bool isVillaNumberExists = _unitOfWork.VillaNumberRepository.Any(item =>
                item.Villa_Number == number.Villa_Number);

            if (!isVillaNumberExists) 
            {
                _unitOfWork.VillaNumberRepository.Add(number);
                _unitOfWork.VillaNumberRepository.Save();
            }
        }

        public bool DeleteVillaNumber(int number)
        {
            VillaNumber? villaNumberForRemove = _unitOfWork.VillaNumberRepository.Get(item =>
            item.Villa_Number == number);

            if (villaNumberForRemove != null)
            {
                _unitOfWork.VillaNumberRepository.Remove(villaNumberForRemove);
                _unitOfWork.VillaNumberRepository.Save();
                return true;
            }
            return false;
        }

        public IEnumerable<VillaNumber> GetAllVillaNumbers()
        {
           return _unitOfWork.VillaNumberRepository.GetAll(includeProperties: "Villa");
        }

        public VillaNumber GetVillaNumber(int number)
        {
            return _unitOfWork.VillaNumberRepository.Get(x => x.Villa_Number == number, includeProperties: "Villa");
        }

        public void UpdateVillaNumber(VillaNumber villaNumber)
        {
            _unitOfWork.VillaNumberRepository.Update(villaNumber);
            _unitOfWork.VillaNumberRepository.Save();
        }
    }
}
