using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TravelLogger.Models.Data
{
    public class TravelLoggerContext : IdentityDbContext<IdentityUser>
    {
        public TravelLoggerContext(DbContextOptions<TravelLoggerContext> options) : base(options)
        {
        }

        public DbSet<TravelLog> TravelLogs { get; set; }
    }
}
