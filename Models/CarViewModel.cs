using ChargeIT.Data.DbModels;
using System.ComponentModel.DataAnnotations;

namespace ChargeIT.Models
{
    public class CarViewModel
    {
        public int Id { get; set; }
        [Required]
        public string PlateNumber { get; set; }
        public CarOwnerDbModel CarOwner { get; set; }
        public int? OwnerId { get; set; }
        public List<DropDownViewModel> Owners { get; set; }

    }
}

