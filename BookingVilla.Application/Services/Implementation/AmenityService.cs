using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Services.Interface;
using BookingVilla.Domain.Entities;

namespace BookingVilla.Application.Services.Implementation
{
    public class AmenityService : IAmenityService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AmenityService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void CreateAmenity(Amenity amenity)
        {
            _unitOfWork.AmenityRepository.Add(amenity);
            _unitOfWork.AmenityRepository.Save();
        }

        public bool DeleteAmenity(int id)
        {
            Amenity? amenityForRemove = _unitOfWork.AmenityRepository.Get(x => x.Id == id);

            if (amenityForRemove != null)
            {
                _unitOfWork.AmenityRepository.Remove(amenityForRemove);
                _unitOfWork.AmenityRepository.Save();
                return true;
            }
            return false;
        }

        public IEnumerable<Amenity> GetAllAmenities()
        {
            return _unitOfWork.AmenityRepository.GetAll(includeProperties: "Villa");
        }

        public Amenity GetAmenityById(int id)
        {
            return _unitOfWork.AmenityRepository.Get(x => x.Id == id, includeProperties: "Villa");
        }

        public void UpdateAmenity(Amenity amenity)
        {
            _unitOfWork.AmenityRepository.Update(amenity);
            _unitOfWork.AmenityRepository.Save();
        }
    }
}
