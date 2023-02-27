using ChargeIT.Data.DbModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ChargeIT.Models
{
    public class CarOwnerViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public ICollection<CarDbModel> Cars { get; set; }    
    }
}
