using AccountMicroservice.Api.Database;
using AccountMicroservice.Api.Models.Business;
using Microsoft.EntityFrameworkCore;

namespace AccountMicroservice.Api.Services.User_services
{
    public class UserService(ApplicationDbContext context) : IUserService
    {
        public async Task<User?> GetUserByIdAsync(Guid userId)
            => await context.Users.SingleOrDefaultAsync(x => x.Id == userId);

        public async Task<User?> GetUserByEmailAsync(string email)
            => await context.Users.SingleOrDefaultAsync(x => x.Email == email);

        public async Task AddUserAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            context.Users.Update(user);
            await context.SaveChangesAsync();
        }
    }
}
