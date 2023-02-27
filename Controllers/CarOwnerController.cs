using ChargeIT.Data;
using ChargeIT.Data.DbModels;
using ChargeIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChargeIT.Controllers
{
    [Authorize]
    public class CarOwnerController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public List<CarOwnerViewModel> owners;

        public CarOwnerController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public IActionResult Index()
        {
            owners = _applicationDbContext.CarOwners.Select(c => new CarOwnerViewModel
            {
                Name = c.Name,
                Email = c.Email,
                Id = c.Id,
                Cars = c.Cars
            }).ToList();

            return View(owners);
        }

        public IActionResult AddCarOwner()
        {
            var carOwnerViewModel = new CarOwnerViewModel();

            return View(carOwnerViewModel);
        }

        [HttpPost]

        public IActionResult AddNewCarOwner(CarOwnerViewModel carOwnerViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddCarOwner", carOwnerViewModel);
            }

            var isNotUniqueCarNumber = _applicationDbContext.CarOwners.Any(c => c.Id != carOwnerViewModel.Id && c.Name == carOwnerViewModel.Name);

            if (isNotUniqueCarNumber)
            {
                ModelState.AddModelError(nameof(CarOwnerViewModel.Name), "Name already exists!");
                return View("AddCarOwner", carOwnerViewModel);
            }

            _applicationDbContext.CarOwners.Add(new CarOwnerDbModel
            {
                 Name = carOwnerViewModel.Name,
                 Email = carOwnerViewModel.Email,
                 Cars = carOwnerViewModel.Cars,
            });

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult DeleteCarOwner(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(cm => cm.Id == id);

            var existingCars = _applicationDbContext.Cars.Where(c => c.OwnerId == id).ToList();

            if (existingCarOwner != null)
            {
                foreach(var car in existingCars)
                {
                    _applicationDbContext.Cars.Remove(car);
                }
                _applicationDbContext.CarOwners.Remove(existingCarOwner);
                _applicationDbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult EditCarOwner(int id)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(cm => cm.Id == id);

            if (existingCarOwner == null)
            {
                return RedirectToAction("Index");
            }

            var model = new CarOwnerViewModel()
            {
                Name = existingCarOwner.Name,
                Email = existingCarOwner.Email,
                Cars = existingCarOwner.Cars
            };

            return View(model);
        }

        [HttpPost]

        public IActionResult EditExistingCarOwner(CarOwnerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCarOwner", model);
            }

            var existingCarOwner = _applicationDbContext.CarOwners.FirstOrDefault(cm => cm.Id == model.Id);

            var carOwnerWithSameName = _applicationDbContext.CarOwners.FirstOrDefault(c => c.Id != model.Id && c.Name == model.Name);

            if (carOwnerWithSameName == null)
            {
                existingCarOwner.Email = model.Email;
                existingCarOwner.Cars = model.Cars;
                existingCarOwner.Name = model.Name;
                _applicationDbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("Name", "There is already a car owner with this name!");

            return View("EditCarOwner");
        }

        public IActionResult CarOwnerDetails(CarOwnerViewModel carOwnerViewModel)
        {
            var existingCarOwner = _applicationDbContext.CarOwners.Include(c => c.Cars).FirstOrDefault(c => c.Id == carOwnerViewModel.Id);

            if (existingCarOwner == null)
            {
                return RedirectToAction("Index");
            }

            var existingCarOwnerViewModel = new CarOwnerViewModel
            {
                Id = existingCarOwner.Id,
                Name = existingCarOwner.Name,
                Email = existingCarOwner.Email,
                Cars = existingCarOwner.Cars,
            };

            return View(existingCarOwnerViewModel);
        }
    }
}
