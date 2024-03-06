using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Application.Common.Utility;
using BookingVilla.Application.Services.Interface;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Hosting;

namespace BookingVilla.Application.Services.Implementation
{
    public class VillaService : IVillaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaService(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public void CreateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"Images\VillaImages");

                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                {
                    villa.Image.CopyTo(fileStream);
                    villa.ImageUrl = $"\\Images\\VillaImages\\{fileName}";
                }
            }
            else
            {
                villa.ImageUrl = "https://placehold.co/600x400";
            }
            _unitOfWork.VillaRepository.Add(villa);
            _unitOfWork.VillaRepository.Save();
        }

        public bool DeleteVilla(int id)
        {
            Villa? villaFromDb = _unitOfWork.VillaRepository.Get(x => x.Id == id);

            if (villaFromDb is not null)
            {
                if (!string.IsNullOrEmpty(villaFromDb.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villaFromDb.ImageUrl.TrimStart('\\'));

                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.VillaRepository.Remove(villaFromDb);
                _unitOfWork.VillaRepository.Save();
                return true;
            }
            return false;
        }

        public IEnumerable<Villa> GetAllVillas()
        {
            return _unitOfWork.VillaRepository.GetAll(includeProperties: "Amenities");
        }

        public Villa GetVilla(int id)
        {
            return _unitOfWork.VillaRepository.Get(x => x.Id == id, includeProperties: "Amenities");
        }

        public void UpdateVilla(Villa villa)
        {
            if (villa.Image != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"Images\VillaImages");

                if (!string.IsNullOrEmpty(villa.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                    if (File.Exists(oldImagePath))
                    {
                        File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                {
                    villa.Image.CopyTo(fileStream);
                    villa.ImageUrl = $"\\Images\\VillaImages\\{fileName}";
                }
            }
            _unitOfWork.VillaRepository.Update(villa);
            _unitOfWork.VillaRepository.Save();
        }

        public IEnumerable<Villa> GetAllAvailableVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.VillaRepository.GetAll(includeProperties: "Amenities");
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(b => b.Status == StaticDetails.BookStatus.StatusApproved ||
            b.Status == StaticDetails.BookStatus.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {
                int roomAvailable = StaticDetails.VillaNumberAvailability_Count
                    (villa.Id, villaNumbersList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomAvailable > 0 ? true : false;
            }
            return villaList;
        }

        bool IVillaService.IsAvailableVillaByDate(int villaId, int nights, DateOnly checkInDate)
        {
            var villaNumbersList = _unitOfWork.VillaNumberRepository.GetAll().ToList();
            var bookedVillas = _unitOfWork.BookingRepository.GetAll(b => b.Status == StaticDetails.BookStatus.StatusApproved ||
            b.Status == StaticDetails.BookStatus.StatusCheckedIn).ToList();
            int roomAvailable = StaticDetails.VillaNumberAvailability_Count
                (villaId, villaNumbersList, checkInDate, nights, bookedVillas);
            return roomAvailable > 0 ? true : false;
        }
    }
}
