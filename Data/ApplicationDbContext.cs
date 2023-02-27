using ChargeIT.Data.DbModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChargeIT.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<CarDbModel> Cars { get; set; }
        public DbSet<ChargeMachineDbModel> ChargeMachines { get; set; }
        public DbSet<BookingDbModel> Bookings { get; set; }
        public DbSet<CarOwnerDbModel> CarOwners { get; set; }
    }
}