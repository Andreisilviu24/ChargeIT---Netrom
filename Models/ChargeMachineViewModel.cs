using System.ComponentModel.DataAnnotations;

namespace ChargeIT.Models
{
    public class ChargeMachineViewModel
    {
        public int Id { get; set; }
        public Guid Code { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        [Range(1, Int32.MaxValue)]
        public int? Number { get; set; }
        [Range(-180, 180)]
        public double? Longitude { get; set; }
        [Range(-90, 90)]
        public double? Latitude { get; set; }

    }
}
