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
                new Role{Id = new Guid("3d2dc356-2996-4616-aa2b-64ebc93e7d8f"), Name = RoleNames.User},
                new Role{Id = new Guid("7af2e6a9-a998-4103-aeab-9ee9cf00fb0a"), Name = RoleNames.Verified},
                new Role{Id = new Guid("32d6dd4f-15bd-4ef4-b3f4-733425778126"), Name = RoleNames.Admin},
                new Role{Id = new Guid("5b69c81c-3a6e-4fad-b636-c9af92671a84"), Name = RoleNames.Moderator});

            builder.Entity<User>()
                .HasMany(x => x.Roles)
                .WithMany()
                .UsingEntity<UserRole>();
        }
    }
}
