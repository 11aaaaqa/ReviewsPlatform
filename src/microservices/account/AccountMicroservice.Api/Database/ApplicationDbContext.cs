using AccountMicroservice.Api.Constants;
using AccountMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().HasIndex(x => x.Email).IsUnique();
            builder.Entity<User>().HasIndex(x => x.UserName).IsUnique();

            builder.Entity<Role>().HasIndex(x => x.Name).IsUnique();

            builder.Entity<Role>().HasData(
                new Role{Id = new Guid(RoleIds.UserId), Name = RoleNames.User},
                new Role{Id = new Guid(RoleIds.VerifiedId), Name = RoleNames.Verified},
                new Role{Id = new Guid(RoleIds.AdminId), Name = RoleNames.Admin},
                new Role{Id = new Guid(RoleIds.ModeratorId), Name = RoleNames.Moderator});

            builder.Entity<User>()
                .HasMany(x => x.Roles)
                .WithMany()
                .UsingEntity<UserRole>();
        }
    }
}
