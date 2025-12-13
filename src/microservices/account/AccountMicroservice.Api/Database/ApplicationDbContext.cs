using AccountMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
