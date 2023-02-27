using Microsoft.AspNetCore.Mvc;

namespace ChargeIT.Models
{
    public class DropDownViewModel
    {
        public int Id { get; set; }
        public string Value { get; set; } 
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
