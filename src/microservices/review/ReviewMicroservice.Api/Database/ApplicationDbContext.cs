using Microsoft.EntityFrameworkCore;
using ReviewMicroservice.Api.Models.Business;

namespace ReviewMicroservice.Api.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewReaction> ReviewReactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReviewReaction>().HasKey(x => new { x.ReviewId, x.UserId });
            builder.Entity<ReviewReaction>()
                .HasOne<Review>()
                .WithMany()
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
