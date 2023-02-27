using ChargeIT.Data;
using ChargeIT.Data.DbModels;
using ChargeIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargeIT.Controllers
{
    [Authorize]
    public class CarController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public List<CarViewModel> cars;
        public CarController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public IActionResult Index()
        {
            // var carViewModels = new List<CarViewModel>();

            cars = _applicationDbContext.Cars.Select(c => new CarViewModel
            {
                Id = c.Id,
                PlateNumber = c.PlateNumber,
                OwnerId = c.OwnerId,
                CarOwner = c.Owner,
            }).ToList();

            return View(cars);
        }

        public IActionResult AddCar()
        {
            var carOwners = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
            {
                Id = co.Id,
                Value = $"{co.Name}, {co.Email}"
            }).ToList();

            var car = new CarViewModel
            {
                Owners = carOwners,
            };

            return View(car);
        }

        [HttpPost]

        public IActionResult AddNewCar(CarViewModel carViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("AddCar");
            }

            var isNotUniqueCarNumber = _applicationDbContext.Cars.Any(c => c.Id != carViewModel.Id && c.PlateNumber == carViewModel.PlateNumber);

            if (isNotUniqueCarNumber)
            {
                ModelState.AddModelError(nameof(CarViewModel.PlateNumber), "Plate number already exists!");
                return View("AddCar");
            }

            var carDbModel = new CarDbModel
            {
                PlateNumber = carViewModel.PlateNumber,
                OwnerId = carViewModel.OwnerId,
            };

            _applicationDbContext.Cars.Add(carDbModel);

            var owner2 = _applicationDbContext.CarOwners.FirstOrDefault(co => co.Id == carViewModel.OwnerId);

            owner2.Cars.Add(carDbModel);

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult DeleteCar(int id)
        {
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(cm => cm.Id == id);

            if (existingCar != null)
            {
                _applicationDbContext.Cars.Remove(existingCar);
                _applicationDbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult EditCar(int id)
        {
            var carOwners = _applicationDbContext.CarOwners.Select(co => new DropDownViewModel
            {
                Id = co.Id,
                Value = $"{co.Name}, {co.Email}"
            }).ToList();

            var car = new CarViewModel
            {
                Owners = carOwners,
            };

            var existingCar = _applicationDbContext.Cars.FirstOrDefault(cm => cm.Id == id);

            if (existingCar == null)
            {
                return RedirectToAction("Index");
            }

            var model = new CarViewModel()
            {
                PlateNumber = existingCar.PlateNumber,
                OwnerId = existingCar.OwnerId,
                CarOwner = existingCar.Owner,
                Owners = carOwners,
            };
            
            return View(model);
        }

        [HttpPost]

        public IActionResult EditExistingCar(CarViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditCar", model);
            }

            var existingCar = _applicationDbContext.Cars.FirstOrDefault(cm => cm.Id == model.Id);

            var carWithSamePlateNumber = _applicationDbContext.Cars.FirstOrDefault(c => c.Id != model.Id && c.PlateNumber == model.PlateNumber);

            if (carWithSamePlateNumber == null)
            {
                existingCar.PlateNumber = model.PlateNumber;
                existingCar.OwnerId = model.OwnerId;
                existingCar.Owner = model.CarOwner;
                _applicationDbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("PlateNumber", "There is already a car with this plate number!");

            return View("EditCar");
        }

        /* public IActionResult CarDetails(int id)
         {
             var existingCar = _applicationDbContext.Cars.FirstOrDefault(cm => cm.Id == id);

             if (existingCar == null)
             {
                 return RedirectToAction("Index");
             }

             var model = new CarViewModel()
             {
                 Id = existingCar.Id,
                 PlateNumber = existingCar.PlateNumber,
             };

             return View(model);
         }*/

        public IActionResult CarDetails(CarViewModel carViewModel)
        {
            var existingCar = _applicationDbContext.Cars.FirstOrDefault(c => c.Id == carViewModel.Id);

            if (existingCar == null)
            {
                return RedirectToAction("Index");
            }

            var existingCarViewModel = new CarViewModel
            {
                Id = existingCar.Id,
                PlateNumber = existingCar.PlateNumber,
                OwnerId = existingCar.OwnerId,
                CarOwner = _applicationDbContext.CarOwners.FirstOrDefault(cm => cm.Id == existingCar.OwnerId),
            };

            var carDetailsViewModel = new CarDetailsViewModel
            {
                Car = existingCarViewModel,
                CarId = existingCarViewModel.Id,
                Bookings = new List<BookingViewModel>()
            };

            var bookingsDbModel = _applicationDbContext.Bookings.Where(b => b.CarId == carDetailsViewModel.CarId).ToList();

            foreach (var booking in bookingsDbModel)
            {
                var bookingViewModel = new BookingViewModel
                {
                    Id = booking.Id,
                    Code = booking.Code,
                    CarId = booking.CarId,
                    Car = _applicationDbContext.Cars.FirstOrDefault(c => c.Id == booking.CarId),
                    ChargeMachineId = booking.ChargeMachineId,
                    ChargeMachine = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == booking.ChargeMachineId),
                    StartTime = booking.StartTime,
                    EndTime = booking.EndTime,
                };
                carDetailsViewModel.Bookings.Add(bookingViewModel);
            }

            return View(carDetailsViewModel);
        }
    }

}