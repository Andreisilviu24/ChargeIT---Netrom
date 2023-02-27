using System.ComponentModel.DataAnnotations;

namespace ChargeIT.Models
{
    public class AddBookingViewModel
    {
        public List<DropDownViewModel> ChargeMachines { get; set; }
        public List<DropDownViewModel> Cars { get; set; }
        [Display(Name = "Charge Machine")]
        [Required(ErrorMessage = "Please select a valid charge machine!")]
        public int? ChargeMachineId { get; set; }
        [Required(ErrorMessage = "Please select a valid car!")]
        [Display(Name = "Car")]
        public int? CarId { get; set; }
        [Required]
        public DateTime? StartTime { get; set; }
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
        [Required(ErrorMessage = "Please select a valid interval!")]
        [Display(Name = "Available Intervals")]
        public int? IntervalHour { get; set; }
    }
}
