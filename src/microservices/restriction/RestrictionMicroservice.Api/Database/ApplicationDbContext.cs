using Microsoft.EntityFrameworkCore;
using RestrictionMicroservice.Api.Models.Business;

namespace RestrictionMicroservice.Api.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Report> Reports { get; set; }
        public DbSet<Restriction> Restrictions { get; set; }
    }
}
