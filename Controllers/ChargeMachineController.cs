using ChargeIT.Data;
using ChargeIT.Data.DbModels;
using ChargeIT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChargeIT.Controllers
{
    [Authorize]
    public class ChargeMachineController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        public ChargeMachineController(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }
        public IActionResult Index()
        {
            var chargeMachineViewModels = _applicationDbContext.ChargeMachines.Select(cm => new ChargeMachineViewModel
            {
                Id = cm.Id,
                City = cm.City,
                Code = cm.Code,
                Latitude = cm.Latitude,
                Longitude = cm.Longitude,
                Number = cm.Number,
                Street = cm.Street,
            }).ToList();

            /*chargeMachines.ForEach(cm =>
            {
                chargeMachineViewModels.Add(new ChargeMachineViewModel
                {
                    Id = cm.Id,
                    City = cm.City,
                    Code = cm.Code,
                    Latitude = cm.Latitude,
                    Longitude = cm.Longitude,
                    Number = cm.Number,
                    Street = cm.Street,
                });
            });*/

            /*foreach(var cm in chargeMachines)
            {
                chargeMachineViewModels.Add(new ChargeMachineViewModel
                {
                    Id = cm.Id,
                    City = cm.City,
                    Code = cm.Code,
                    Latitude = cm.Latitude,
                    Longitude = cm.Longitude,
                    Number = cm.Number,
                    Street = cm.Street,

                });
            }*/

            return View(chargeMachineViewModels);
        }

        public IActionResult AddStation()
        {
            var chargeMachineViewModel = new ChargeMachineViewModel();

            return View(chargeMachineViewModel);
        }

        [HttpPost]

        public IActionResult AddNewStation(ChargeMachineViewModel chargeMachineViewModel)
        {
            /*var existingChargeMachine = _applicationDbContext.ChargeMachines
                .FirstOrDefault(cm => cm.Code == chargeMachineViewModel.Code);

            if(existingChargeMachine != null)
            {
                throw new Exception("There is already a charge machine with the same code!");
            }*/

            if (!ModelState.IsValid)
            {
                return View("AddStation", chargeMachineViewModel);
            }

            _applicationDbContext.ChargeMachines.Add(new ChargeMachineDbModel
            {
                City = chargeMachineViewModel.City,
                Code = Guid.NewGuid(),
                Latitude = chargeMachineViewModel.Latitude.Value,
                Longitude = chargeMachineViewModel.Longitude.Value,
                Number = chargeMachineViewModel.Number.Value,
                Street = chargeMachineViewModel.Street,
            });

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        //[HttpPost("ChargeMachine/DeleteStation/{ChargeMachineId}")]        

        public IActionResult DeleteStation(int id)
        {
            var existingChargeMachine = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == id);

            if (existingChargeMachine != null)
            {
                _applicationDbContext.ChargeMachines.Remove(existingChargeMachine);
                _applicationDbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult EditStation(int id)
        {
            var existingChargeMachine = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == id);

            if (existingChargeMachine == null)
            {
                return RedirectToAction("Index");
            }

            var model = new ChargeMachineViewModel()
            {
                City = existingChargeMachine.City,
                Code = existingChargeMachine.Code,
                Latitude = existingChargeMachine.Latitude,
                Longitude = existingChargeMachine.Longitude,
                Number = existingChargeMachine.Number,
                Street = existingChargeMachine.Street,
            };

            return View(model);
        }

        [HttpPost]

        public IActionResult EditExistingStation(ChargeMachineViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("EditStation", model);
            }

            var existingStation = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == model.Id);

            existingStation.City = model.City;
            existingStation.Street = model.Street;
            existingStation.Number = model.Number.Value;
            existingStation.Latitude = model.Latitude.Value;
            existingStation.Longitude = model.Longitude.Value;

            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult StationDetails(ChargeMachineViewModel chargeMachineViewModel)
        {
            var existingChargingMachine = _applicationDbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == chargeMachineViewModel.Id);

            if (existingChargingMachine == null)
            {
                return RedirectToAction("Index");
            }

            var existingChargingMachineViewModel = new ChargeMachineViewModel
            {
                Id = existingChargingMachine.Id,
                Code = existingChargingMachine.Code,
                City = existingChargingMachine.City,
                Street = existingChargingMachine.Street,
                Number = existingChargingMachine.Number,
                Longitude = existingChargingMachine.Longitude,
                Latitude = existingChargingMachine.Latitude,
            };

            var chargingMachineDetailsViewModel = new ChargeMachineDetailsViewModel
            {
                ChargeMachine = existingChargingMachineViewModel,
                ChargeMachineId = existingChargingMachineViewModel.Id,
                Bookings = new List<BookingViewModel>(),
            };

            var bookingsDbModel = _applicationDbContext.Bookings.Where(b => b.ChargeMachineId == existingChargingMachineViewModel.Id).ToList();

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
                chargingMachineDetailsViewModel.Bookings.Add(bookingViewModel);
            }

            return View(chargingMachineDetailsViewModel);
        }
    }
}