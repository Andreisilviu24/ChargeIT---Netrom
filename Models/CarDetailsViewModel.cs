namespace ChargeIT.Models
{
    public class CarDetailsViewModel
    {
        public List<BookingViewModel> Bookings { get; set; }
        public CarViewModel Car { get; set; }
        public int CarId { get; set; }
    }
}
