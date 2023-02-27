namespace ChargeIT.Models
{
    public class ChargeMachineDetailsViewModel
    {
        public List<BookingViewModel> Bookings { get; set; }
        public ChargeMachineViewModel ChargeMachine { get; set; }
        public int ChargeMachineId { get; set; }
    }
}
