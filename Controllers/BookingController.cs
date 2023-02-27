using ChargeIT.Data;
using ChargeIT.Data.DbModels;
using ChargeIT.Models;
using ChargeIT.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Net;
using System.Net.Mail;

namespace ChargeIT.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmailSettings _emailSettings;
        private readonly List<int> _totalAvailableHours = new List<int>();
        private const int TotalAvailableHoursInADay = 24;

        public BookingController(ApplicationDbContext dbContext, EmailSettings emailSettings)
        {
            _dbContext = dbContext;
            _emailSettings = emailSettings;
            for (var hour = 0; hour < TotalAvailableHoursInADay; hour++)
            {
                _totalAvailableHours.Add(hour);
            }
        }
        public IActionResult Index()
        {
            var chargeMachineViewModels = _dbContext.ChargeMachines.Select(cm => new DropDownViewModel
            {
                Id = cm.Id,
                Value = $"{cm.Code}, {cm.City}, {cm.Number}, {cm.Latitude}, {cm.Longitude}",
                Latitude = cm.Latitude,
                Longitude = cm.Longitude,
            }).ToList();

            var carViewModels = _dbContext.Cars.Select(c => new DropDownViewModel
            {
                Id = c.Id,
                Value = c.PlateNumber
            }).ToList();

            var bookingViewModel = new AddBookingViewModel
            {
                ChargeMachines = chargeMachineViewModels,
                Cars = carViewModels
            };

            return View(bookingViewModel);
        }

        [HttpPost]
        public IActionResult SaveBooking(AddBookingViewModel addBookingViewModel)
        {
            if (ModelState.IsValid)
            {

                var startTime = addBookingViewModel.StartTime.Value.AddHours(addBookingViewModel.IntervalHour.Value);

                if (_dbContext.Bookings.FirstOrDefault(b => b.ChargeMachineId == addBookingViewModel.ChargeMachineId && b.StartTime == startTime) != null)
                {
                    ModelState.AddModelError(nameof(addBookingViewModel.IntervalHour), "There is an already allocated interval for the selected machine for the selected interval!");
                }

                if (_dbContext.Bookings.FirstOrDefault(b => b.CarId == addBookingViewModel.CarId && b.StartTime == startTime) != null)
                {
                    ModelState.AddModelError(nameof(addBookingViewModel.IntervalHour), "There is an already allocated interval for the selected car for the selected interval!");
                }


                if (ModelState.IsValid)
                {
                    var bookingDbModel = _dbContext.Bookings.Add(new BookingDbModel
                    {
                        ChargeMachineId = addBookingViewModel.ChargeMachineId.Value,
                        ChargeMachine = _dbContext.ChargeMachines.FirstOrDefault(cm => cm.Id == addBookingViewModel.ChargeMachineId),
                        CarId = addBookingViewModel.CarId.Value,
                        Car = _dbContext.Cars.FirstOrDefault(c => c.Id == addBookingViewModel.CarId),
                        Code = Guid.NewGuid(),
                        StartTime = startTime,
                        EndTime = startTime.AddMinutes(59).AddSeconds(59)
                    });

                    _dbContext.SaveChanges();
                    SendEmailToTheUser(bookingDbModel.Entity.Id);
                }
            }

            if (!ModelState.IsValid)
            {
                var chargeMachineViewModels = _dbContext.ChargeMachines.Select(cm => new DropDownViewModel
                {
                    Id = cm.Id,
                    Value = $"{cm.Code}, {cm.City}, {cm.Number}"
                }).ToList();

                var carViewModels = _dbContext.Cars.Select(c => new DropDownViewModel
                {
                    Id = c.Id,
                    Value = c.PlateNumber
                }).ToList();

                addBookingViewModel.Cars = carViewModels;
                addBookingViewModel.ChargeMachines = chargeMachineViewModels;
                addBookingViewModel.IntervalHour = null;

                return View("Index", addBookingViewModel);
            }
            return View("Success");

            /*var timeStart = new DateTime(addBookingViewModel.StartTime.Value.Year, addBookingViewModel.StartTime.Value.Month,
                addBookingViewModel.StartTime.Value.Day, addBookingViewModel.IntervalHour.Value, 0, 0);*/
        }

        private void SendEmailToTheUser(int bookingId)
        {
            var booking = _dbContext.Bookings
                .Include(b => b.ChargeMachine)
                .Include(b => b.Car)
                .ThenInclude(c => c.Owner)
                .FirstOrDefault(b => b.Id == bookingId);

            var qrCode = GetBookingQRCode(booking.Code);

            var emailBody = @$"<h3>A new order has been created for your car: {booking.Car.PlateNumber}</h3>
                <p><b>Order number: </b>{booking.Code}</p>
                <p><b>Interval: </b>{booking.StartTime.ToString("yyyy-MM-dd HH:mm")} - {booking.EndTime.ToString("yyyy-MM-dd HH:mm")}</p>
                <p><b>Charge machine code: </b>{booking.ChargeMachine.Code}</p>
                <p><b>City: </b>{booking.ChargeMachine.City}</p>
                <p><b>Street: </b>{booking.ChargeMachine.Street}</p>
                <p><b>Number: </b>{booking.ChargeMachine.Number}</p>
            ";

            var message = new MailMessage();
            message.From = new MailAddress(_emailSettings.EmailAddress);
            message.To.Add(booking.Car.Owner.Email);
            message.Subject = "A new booking was created for you";
            message.IsBodyHtml = true;
            message.Body = emailBody;

            using (MemoryStream ms = new MemoryStream(qrCode))
            {
                message.Attachments.Add(new Attachment(ms, $"{booking.Code}.png"));

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_emailSettings.EmailAddress, _emailSettings.AppPassword),
                    EnableSsl = true,
                };

                smtpClient.Send(message);
            }
        }

        private byte[] GetBookingQRCode(Guid code)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(code.ToString(), QRCodeGenerator.ECCLevel.Q);
            var bitmapByteQRCode = new BitmapByteQRCode(qrCodeData);
            var encodedQrCode = bitmapByteQRCode.GetGraphic(5);
            return encodedQrCode;
        }

        [HttpGet("Bookings/GetAvailableIntervals")]
        public ActionResult<List<int>> GetAvailableIntervals(int chargeMachineId, DateTime date)
        {
            var notAvailableHours = _dbContext.Bookings.Where(b => b.ChargeMachineId == chargeMachineId &&
                b.StartTime >= date && b.StartTime <= date.AddHours(23).AddMinutes(59).AddMinutes(59)).Select(b =>
                b.StartTime.Hour).ToList();

            var currentDate = DateTime.Now;

            var totalAvailableHours = _totalAvailableHours;

            if (date.Date == DateTime.Now.Date)
            {
                var currentHour = currentDate.Hour;

                totalAvailableHours = totalAvailableHours.Where(tav => tav > currentHour).ToList();
            }

            var availableHours = totalAvailableHours.Except(notAvailableHours).ToList();

            return availableHours;
        }

        public IActionResult DeleteStationBooking(int id)
        {
            var now = DateTime.Now;

            var existingBooking = _dbContext.Bookings.FirstOrDefault(cm => cm.Id == id);

            var chargeMachineId = existingBooking.ChargeMachineId;

            var startTime = existingBooking.StartTime;

            if (existingBooking != null && startTime > now)
            {
                _dbContext.Bookings.Remove(existingBooking);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("StationDetails", "ChargeMachine", new { id = chargeMachineId });
        }

        public IActionResult DeleteCarBooking(int id)
        {
            var now = DateTime.Now;

            var existingBooking = _dbContext.Bookings.FirstOrDefault(cm => cm.Id == id);

            var carId = existingBooking.CarId;

            var startTime = existingBooking.StartTime;

            if (existingBooking != null && startTime > now)
            {
                _dbContext.Bookings.Remove(existingBooking);
                _dbContext.SaveChanges();
            }

            return RedirectToAction("CarDetails", "Car", new { id = carId });
        }

    }
}
