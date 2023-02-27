using ChargeIT.Data;
using ChargeIT.Data.DbModels;
using ChargeIT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ChargeIT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {

            /*var chargeMachine = new ChargeMachineDbModel
            {
                Code = new Guid("11223344-5566-7788-99AA-BBCCDDEEFF00"),
                City = "Pitesti",
                Street = "Eminescu",
                Number = 33,
                Latitude = 10,
                Longitude = 10
            };

            _dbContext.ChargeMachines.Add(chargeMachine);
            _dbContext.SaveChanges();

            var car = new CarDbModel
            {
                PlateNumber = "MH01ALO"
            };

            _dbContext.Cars.Add(car);
            _dbContext.SaveChanges();

            var booking = new BookingDbModel
            {
                Code = Guid.NewGuid(),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                Car = car,
                ChargeMachine = chargeMachine
            };

            _dbContext.Bookings.Add(booking);
            _dbContext.SaveChanges();*/

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}