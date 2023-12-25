using BookingVilla.Application.Common.Interfaces;
using BookingVilla.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BookingVilla.Controllers
{
	public class VillaController : Controller
	{

		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		
		public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			var villas = _unitOfWork.VillaRepository.GetAll();
			return View(villas);
		}

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		public IActionResult Create(Villa villa)
		{
			if (villa.Name == villa.Description)
			{
				ModelState.AddModelError("", "The name and description should be differ");
			}

			if (ModelState.IsValid)
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
				TempData["success"] = "Success! The villa is created successfully.";
				return RedirectToAction(nameof(Index));
			}
			return View();
		}

		public IActionResult Update(int villaId)
		{
			Villa? villa = _unitOfWork.VillaRepository.Get(x => x.Id == villaId);
			if (villa == null)
			{
				return RedirectToAction("Error", "Home");
			}
			return View(villa);
		}

		[HttpPost]
		public IActionResult Update(Villa villa)
		{
			if (ModelState.IsValid && villa.Id > 0)
			{
				if (villa.Image != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
					string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"Images\VillaImages");

					if (!string.IsNullOrEmpty(villa.ImageUrl))
					{
						var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
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
				TempData["success"] = "Success! The villa is updated successfully.";
				return RedirectToAction(nameof(Index));
			}
			return View(nameof(Index));
		}


		public IActionResult Delete(int villaId)
		{
			Villa? villa = _unitOfWork.VillaRepository.Get(x => x.Id == villaId);
			if (villa == null)
			{
				return RedirectToAction("Error", "Home");
			}
			return View(villa);
		}

		[HttpPost]
		public IActionResult Delete(Villa villa)
		{
			Villa? villaFromDb = _unitOfWork.VillaRepository.Get(x => x.Id == villa.Id);

			if (villaFromDb is not null)
			{
				if (!string.IsNullOrEmpty(villaFromDb.ImageUrl))
				{
					var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villaFromDb.ImageUrl.TrimStart('\\'));

					if (System.IO.File.Exists(oldImagePath))
					{
						System.IO.File.Delete(oldImagePath);
					}
				}

				_unitOfWork.VillaRepository.Remove(villaFromDb);
				_unitOfWork.VillaRepository.Save();
				TempData["success"] = "Success! The villa is deleted successfully.";
				return RedirectToAction(nameof(Index));
			}
			return View(nameof(Index));
		}
	}
}
